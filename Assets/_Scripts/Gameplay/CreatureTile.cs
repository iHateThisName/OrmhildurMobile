using UnityEngine;

public class CreatureTile : TileEntityBase
{
    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;

    private bool hasBeenDug = false; 

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        hasBeenDug = false;
    }

    public override void OnTileClicked()
    {
        if (hasBeenDug) return; // Don't do anything if already dug
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