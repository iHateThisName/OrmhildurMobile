using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimonSaysMinigame : MinigameBase
{
    [Header("UI References")]
    public TextMeshProUGUI TimerText;
    public Button[] SimonButtons;

    [Header("Game Rules")]
    public int SequenceLength = 4;
    public float FlashSpeed = 0.3f;

    private List<int> currentSequence = new List<int>();
    private int playerProgressIndex = 0;
    private bool isShowingSequence = false;

    public override void StartMinigame()
    {
        base.StartMinigame();

        if (SimonButtons.Length < 4) Debug.LogError("Not enough buttons assigned!");

        for (int i = 0; i < SimonButtons.Length; i++)
        {
            int index = i;
            SimonButtons[i].onClick.RemoveAllListeners();
            SimonButtons[i].onClick.AddListener(() => OnSimonButtonClicked(index));
        }

        GenerateSequence();

        // Call the async method directly (fire and forget)
        PlaySequenceAnimationAsync();
    }

    protected override void Update()
    {
        base.Update();

        if (isPlaying && TimerText != null)
        {
            TimerText.text = $"{timeRemaining:F1}s";
        }
    }

    private void GenerateSequence()
    {
        currentSequence.Clear();
        for (int i = 0; i < SequenceLength; i++)
        {
            currentSequence.Add(Random.Range(0, SimonButtons.Length));
        }
    }

    // Swapped IEnumerator to async void
    private async void PlaySequenceAnimationAsync()
    {
        isShowingSequence = true;

        try
        {
            // Wait 0.2s, passing the cancellation token to abort if the minigame is destroyed
            await Awaitable.WaitForSecondsAsync(0.2f, destroyCancellationToken);

            foreach (int buttonIndex in currentSequence)
            {
                Button targetButton = SimonButtons[buttonIndex];
                Image btnImage = targetButton.GetComponent<Image>();
                Color originalColor = btnImage.color;

                btnImage.color = Color.white;

                await Awaitable.WaitForSecondsAsync(FlashSpeed, destroyCancellationToken);

                btnImage.color = originalColor;

                await Awaitable.WaitForSecondsAsync(0.1f, destroyCancellationToken);
            }

            // Sequence over, let the player click!
            isShowingSequence = false;
            playerProgressIndex = 0;

        }
        catch (System.OperationCanceledException)
        {
            // This safely catches the abort signal if the minigame is closed early.
            // You don't need to put anything here; the error is cleanly swallowed.
            Debug.Log("[Simon Says] Sequence aborted safely due to scene unload.");
        }
    }

    private void OnSimonButtonClicked(int clickedIndex)
    {
        if (isShowingSequence || !isPlaying) return;

        if (clickedIndex == currentSequence[playerProgressIndex])
        {
            playerProgressIndex++;

            if (playerProgressIndex >= SequenceLength)
            {
                EndMinigame(true);
            }
        }
        else
        {
            EndMinigame(false);
        }
    }
}