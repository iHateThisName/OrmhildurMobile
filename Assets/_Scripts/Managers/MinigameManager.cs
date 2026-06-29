using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization; // 1. Added Localization namespace
using Assets._Scripts.Utilities.Singleton;

[System.Serializable]
public struct MinigameUIData
{
    public EnumScene MinigameScene;

    [Tooltip("Select the String Table and Key for the Intro text.")]
    public LocalizedString IntroText; // 2. Changed from string to LocalizedString

    [Tooltip("Select the String Table and Key for the Win text.")]
    public LocalizedString WinText;   // 2. Changed from string to LocalizedString

    [Tooltip("Select the String Table and Key for the Loss text.")]
    public LocalizedString LossText;  // 2. Changed from string to LocalizedString
}

public class MinigameManager : Singleton<MinigameManager>
{
    [Header("Minigame Setup")]
    public List<EnumScene> MinigameScenes;

    public List<MinigameUIData> MinigameTextData;

    [Header("Intro UI")]
    public GameObject IntroPanel;
    public Button IntroStartButton;
    public TMP_Text IntroTextDisplay;

    [Header("Outro UI")]
    public GameObject OutroPanel;
    public Button OutroOkButton;
    public TMP_Text OutroTextDisplay;

    private EnumScene currentlyPreloadedScene;
    private MinigameBase activeMinigame;
    private GameObject activeMinigameCanvas;

    private async void Start()
    {
        if (IntroPanel != null) IntroPanel.SetActive(false);
        if (OutroPanel != null) OutroPanel.SetActive(false);

        IntroStartButton.onClick.AddListener(OnIntroStartClicked);
        OutroOkButton.onClick.AddListener(OnOutroOkClicked);

        await PreloadNextMinigame();
    }

    private async Awaitable PreloadNextMinigame()
    {
        currentlyPreloadedScene = MinigameScenes[Random.Range(0, MinigameScenes.Count)];
        await GameSceneManager.Instance.PreloadSceneAdditiveAsync(currentlyPreloadedScene);
    }

    public void TriggerPreloadedMinigame()
    {
        GridGameManager.Instance.ChangeGameState(EnumGridGameState.MinigameActive);

        string targetSceneName = currentlyPreloadedScene.ToString();
        Scene minigameScene = SceneManager.GetSceneByName(targetSceneName);

        if (!minigameScene.IsValid() || !minigameScene.isLoaded)
        {
            Debug.LogError($"[Minigame Error] Scene '{targetSceneName}' is not loaded!");
            return;
        }

        activeMinigame = null;
        activeMinigameCanvas = null;

        GameObject[] rootObjects = minigameScene.GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects)
        {
            MinigameBase foundScript = rootObj.GetComponentInChildren<MinigameBase>(true);
            if (foundScript != null)
            {
                activeMinigame = foundScript;
                activeMinigameCanvas = rootObj;
                break;
            }
        }

        if (activeMinigame != null && activeMinigameCanvas != null)
        {
            MinigameUIData currentData = GetUIDataForScene(currentlyPreloadedScene);

            // 3. Asynchronously fetch and assign the localized Intro text
            if (IntroTextDisplay != null && !currentData.IntroText.IsEmpty)
            {
                currentData.IntroText.GetLocalizedStringAsync().Completed += (handle) =>
                {
                    IntroTextDisplay.text = handle.Result;
                };
            }

            IntroPanel.SetActive(true);
        }
        else
        {
            Debug.LogError($"[Minigame Error] Could not find the MinigameBase script in {targetSceneName}!");
        }
    }

    private void OnIntroStartClicked()
    {
        IntroPanel.SetActive(false);

        activeMinigameCanvas.SetActive(true);
        activeMinigame.OnMinigameComplete += HandleMinigameFinished;
        activeMinigame.StartMinigame();
    }

    private void HandleMinigameFinished(bool playerWon)
    {
        activeMinigame.OnMinigameComplete -= HandleMinigameFinished;
        MinigameUIData currentData = GetUIDataForScene(currentlyPreloadedScene);

        if (playerWon)
        {
            InventoryManager.Instance.AddToolCharge(EnumGridTool.IcePick, 2);
            Debug.Log("Minigame Won!");

            // 4. Asynchronously fetch and assign the localized Win text
            if (OutroTextDisplay != null && !currentData.WinText.IsEmpty)
            {
                currentData.WinText.GetLocalizedStringAsync().Completed += (handle) =>
                {
                    OutroTextDisplay.text = handle.Result;
                };
            }
        }
        else
        {
            InventoryManager.Instance.AddToolCharge(EnumGridTool.IcePick, -2);
            Debug.Log("Minigame Lost!");

            // 5. Asynchronously fetch and assign the localized Loss text
            if (OutroTextDisplay != null && !currentData.LossText.IsEmpty)
            {
                currentData.LossText.GetLocalizedStringAsync().Completed += (handle) =>
                {
                    OutroTextDisplay.text = handle.Result;
                };
            }
        }

        GridGameManager.Instance.IsResolvingInteraction = false;
        activeMinigameCanvas.SetActive(false);
        OutroPanel.SetActive(true);
    }

    private async void OnOutroOkClicked()
    {
        OutroPanel.SetActive(false);
        activeMinigame = null;
        activeMinigameCanvas = null;

        await GameSceneManager.Instance.UnloadSceneAdditiveAsync(currentlyPreloadedScene);
        GridGameManager.Instance.ChangeGameState(EnumGridGameState.PlayerTurn);
        await PreloadNextMinigame();
    }

    private MinigameUIData GetUIDataForScene(EnumScene scene)
    {
        foreach (var data in MinigameTextData)
        {
            if (data.MinigameScene == scene)
            {
                return data;
            }
        }
        return new MinigameUIData();
    }

    private void OnDestroy()
    {
        if (IntroStartButton != null) IntroStartButton.onClick.RemoveListener(OnIntroStartClicked);
        if (OutroOkButton != null) OutroOkButton.onClick.RemoveListener(OnOutroOkClicked);
    }
}