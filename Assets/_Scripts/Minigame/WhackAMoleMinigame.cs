using UnityEngine;
using UnityEngine.UI;

public class WhackAMoleMinigame : MinigameBase
{
    [Header("Whack-A-Mole Settings")]
    [Tooltip("How many moles the player needs to hit to win.")]
    public int ScoreToWin = 5;

    [Tooltip("Assign the 6 clickable mole buttons here.")]
    public Button[] MoleButtons;

    private int currentScore = 0;
    private int currentMoleIndex = -1;

    public override void StartMinigame()
    {
        // 1. Call the base method to start the timer
        base.StartMinigame();

        currentScore = 0;

        // 2. Hide all moles and hook up their click events
        for (int i = 0; i < MoleButtons.Length; i++)
        {
            MoleButtons[i].gameObject.SetActive(false);

            // Clean up old listeners just in case, then add the click event
            MoleButtons[i].onClick.RemoveAllListeners();
            MoleButtons[i].onClick.AddListener(OnMoleClicked);
        }

        // 3. Spawn the very first mole
        SpawnRandomMole();
    }

    protected override void Update()
    {
        // Keep the timer ticking from MinigameBase
        base.Update();
    }

    private void SpawnRandomMole()
    {
        // Hide the current mole if there is one
        if (currentMoleIndex != -1)
        {
            MoleButtons[currentMoleIndex].gameObject.SetActive(false);
        }

        // Pick a completely random new mole
        int newMoleIndex = Random.Range(0, MoleButtons.Length);

        // Optional: Ensure it doesn't spawn in the exact same spot twice in a row
        if (newMoleIndex == currentMoleIndex && MoleButtons.Length > 1)
        {
            newMoleIndex = (newMoleIndex + 1) % MoleButtons.Length;
        }

        currentMoleIndex = newMoleIndex;

        // Show the new mole
        MoleButtons[currentMoleIndex].gameObject.SetActive(true);
    }

    private void OnMoleClicked()
    {
        // Ignore clicks if the game isn't running (e.g., time just ran out)
        if (!isPlaying) return;

        currentScore++;

        if (currentScore >= ScoreToWin)
        {
            // They hit enough! Win the game.
            EndMinigame(true);
        }
        else
        {
            // Instantly spawn the next one
            SpawnRandomMole();
        }
    }
}