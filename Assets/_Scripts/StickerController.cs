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


    private void Start() {
        this.defaulttickerScale = this.stickerCreatuerGameObject.transform.localScale;
        this.stickerButton.onClick.AddListener(OnClick);
    }

    private void OnDisable() {
        this.stickerButton.onClick.RemoveAllListeners();
    }

    [Button]
    public void DisplaySticker() {
        this.stickerCreatuerGameObject.SetActive(true);
        this.stickerHidenGameObject.SetActive(false);
        this.isStickerDisplayed = true;

        this.stickerCreatuerGameObject.transform.DOLocalJump(new Vector3(0, 0, 0), .25f, 1, 0.5f).SetEase(Ease.OutBounce).OnComplete(() => {
            this.stickerCreatuerGameObject.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 2f).SetEase(Ease.InSine).SetLoops(-1, LoopType.Yoyo);
        });
    }

    [Button]
    public void HideSticker() {
        this.stickerCreatuerGameObject.SetActive(false);
        this.stickerHidenGameObject.SetActive(true);
        this.isStickerDisplayed = false;

        // Disable sticker effect and animations
        this.stickerCreatuerGameObject.transform.DOKill(false); // stop all tweens.
        this.stickerCreatuerGameObject.transform.localScale = this.defaulttickerScale;

    }

    public void OnClick() {
        Debug.Log($"StickerController: Clicked for {this.creatureName}");

        if (!isStickerDisplayed) {
            DisplaySticker();
        } else {
            HideSticker();
        }

        UIBestiary.Instance.DisplayCreatureDetails(this.creatureName, this.isStickerDisplayed);

    }
}
