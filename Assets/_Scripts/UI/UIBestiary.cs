using UnityEngine;
using UnityEngine.UI;

public class UIBestiary : MonoBehaviour {

    [SerializeField] private Button mainMenuButton;
    void Start() {
        this.mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void OnDisable() {
        this.mainMenuButton.onClick.RemoveAllListeners();
    }
    private void OnMainMenuButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainMenuScene);
        this.mainMenuButton.interactable = false;
    }
}
