using UnityEngine;

public class TreasureTile : TileEntityBase, IScannable
{
    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip treasureClip;

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

                if (treasureClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(treasureClip);
                }

                this.VisualRenderer.sprite = defaultSprite;
                VisualRenderer.color = Color.gold;

                GrantRandomLoot();
            }
            else
            {
                // They clicked the creature with the Hand or no tool
                Debug.Log($"<color=yellow>[CreatureTile]</color> You need a tool to dig this up!");
            }
        }
    }

    private void GrantRandomLoot()
    {
        int lootRoll = Random.Range(0, 2);

        if (lootRoll == 0)
        {
            InventoryManager.Instance.AddToolCharge(EnumGridTool.IcePick, 3);
        }
        else
        {
            InventoryManager.Instance.AddToolCharge(EnumGridTool.MagnifyingGlass, 1);
        }
    }
}