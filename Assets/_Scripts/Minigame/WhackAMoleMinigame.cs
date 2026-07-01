using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct CharacterSprites
{
    public string CharacterName;
    public Sprite NormalSprite;
    public Sprite HitSprite;
}

public class WhackAMoleMinigame : MinigameBase
{
    [Header("Whack-A-Mole Settings")]
    [Tooltip("How many moles the player needs to hit to win.")]
    public int ScoreToWin = 5;

    [Tooltip("Assign the 6 clickable mole buttons here.")]
    public Button[] MoleButtons;

    [Header("Character Settings")]
    [Tooltip("Assign your 4 character sprite pairs here.")]
    public CharacterSprites[] AvailableCharacters;

    private int currentScore = 0;
    private int currentMoleIndex = -1;

    // Tracks the character chosen for the current round
    private CharacterSprites currentCharacter;

    // Prevents the player from double-clicking during the 1-second delay
    private bool isWaitingForDelay = false;

    public override void StartMinigame()
    {
        base.StartMinigame();

        currentScore = 0;
        isWaitingForDelay = false;

        // 2. Pick a random character for this playthrough
        if (AvailableCharacters.Length > 0)
        {
            int randomCharIndex = Random.Range(0, AvailableCharacters.Length);
            currentCharacter = AvailableCharacters[randomCharIndex];
        }

        // Hook up the click events and hide moles
        for (int i = 0; i < MoleButtons.Length; i++)
        {
            MoleButtons[i].gameObject.SetActive(false);
            MoleButtons[i].onClick.RemoveAllListeners();
            MoleButtons[i].onClick.AddListener(OnMoleClicked);
        }

        SpawnRandomMole();
    }

    protected override void Update()
    {
        base.Update();
    }

    private void SpawnRandomMole()
    {
        // Hide the old mole
        if (currentMoleIndex != -1)
        {
            MoleButtons[currentMoleIndex].gameObject.SetActive(false);
        }

        // Pick a completely random new mole
        int newMoleIndex = Random.Range(0, MoleButtons.Length);
        if (newMoleIndex == currentMoleIndex && MoleButtons.Length > 1)
        {
            newMoleIndex = (newMoleIndex + 1) % MoleButtons.Length;
        }

        currentMoleIndex = newMoleIndex;
        Button newMoleButton = MoleButtons[currentMoleIndex];

        // Reset the button state and apply the "Normal" sprite
        newMoleButton.interactable = true;
        Image moleImage = newMoleButton.GetComponent<Image>();
        if (moleImage != null)
        {
            moleImage.sprite = currentCharacter.NormalSprite;
        }

        // Show the new mole
        newMoleButton.gameObject.SetActive(true);
    }

    private void OnMoleClicked()
    {
        // Ignore clicks if game over, or if we are already waiting for the hit delay
        if (!isPlaying || isWaitingForDelay) return;

        isWaitingForDelay = true;
        currentScore++;

        //Change to the "Hit" expression and disable clicking
        Button hitMoleButton = MoleButtons[currentMoleIndex];
        hitMoleButton.interactable = false; // Stops double-clicks

        Image moleImage = hitMoleButton.GetComponent<Image>();
        if (moleImage != null)
        {
            moleImage.sprite = currentCharacter.HitSprite;
        }

        StartCoroutine(HitDelayRoutine());
    }

    private IEnumerator HitDelayRoutine()
    {
        // Wait for exactly 1 second
        yield return new WaitForSeconds(1f);

        isWaitingForDelay = false;

        if (currentScore >= ScoreToWin)
        {
            // They hit enough! Win the game.
            EndMinigame(true);
        }
        else
        {
            // Spawn the next one
            SpawnRandomMole();
        }
    }
}