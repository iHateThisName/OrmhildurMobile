using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransistionController : MonoBehaviour {

    [SerializeField] private Image CanvasFadeImage;
    private static readonly float FadeDuration = 1.5f;
    public async Awaitable FadeOut(float? duration = null) {
        Color currentColor = this.CanvasFadeImage.color;
        currentColor.a = 0;
        this.CanvasFadeImage.color = currentColor;

        await this.CanvasFadeImage.DOFade(1f, duration ?? FadeDuration).AsyncWaitForCompletion();
    }

    public async Awaitable FadeIn(float? duration = null) {
        Color currentColor = this.CanvasFadeImage.color;
        currentColor.a = 1;
        this.CanvasFadeImage.color = currentColor;
        await this.CanvasFadeImage.DOFade(0f, duration ?? FadeDuration).AsyncWaitForCompletion();
    }
}
