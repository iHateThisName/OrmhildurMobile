using UnityEngine;
using System.Collections.Generic;

public class TrackTile : TileEntityBase, IScannable
{
    // STATIC DICTIONARIES:
    public static Dictionary<Vector2Int, int> TrackDistances = new Dictionary<Vector2Int, int>();
    // NEW: Maps a track's grid position to the CreatureShape that made it
    public static Dictionary<Vector2Int, CreatureShape> TrackSources = new Dictionary<Vector2Int, CreatureShape>();

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip trackFoundClip;

    [Header("Fallback Visuals")]
    [Tooltip("These will only be used if the CreatureShape doesn't have custom sprites assigned.")]
    [SerializeField] private Sprite fallbackHintSprite;
    [SerializeField] private Sprite fallbackDugSprite;

    private bool hasBeenDug = false;
    private Vector2Int currentGridPos;

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        currentGridPos = gridPosition;
        hasBeenDug = false;
    }

    public void ApplyScannedVisual()
    {
        if (hasBeenDug) return;

        // Determine which sprite to use
        Sprite spriteToUse = fallbackHintSprite;
        if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape) && sourceShape.TrackHintSprite != null)
        {
            spriteToUse = sourceShape.TrackHintSprite;
        }

        if (spriteToUse != null && this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = fallbackHintSprite;
            this.VisualRenderer.color = Color.white;
        }
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (hasBeenDug) return;

        if (GridGameManager.Instance.CurrentTool == EnumGridTool.MagnifyingGlass)
        {
            if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.MagnifyingGlass))
            {
                PerformRadarScan();
            }
            return;
        }

        if ((tool.HasValue && tool.Value == EnumGridTool.IcePick) || (GridGameManager.Instance.CurrentTool == EnumGridTool.IcePick))
        {
            if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.IcePick))
            {
                hasBeenDug = true;
                base.OnTileClicked(tool);

                if (trackFoundClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(trackFoundClip);
                }

                // Determine which sprite to use
                Sprite spriteToUse = fallbackDugSprite;
                if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape) && sourceShape.TrackDugSprite != null)
                {
                    spriteToUse = sourceShape.TrackDugSprite;
                }

                this.VisualRenderer.sprite = spriteToUse;

                // Apply transparency logic based on distance
                float alpha = 1f;
                if (TrackDistances.TryGetValue(currentGridPos, out int distance))
                {
                    if (distance == 2) alpha = 0.65f;
                    else if (distance >= 3) alpha = 0.35f;
                }

                this.VisualRenderer.color = new Color(1f, 1f, 1f, alpha);
            }
            else
            {
                Debug.Log($"<color=yellow>[TrackTile]</color> Not enough Ice Picks!");
            }
        }
    }
}