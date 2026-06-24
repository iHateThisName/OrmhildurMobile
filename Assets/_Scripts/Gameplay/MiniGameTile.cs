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

    public override void OnTileClicked()
    {
        if (hasBeenTriggered) return;

        if (GridGameManager.Instance.CurrentTool == EnumGridTool.MagnifyingGlass)
        {
            PerformRadarScan();
            return;
        }



        hasBeenTriggered = true;

        base.OnTileClicked();
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