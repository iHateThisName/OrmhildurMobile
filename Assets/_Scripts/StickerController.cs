using DG.Tweening;
using Gaskellgames;
using UnityEngine;
using UnityEngine.UI;

public class StickerController : MonoBehaviour {


    [SerializeField] private GameObject stickerCreatuerGameObject;
    [SerializeField] private GameObject stickerHidenGameObject;
    [SerializeField] private Button stickerButton;
    [field: SerializeField] public EnumCreatureName creatureName { get; private set; } = EnumCreatureName.None;

    private Vector2 defaulttickerScale;
    private bool isStickerDisplayed = false;
    private int stickerCount = 0;
    private bool IsStickerCollected => stickerCount > 0;
    private bool IsCorrectBiome => GameManager.GetCreatureBiome(this.creatureName) == GameManager.Instance.CurrentBiomeSelected;

    private void OnEnable() {
        this.stickerButton.onClick.AddListener(OnClick);
        UIBestiary.Instance.OnBiomeChanged += HandleBiomeChanged;

    }
    private void OnDisable() {
        this.stickerButton.onClick.RemoveAllListeners();
        UIBestiary.Instance.OnBiomeChanged -= HandleBiomeChanged;
    }

    private void Start() {
        this.defaulttickerScale = this.stickerCreatuerGameObject.transform.localScale;
        this.stickerCount = GameManager.Instance.GetCreatureSaveData(this.creatureName).amount;

        if (this.IsStickerCollected) {
            DisplaySticker();
        }

        if (!this.IsCorrectBiome) {
            // The creature do not belong to the current biome, hide the sticker and disable the button
            this.stickerButton.gameObject.SetActive(false);
        }
    }

    private void HandleBiomeChanged(EnumBiomes newBiome) {
        if (this.IsCorrectBiome) {
            DisplaySticker(); // Show
        } else {
            DisableSticker(); // Hide and disable
        }
    }

    [Button]
    public void DisplayStickerWithAnimation() {
        DisplaySticker();

        this.stickerCreatuerGameObject.transform.DOLocalJump(new Vector3(0, 0, 0), .25f, 1, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => {
            this.stickerCreatuerGameObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 2f).SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo);
        });
    }

    private void DisplaySticker() {
        if (this.IsStickerCollected) {
            this.stickerCreatuerGameObject.SetActive(true);
            this.stickerHidenGameObject.SetActive(false);
            this.isStickerDisplayed = true;
            this.stickerButton.interactable = true;
        } else {
            HideSticker();
        }
    }

    [Button]
    public void HideSticker() {
        this.stickerCreatuerGameObject.SetActive(false);
        this.stickerHidenGameObject.SetActive(true);
        this.isStickerDisplayed = false;
        this.stickerButton.interactable = true;

        DisableStickerAnimation();

    }

    private void DisableStickerAnimation() {
        // Disable sticker effect and animations
        this.stickerCreatuerGameObject.transform.DOKill(false); // stop all tweens.
        this.stickerCreatuerGameObject.transform.localScale = this.defaulttickerScale;
    }

    public void DisableSticker() {
        this.stickerButton.interactable = false;
        this.stickerCreatuerGameObject.SetActive(false);
        this.stickerHidenGameObject.SetActive(false);
        this.isStickerDisplayed = false;
        DisableStickerAnimation();
    }

    public void OnClick() {
        Debug.Log($"StickerController: Clicked for {this.creatureName}");
        if (!this.IsStickerCollected) {
            Debug.Log($"StickerController: Sticker for {this.creatureName} is not collected yet.");
            return;
        }

        if (!this.isStickerDisplayed) {
            DisplayStickerWithAnimation();
        } else {
            HideSticker();
        }

        UIBestiary.Instance.DisplayCreatureDetails(this.creatureName, this.isStickerDisplayed);

    }
}
