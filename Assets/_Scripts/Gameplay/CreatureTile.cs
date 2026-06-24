using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;

public class CreatureTile : TileEntityBase, IScannable
{
    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite defaultSprite;

    private bool hasBeenDug = false;

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        hasBeenDug = false;
    }

    public void ApplyScannedVisual()
    {
        if (hasBeenDug) return;

        if (questionMarkSprite != null && this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = questionMarkSprite;
            this.VisualRenderer.color = Color.white;
        }
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (hasBeenDug) return;

        if ((tool.HasValue && tool.Value == EnumGridTool.MagnifyingGlass) || (!tool.HasValue && GridGameManager.Instance.CurrentTool == EnumGridTool.MagnifyingGlass))
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

                this.VisualRenderer.sprite = defaultSprite;
                VisualRenderer.color = Color.green;

                // Ask the tracker if this was the final piece
                bool isFullyDiscovered = CreatureTracker.Instance.ReportTileDug(this.CurrentGridPosition);

                if (isFullyDiscovered)
                {
                    audioSource.PlayOneShot(creatureCompleteClip);

                    //Sparkles
                }
                else
                {
                    audioSource.PlayOneShot(digCreaturePartClip);
                }
            }
            else
            {
                // They clicked the creature with the Hand or no tool
                Debug.Log($"<color=yellow>[CreatureTile]</color> You need a tool to dig this up!");
            }
        }
    }
}