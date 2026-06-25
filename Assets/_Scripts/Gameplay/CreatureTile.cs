using UnityEngine;

public class CreatureTile : TileEntityBase, IScannable
{
    [Header("Creature Tile Visuals")]
    [SerializeField] private SpriteRenderer creatureVisualRendere;
    [SerializeField] private SpriteMask mask;
    [field:SerializeField] public EnumCreatureName creatureName { get; private set; } = EnumCreatureName.None;

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite defaultSprite;

    private bool hasBeenDug = false;
    private bool isCreatureAnchor = false;

    private void Awake() {
        // Hide the mask and creature visual by default, they will be later revealed by the player.
        this.mask.gameObject.SetActive(false);
        // Only the anchor tile will show the creature visual, the rest will be hidden behind the mask.
        this.creatureVisualRendere.gameObject.SetActive(false);
    }
    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        hasBeenDug = false;
    }

    public void SetAsCreatureAnchorTile() {
        this.isCreatureAnchor = true;
        this.creatureVisualRendere.gameObject.SetActive(true);
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