using Assets._Scripts.Utilities.Singleton;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : PersistentSingleton<GameSceneManager> {

    private TransistionController transistionManager;
    private readonly float loadingSceneDuration = 2f; // Minimumm Duration for displaying the loading scene.

    private readonly string transitionPrefabPath = "UI/TransistionPrefab Variant";

    public async void LoadScene(Enum scene) {

        TransistionController transistionController = CreateTransition();

        // Fade out the current scene
        await transistionController.FadeOut();

        // Load the loading scene and set it as the active scene
        await SceneManager.LoadSceneAsync(EnumScene.LoadingScene.ToString(), LoadSceneMode.Single);

        // Load the target scene asynchronously in the background
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene.ToString(), LoadSceneMode.Single);
        loadOperation.allowSceneActivation = false;

        // Wait for the loading scene to be displayed for a minimum duration and until the target scene is loaded
        float timer = 0;
        while (timer < this.loadingSceneDuration || loadOperation.progress < 0.9f) {
            timer += Time.deltaTime;
            await Awaitable.NextFrameAsync();
        }

        // Activate the target scene
        loadOperation.allowSceneActivation = true;

        // Fade in the target scene
        await transistionController.FadeIn();

        // Unload all scenes that are not the target scene
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene sceneToUnload = SceneManager.GetSceneAt(i);
            if (sceneToUnload.name != scene.ToString()) {
                await SceneManager.UnloadSceneAsync(sceneToUnload);
            }
        }
    }

    private TransistionController CreateTransition() {
        if (this.transistionManager == null) {
            GameObject trasistionPrefab = Resources.Load<GameObject>(this.transitionPrefabPath);
            GameObject transitionObject = Instantiate(trasistionPrefab);
            this.transistionManager = transitionObject.GetComponent<TransistionController>();
            DontDestroyOnLoad(transitionObject);
        }
        return this.transistionManager;
    }
}

public enum EnumScene {
    MainMenuScene,
    LoadingScene,
    BestiaryBookScene,
    GameScene,
}
