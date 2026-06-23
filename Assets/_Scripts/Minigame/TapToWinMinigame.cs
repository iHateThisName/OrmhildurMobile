using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TapToWinMinigame : MinigameBase
{
    [Header("UI References")]
    public TextMeshProUGUI TimerText;
    public TextMeshProUGUI TapCountText;
    public Button MashButton;

    [Header("Game Rules")]
    public int TapsRequired = 10;
    private int currentTaps = 0;

    public override void StartMinigame()
    {
        // 1. Call the base method to start the 5-second clock
        base.StartMinigame();

        currentTaps = 0;
        UpdateUI();

        // 2. Safely hook up the UI button via code
        MashButton.onClick.RemoveAllListeners();
        MashButton.onClick.AddListener(OnMashButtonClicked);
    }

    protected override void Update()
    {
        // 1. Let the base class handle the countdown and timeout loss
        base.Update();

        if (!isPlaying) return;

        // 2. Update the visual timer (F1 formats it to one decimal place, e.g., "4.2s")
        if (TimerText != null)
        {
            TimerText.text = $"{timeRemaining:F1}s";
        }
    }

    private void OnMashButtonClicked()
    {
        if (!isPlaying) return;

        currentTaps++;
        UpdateUI();

        // Check for the win condition
        if (currentTaps >= TapsRequired)
        {
            EndMinigame(true); // Player won!
        }
    }

    private void UpdateUI()
    {
        if (TapCountText != null)
        {
            TapCountText.text = $"{currentTaps} / {TapsRequired}";
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks when the scene unloads
        if (MashButton != null)
        {
            MashButton.onClick.RemoveAllListeners();
        }
    }
}