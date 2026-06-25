using UnityEngine;

public class MiniGameTile : TileEntityBase, IScannable
{
    private bool hasBeenTriggered = false;

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip miniGameClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite defaultSprite;
    public void ApplyScannedVisual()
    {
        if (hasBeenTriggered) return;

        if (questionMarkSprite != null && this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = questionMarkSprite;
            this.VisualRenderer.color = Color.white;
        }
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (hasBeenTriggered) return;

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
                hasBeenTriggered = true;

                base.OnTileClicked(tool);
                this.VisualRenderer.sprite = defaultSprite;
                VisualRenderer.color = Color.red;

                if (miniGameClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(miniGameClip);
                }

                // Trigger random minigame from the MinigameManager
                MinigameManager.Instance.TriggerPreloadedMinigame();
            }
        }
        else
        {
            // They clicked the creature with the Hand or no tool
            Debug.Log($"<color=yellow>[CreatureTile]</color> You need a tool to dig this up!");
        }
    }
}