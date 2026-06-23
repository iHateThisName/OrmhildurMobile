using UnityEngine;

public class MiniGameTile : TileEntityBase
{
    private bool hasBeenTriggered = false;

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip miniGameClip;

    public override void OnTileClicked()
    {
        // 1. Prevent the player from clicking this tile multiple times
        if (hasBeenTriggered) return;
        hasBeenTriggered = true;

        base.OnTileClicked();
        VisualRenderer.color = Color.red;

        if (miniGameClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(miniGameClip);
        }

        // Trigger random minigame from the MinigameManager
        MinigameManager.Instance.TriggerPreloadedMinigame();
    }
}