using Assets._Scripts.Utilities.Singleton;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBestiary : Singleton<UIBestiary> {

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playButton;

    [Header("Creature Entries")]
    [SerializeField] private GridLayoutGroup birdManEntry;

    void Start() {
        this.mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        this.playButton.onClick.AddListener(OnPlayButtonClicked);
    }


    private void OnDisable() {
        this.mainMenuButton.onClick.RemoveAllListeners();
        this.playButton.onClick.RemoveAllListeners();
    }
    private void OnMainMenuButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainMenuScene);
        this.mainMenuButton.interactable = false;
    }
    private void OnPlayButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainGame);
    }

    public void DisplayCreatureDetails(EnumCreatureName creatureName, bool isStickerDisplayed) {
        
        switch (creatureName) {
            case EnumCreatureName.BirdMan:
                this.birdManEntry.gameObject.SetActive(isStickerDisplayed);
                break;

            default:
                Debug.LogWarning($"Creature details for {creatureName} are not implemented yet.");
                break;
        }
    }
}
