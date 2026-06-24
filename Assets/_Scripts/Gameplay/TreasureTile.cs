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

    public override void OnTileClicked()
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

        hasBeenDug = true;

        base.OnTileClicked();

        if (treasureClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(treasureClip);
        }

        this.VisualRenderer.sprite = defaultSprite;
        VisualRenderer.color = Color.gold;

        GrantRandomLoot();
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