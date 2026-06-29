using UnityEngine;
using System.Collections.Generic;

public class TrackTile : TileEntityBase, IScannable
{
    // STATIC DICTIONARIES:
    public static Dictionary<Vector2Int, int> TrackDistances = new Dictionary<Vector2Int, int>();
    public static Dictionary<Vector2Int, CreatureShape> TrackSources = new Dictionary<Vector2Int, CreatureShape>();

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip trackFoundClip;

    [Header("Fallback Visuals")]
    [Tooltip("These will only be used if the CreatureShape doesn't have custom sprites assigned.")]
    [SerializeField] private Sprite fallbackHintSprite;
    [SerializeField] private Sprite fallbackDugSprite;

    private bool hasBeenDug = false;

    [Header("Testing")]
    [SerializeField] private bool instantReveal = false;

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

        // --- ISOLATED TEST LOGIC: Instant Reveal on Scan ---
        if (instantReveal)
        {
            hasBeenDug = true;

            // Play audio (checking !isPlaying prevents eardrum destruction if an AoE hits 4 tracks at once)
            if (trackFoundClip != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(trackFoundClip);
            }

            Sprite testSpriteToUse = fallbackDugSprite;
            if (TrackSources.TryGetValue(currentGridPos, out CreatureShape testSourceShape) && testSourceShape.TrackDugSprite != null)
            {
                testSpriteToUse = testSourceShape.TrackDugSprite;
            }

            if (this.VisualRenderer != null)
            {
                this.VisualRenderer.sprite = testSpriteToUse;

                float testAlpha = 1f;
                if (TrackDistances.TryGetValue(currentGridPos, out int testDistance))
                {
                    if (testDistance == 2) testAlpha = 0.65f;
                    else if (testDistance >= 3) testAlpha = 0.35f;
                }
                this.VisualRenderer.color = new Color(1f, 1f, 1f, testAlpha);
            }

            // Exit early so we don't apply the hint visuals below
            return;
        }
        // ---------------------------------------------------

        // Standard Hint Logic
        Sprite spriteToUse = fallbackHintSprite;
        if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape) && sourceShape.TrackHintSprite != null)
        {
            spriteToUse = sourceShape.TrackHintSprite;
        }

        if (spriteToUse != null && this.VisualRenderer != null)
        {
            // Kept hardcoded to fallbackHintSprite per your request
            this.VisualRenderer.sprite = fallbackHintSprite;
            this.VisualRenderer.color = Color.white;
        }
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (hasBeenDug) return;

        bool isMagnifyingGlass = (tool.HasValue && tool.Value == EnumGridTool.MagnifyingGlass) ||
                                 (GridGameManager.Instance.CurrentTool == EnumGridTool.MagnifyingGlass);

        if (isMagnifyingGlass)
        {
            if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.MagnifyingGlass))
            {
                // We just call the standard scan. ApplyScannedVisual() handles the test logic now!
                PerformRadarScan();
            }
            return;
        }

        // --- ICE PICK LOGIC ---
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

                Sprite spriteToUse = fallbackDugSprite;
                if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape) && sourceShape.TrackDugSprite != null)
                {
                    spriteToUse = sourceShape.TrackDugSprite;
                }

                this.VisualRenderer.sprite = spriteToUse;

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