using DG.Tweening;
using Gaskellgames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FallingObjectsMiniGame : MinigameBase {

    private EnumBiomes currentBiome => GameManager.Instance.CurrentBiomeSelected;
    private readonly float catchYPosition = -1f;
    private readonly float outOfBoundsYPosition = -5.5f;
    private readonly float startYPosition = 5f;
    [SerializeField] private Vector2 xBounds = new Vector2(-5f, 5f);
    private float fallingSpeed = 2f;
    private float margin = 0.5f; // Margin to ensure objects don't spawn partially off-screen
    [SerializeField] private Camera fallingObjectsCamera;

    // UI References

    [Header("Falling Object References")]
    [SerializeField] private GameObject seaFalling;
    [SerializeField] private GameObject cliffFalling;
    [SerializeField] private GameObject jaggedMountainsFalling;
    [SerializeField] private List<Transform> fallingObjectPool = new List<Transform>();

    private void Start() {

        this.xBounds = new Vector2(-this.fallingObjectsCamera.orthographicSize * this.fallingObjectsCamera.aspect, this.fallingObjectsCamera.orthographicSize * this.fallingObjectsCamera.aspect);

        // Add a little margin to the bounds to ensure objects don't spawn partially off-screen
        this.xBounds.x += this.margin;
        this.xBounds.y -= this.margin;
    }
    [Button]
    public override void StartMinigame() {
        base.StartMinigame();

        StartFallingObject();
    }

    [Button]
    private void StartFallingObject() {

        Transform selectedObject = fallingObjectPool.FirstOrDefault(e => !e.gameObject.activeInHierarchy);
        if (selectedObject == null) selectedObject = SpawnFallingObject();

        selectedObject.position = new Vector3(UnityEngine.Random.Range(this.xBounds.x, this.xBounds.y), this.startYPosition, 0f);
        selectedObject.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        //sequence.Append(selectedObject.transform.DOMoveY(this.catchYPosition, this.fallingSpeed).SetEase(Ease.Linear));
        //sequence.Append(selectedObject.transform.DORotate(new Vector3(0f, 0f, 360f), this.fallingSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear))

        selectedObject.transform.DOMoveY(this.catchYPosition, 2f).SetEase(Ease.Linear).OnComplete(() => {

            // Check if the object was caught

            selectedObject.transform.DOMoveY(this.outOfBoundsYPosition, 1f).SetEase(Ease.Linear).OnComplete(() => {
                // Object was not caught.
                selectedObject.transform.DOKill();
                selectedObject.gameObject.SetActive(false);

            });
        });

    }

    private Transform SpawnFallingObject() {

        Transform cloneTransform = currentBiome switch {
            EnumBiomes.Cliffs => this.cliffFalling.transform,
            EnumBiomes.Sea => this.seaFalling.transform,
            EnumBiomes.JaggedMountains => this.jaggedMountainsFalling.transform,
            _ => throw new ArgumentOutOfRangeException($"Unhandled biome: {currentBiome}")
        };

        GameObject newObject = Instantiate(cloneTransform.gameObject, Vector3.zero, Quaternion.identity, this.transform);
        newObject.transform.SetParent(cloneTransform.parent);
        this.fallingObjectPool.Add(newObject.transform);
        return newObject.transform;
    }
}
