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
            Color trackTint = Color.white; // Default fallback color

            // Get the distance (default to 1 if not found for some reason)
            int testDistance = 1;
            TrackDistances.TryGetValue(currentGridPos, out testDistance);

            // Ask the SO for the specific sprite and color based on distance
            if (TrackSources.TryGetValue(currentGridPos, out CreatureShape testSourceShape))
            {
                testSpriteToUse = testSourceShape.GetTrackSpriteForDistance(testDistance, fallbackDugSprite);
                trackTint = testSourceShape.trackColor; // Grab the color!
            }

            if (this.VisualRenderer != null)
            {
                this.VisualRenderer.sprite = testSpriteToUse;
                this.VisualRenderer.color = trackTint; // Apply the custom color
            }

            // Exit early so we don't apply the hint visuals below
            return;
        }
        // ---------------------------------------------------

        // Standard Hint Logic
        Sprite spriteToUse = fallbackHintSprite;
        Color hintTint = Color.white; // Default fallback color

        if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape))
        {
            if (sourceShape.TrackHintSprite != null)
            {
                spriteToUse = sourceShape.TrackHintSprite;
            }
            hintTint = sourceShape.trackColor; // Grab the color!
        }

        if (spriteToUse != null && this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = spriteToUse;
            this.VisualRenderer.color = hintTint; // Apply the custom color
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
                Color dugTint = Color.white; // Default fallback color

                // Get the distance (default to 1 if missing)
                int distance = 1;
                TrackDistances.TryGetValue(currentGridPos, out distance);

                // Fetch the tiered sprite and color
                if (TrackSources.TryGetValue(currentGridPos, out CreatureShape sourceShape))
                {
                    spriteToUse = sourceShape.GetTrackSpriteForDistance(distance, fallbackDugSprite);
                    dugTint = sourceShape.trackColor; // Grab the color!
                }

                this.VisualRenderer.sprite = spriteToUse;
                this.VisualRenderer.color = dugTint; // Apply the custom color
            }
            else
            {
                Debug.Log($"<color=yellow>[TrackTile]</color> Not enough Ice Picks!");
            }
        }
    }
}