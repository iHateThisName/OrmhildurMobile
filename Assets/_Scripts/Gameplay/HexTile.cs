using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexTile : TileEntityBase, IScannable
{
    // [TEMP DISABLED] private int healthPoints = 5;
    public bool IsInteractable { get; private set; } = true;

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite emptyScannedSprite;
    [SerializeField] private Sprite defaultSprite;

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        IsInteractable = true;
    }

    public void ApplyScannedVisual()
    {
        if (!this.IsInteractable)
        {
            return;
        }

        if (emptyScannedSprite == null || this.VisualRenderer == null)
        {
            return;
        }

        this.VisualRenderer.sprite = emptyScannedSprite;
        this.VisualRenderer.color = Color.white;
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (!this.IsInteractable) return;



        switch (tool.HasValue ? tool.Value : GridGameManager.Instance.CurrentTool)
        {
            case EnumGridTool.None:
                Debug.Log($"HexTile at {CurrentGridPosition} was clicked with no tool.");
                break;

            case EnumGridTool.IcePick:
                if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.IcePick))
                {
                    if (digClip != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(digClip);
                    }
                    InstaDigTile();
                }
                break;

            case EnumGridTool.Hammer:
                if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.Hammer))
                {
                    InstaDigTile();
                }
                break;

            case EnumGridTool.MagnifyingGlass:
                if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.MagnifyingGlass))
                {
                    PerformRadarScan();
                }
                break;

            default:
                Debug.LogWarning($"HexTile at {CurrentGridPosition} was clicked with an unhandled tool: {GridGameManager.Instance.CurrentTool}");
                break;
        }
    }

    private void InstaDigTile()
    {
        this.IsInteractable = false;
        if (this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = defaultSprite;
            //this.VisualRenderer.color = Color.saddleBrown;
        }
        Debug.Log($"HexTile at {CurrentGridPosition} was dug out. (HP system bypassed)");
    }

    /* ==========================================
       [TEMP DISABLED HP LOGIC] 
       Uncomment below when re-implementing HP
       ========================================== */

    /*
    private void DealSplashDamage(int splashDamage) {
        List<TileEntityBase> neighbors = GridManager.Instance.GetNeighbors(this.CurrentGridPosition).ToList<TileEntityBase>();
        neighbors.ForEach(neighbor => {
            if (neighbor is HexTile neighborHexTile) {
                neighborHexTile.DealTileDamage(splashDamage);
            }
        });
        return;
    }

    private void DealTileDamage(int damage) {
        this.healthPoints -= damage;
        if (this.healthPoints == 0) {
            // Handle tile prefect destruction logic.
            this.IsInteractable = false;
            Debug.Log($"HexTile at {CurrentGridPosition} has been perfectly destroyed.");

        } else if (this.healthPoints < 0) { // hp is negative.
            // Hidden item is destroyed, because of overkill damage.
            this.IsInteractable = false;
            Debug.LogWarning($"HexTile at {CurrentGridPosition} received overkill damage. Health points: {this.healthPoints}");

        } else {
            Debug.Log($"HexTile at {CurrentGridPosition} received {damage} damage. Remaining health points: {this.healthPoints}");
        }
        UpdateTileVisual();
    }

    private void UpdateTileVisual() {
        // Update the visual representation of the tile based on its current health points.
        switch (this.healthPoints) {
            case 5: break;
            case 4:
                this.VisualRenderer.sprite = defaultSprite;
                this.VisualRenderer.color = Color.darkOrange;
                break;
            case 3:
                this.VisualRenderer.color = Color.orange;
                break;
            case 2:
                this.VisualRenderer.color = Color.yellow;
                break;
            case 1:
                this.VisualRenderer.color = Color.yellowGreen;
                break;
            case 0: // Perfect
                this.VisualRenderer.color = Color.lightGreen;
                break;
            default:
                this.VisualRenderer.color = Color.black; // Destroyed state or overkill damage
                break;
        }
    }
    */
}