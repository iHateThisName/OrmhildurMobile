using DG.Tweening;
using UnityEngine;

// This is a base class for all tile entities, which are components that can be attached to randomTiles in the grid to give them special behavior or properties.
public class TileEntityBase : MonoBehaviour {
    public Vector2Int CurrentGridPosition { get; private set; } = Vector2Int.zero;
    [field: SerializeField] public SpriteRenderer VisualRenderer { get; private set; }

    private Sequence sequence;

    private void Awake() {
        if (this.VisualRenderer == null) {
            this.VisualRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
    public virtual void Initialize(Vector2Int gridPosition, Color? visualColor = null) {
        this.CurrentGridPosition = gridPosition;
        // Assigne color if provided, otherwise keep the default color of the sprite.
        if (visualColor.HasValue) this.VisualRenderer.color = visualColor.Value;

        // Additional initialization logic...
    }
    public virtual void OnTileClicked(EnumGridTool? tool) {
        // Called when the tile is clicked, can be overridden by derived classes to implement specific behavior.
        if (this.sequence.IsActive() && this.sequence?.IsPlaying() == true) {
            StopAnimations();
        } else {
            StartTileClickedAnimation();
        }
    }

    public void PerformRadarScan()
    {
        Debug.Log($"<color=cyan>[Radar]</color> Triggered on {this.gameObject.name} at {this.CurrentGridPosition}");

        if (this is IScannable selfScannable)
        {
            selfScannable.ApplyScannedVisual();
        }
        else
        {
            Debug.LogWarning($"<color=cyan>[Radar]</color> {this.gameObject.name} does NOT implement IScannable!");
        }

        var neighbors = GridManager.Instance.GetNeighbors(this.CurrentGridPosition);
        Debug.Log($"<color=cyan>[Radar]</color> Found {System.Linq.Enumerable.Count(neighbors)} neighbors to scan.");

        foreach (TileEntityBase neighbor in neighbors)
        {
            if (neighbor is IScannable scannableNeighbor)
            {
                scannableNeighbor.ApplyScannedVisual();
            }
        }
    }

    public void StopAnimations() {
        this.sequence?.Kill();
        this.VisualRenderer.transform.localScale = Vector3.one;
        this.VisualRenderer.transform.localPosition = Vector3.zero;
    }

    private void StartTileClickedAnimation() {
        this.sequence?.Kill(); // Kill any existing sequence to prevent overlapping animations.

        this.sequence = DOTween.Sequence();

        this.sequence.Append(this.VisualRenderer.transform.DOScale(1.15f, 0.4f));
        this.sequence.Join(this.VisualRenderer.transform.DOMoveZ(-0.5f, 0.4f));

        this.sequence.SetLoops(-1, LoopType.Yoyo);
    }
}
