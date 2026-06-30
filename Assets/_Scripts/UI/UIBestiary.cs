using Assets._Scripts.Utilities.Singleton;
using UnityEngine;
using UnityEngine.UI;

public class UIBestiary : Singleton<UIBestiary> {

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button nextPageButton;


    private Image currentImage;

    [Header("Biome Images")]
    [SerializeField] private Image CliffsBiome;
    [SerializeField] private Image JaggedBiome;

    public event System.Action<EnumBiomes> OnBiomeChanged;

    [Header("Creature Entries")]
    [SerializeField] private GridLayoutGroup birdManEntry;

    private readonly float transitionDuration = 1f;

    private void OnEnable() {
        this.mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        this.playButton.onClick.AddListener(OnPlayButtonClicked);
        this.nextPageButton.onClick.AddListener(OnNextPageButtonClicked);
    }


    private void OnDisable() {
        this.mainMenuButton.onClick.RemoveAllListeners();
        this.playButton.onClick.RemoveAllListeners();
    }

    private void Start() {

        switch (GameManager.Instance.CurrentBiomeSelected) {
            case EnumBiomes.Cliffs:
                this.currentImage = this.CliffsBiome;
                this.CliffsBiome.gameObject.SetActive(true);
                break;
            case EnumBiomes.JaggedMountains:
                this.currentImage = this.JaggedBiome;
                this.JaggedBiome.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Biome {GameManager.Instance.CurrentBiomeSelected} is not implemented yet.");
                break;
        }
    }
    private void OnMainMenuButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainMenuScene);
        this.mainMenuButton.interactable = false;
    }
    private void OnPlayButtonClicked() {
        GameSceneManager.Instance.LoadScene(EnumScene.MainGame);
    }
    private async void OnNextPageButtonClicked() {
        await GameSceneManager.Instance.TransitionControler.FadeOut(this.transitionDuration);

        //EnumBiomes oldBiome = GameManager.Instance.CurrentBiomeSelected;
        EnumBiomes currentBiomeSelected = GameManager.Instance.CurrentBiomeSelected;
        EnumBiomes newBiome = GameManager.NextBiome(biome: currentBiomeSelected, setValue: true);

        this.currentImage.gameObject.SetActive(false); // Hide the old biome image

        switch (newBiome) {
            case EnumBiomes.Cliffs:
                this.currentImage = this.CliffsBiome;
                break;
            case EnumBiomes.JaggedMountains:
                this.currentImage = this.JaggedBiome;
                break;
            default:
                Debug.LogWarning($"Biome {newBiome} is not implemented yet.");
                break;
        }

        this.currentImage.gameObject.SetActive(true); // Show the new biome image

        this.OnBiomeChanged?.Invoke(newBiome);

        await GameSceneManager.Instance.TransitionControler.FadeIn(1f);

        Debug.Log($"Biome changed to {newBiome}");
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
