using System;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour {
    [SerializeField] private Button playButton;
    [SerializeField] private Button bestiaryButton;

    private void OnEnable() {
        this.playButton.onClick.AddListener(OnPlayButtonClicked);
        this.bestiaryButton.onClick.AddListener(OnBestiaryButtonClicked);
    }

    private void OnDisable() {
        this.playButton.onClick.RemoveAllListeners();
        this.bestiaryButton.onClick.RemoveAllListeners();
    }

    private void OnPlayButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainGame);
    }
    private void OnBestiaryButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.BestiaryBookScene);
    }

}
