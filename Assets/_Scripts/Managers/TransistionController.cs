using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransistionController : MonoBehaviour {

    [SerializeField] private Image CanvasFadeImage;
    private readonly float fadeDuration = 2f;
    public async Awaitable FadeOut() {
        Color currentColor = this.CanvasFadeImage.color;
        currentColor.a = 0;
        this.CanvasFadeImage.color = currentColor;

        await this.CanvasFadeImage.DOFade(1f, this.fadeDuration).AsyncWaitForCompletion();
    }

    public async Awaitable FadeIn() {
        Color currentColor = this.CanvasFadeImage.color;
        currentColor.a = 1;
        this.CanvasFadeImage.color = currentColor;
        await this.CanvasFadeImage.DOFade(0f, this.fadeDuration).AsyncWaitForCompletion();
    }
}
