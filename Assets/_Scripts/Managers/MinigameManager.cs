using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets._Scripts.Utilities.Singleton;

public class MinigameManager : Singleton<MinigameManager>
{
    [Header("Minigame Pool")]
    public List<EnumScene> MinigameScenes;

    private EnumScene currentlyPreloadedScene;
    private MinigameBase activeMinigame;

    private async void Start()
    {
        // Start preloading the very first minigame when the hex grid loads
        await PreloadNextMinigame();
    }

    private async Awaitable PreloadNextMinigame()
    {
        // Pick a random scene from the enum list
        currentlyPreloadedScene = MinigameScenes[Random.Range(0, MinigameScenes.Count)];

        // Ask the GameSceneManager to load it silently
        await GameSceneManager.Instance.PreloadSceneAdditiveAsync(currentlyPreloadedScene);
    }

    public void TriggerPreloadedMinigame()
    {
        string targetSceneName = currentlyPreloadedScene.ToString();
        Debug.Log($"[Minigame] Attempting to trigger scene: {targetSceneName}");

        GridGameManager.Instance.ChangeGameState(EnumGridGameState.MinigameActive);

        Scene minigameScene = SceneManager.GetSceneByName(targetSceneName);

        // Debug Check 1: Did the scene actually load?
        if (!minigameScene.IsValid() || !minigameScene.isLoaded)
        {
            Debug.LogError($"[Minigame Error] Scene '{targetSceneName}' is not loaded or valid. Does your EnumScene exactly match the Scene file name?");
            return;
        }

        activeMinigame = null;
        GameObject canvasToActivate = null;

        // Debug Check 2: Searching the root objects
        GameObject[] rootObjects = minigameScene.GetRootGameObjects();
        Debug.Log($"[Minigame] Found {rootObjects.Length} root objects in the scene.");

        foreach (GameObject rootObj in rootObjects)
        {
            // Look for the script deeply (the 'true' makes it search disabled children too)
            MinigameBase foundScript = rootObj.GetComponentInChildren<MinigameBase>(true);

            if (foundScript != null)
            {
                activeMinigame = foundScript;
                canvasToActivate = rootObj;
                Debug.Log($"[Minigame] Found the script on: {rootObj.name}");
                break; // We found it, stop searching
            }
        }

        // Debug Check 3: Activating the game
        if (activeMinigame != null && canvasToActivate != null)
        {
            canvasToActivate.SetActive(true);
            Debug.Log("[Minigame] Canvas turned ON. Starting minigame logic...");

            activeMinigame.OnMinigameComplete += HandleMinigameFinished;
            activeMinigame.StartMinigame();
        }
        else
        {
            Debug.LogError($"[Minigame Error] Could not find the TapToWinMinigame script anywhere in the {targetSceneName} scene! Is it attached to the Canvas?");
        }
    }

    private async void HandleMinigameFinished(bool playerWon)
    {
        activeMinigame.OnMinigameComplete -= HandleMinigameFinished;

        if (playerWon) Debug.Log("Minigame Won! Give a reward!");
        else Debug.Log("Minigame Lost!");

        activeMinigame = null;

        // 1. Ask GameSceneManager to instantly dump the scene from memory
        await GameSceneManager.Instance.UnloadSceneAdditiveAsync(currentlyPreloadedScene);

        // 2. Unlock the hex grid
        GridGameManager.Instance.ChangeGameState(EnumGridGameState.PlayerTurn);

        // 3. Secretly preload the next minigame!
        await PreloadNextMinigame();
    }
}