using UnityEngine;

public class CreatureTile : TileEntityBase, IScannable
{
    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite questionMarkSprite;

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
            PerformRadarScan();
            return;
        }

        hasBeenDug = true;

        base.OnTileClicked();

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
}