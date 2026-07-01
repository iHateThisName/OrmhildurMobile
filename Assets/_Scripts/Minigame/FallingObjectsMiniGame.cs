using DG.Tweening;
using Gaskellgames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-9)]
public class FallingObjectsMiniGame : MinigameBase {

    private EnumBiomes currentBiome => GameManager.Instance.CurrentBiomeSelected;
    private readonly float outOfBoundsYPosition = -5.5f;
    private readonly float startYPosition = 5f;
    private readonly float margin = 0.5f; // Margin to ensure objects don't spawn partially off-screen

    [SerializeField] private float catchY = -3f;
    [SerializeField] private float fallDuration = 5f;
    [SerializeField] private Vector2 xBounds = new Vector2(-5f, 5f);

    [field:SerializeField] public Camera FallingObjectsCamera { get; private set; }
    [SerializeField] private PlayerMoverFallingObjects playerMover;

    [Header("Falling Object References")]
    [SerializeField] private GameObject seaFalling;
    [SerializeField] private GameObject cliffFalling;
    [SerializeField] private GameObject jaggedMountainsFalling;
    [SerializeField] private List<Transform> fallingObjectPool = new List<Transform>();

    [Header("Character Sprite")]
    [field:SerializeField, ReadOnly] public CharaterSprite CurrentCharacterSprite { get; private set; }
    [SerializeField] private List<CharaterSprite> characterSprites = new List<CharaterSprite>();

    private void Start() {
        this.isTimeOutWinConditon = true;

        if (this.playerMover == null) {
            this.playerMover = FindAnyObjectByType<PlayerMoverFallingObjects>();
        }

        this.xBounds = new Vector2(-this.FallingObjectsCamera.orthographicSize * this.FallingObjectsCamera.aspect, this.FallingObjectsCamera.orthographicSize * this.FallingObjectsCamera.aspect);

        // Add a little margin to the bounds to ensure objects don't spawn partially off-screen
        this.xBounds.x += this.margin;
        this.xBounds.y -= this.margin;

        CharaterSprite? characterSprite = this.characterSprites.FirstOrDefault(e => e.Biome == this.currentBiome);
        ValidateCharacterSprite(characterSprite);

    }

    private bool ValidateCharacterSprite(CharaterSprite? sprite) {
        bool isValid = true;

        if (!sprite.HasValue) {
            isValid = false;
            throw new InvalidOperationException("No character sprite found for the current biome.");
        }

        if (sprite.Value.ImageRefrence == null) {
            isValid = false;
            throw new InvalidOperationException("Character sprite is missing an image reference.");
        }

        if (sprite.Value.WalkCycle == null || sprite.Value.WalkCycle.Length == 0) {
            isValid = false;
            Debug.LogWarning("Character sprite has no walk cycle sprites assigned.");
        }

        if (sprite.Value.HitSprite == null) {
            isValid = false;
            Debug.LogWarning("Character sprite has no hit sprite assigned.");
        }

        this.CurrentCharacterSprite = sprite.Value;
        return isValid;
    }

    public override void StartMinigame() {
        base.StartMinigame();

        InvokeRepeating(nameof(StartFallingObject), 0f, 1f);
    }

    [Button]
    private void StartFallingObject() {

        Transform selectedObject = fallingObjectPool.FirstOrDefault(e => !e.gameObject.activeInHierarchy);
        if (selectedObject == null) selectedObject = SpawnFallingObject();

        selectedObject.position = new Vector3(UnityEngine.Random.Range(this.xBounds.x, this.xBounds.y), this.startYPosition, 0f);
        selectedObject.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        //sequence.Append(selectedObject.transform.DOMoveY(this.catchYBounds, this.fallingSpeed).SetEase(Ease.Linear));
        //sequence.Append(selectedObject.transform.DORotate(new Vector3(0f, 0f, 360f), this.fallingSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear))


        selectedObject.transform.DOMoveY(this.catchY, this.fallDuration).SetEase(Ease.Linear).OnComplete(() => {
            selectedObject.transform.DOMoveY(this.outOfBoundsYPosition, 3f).SetEase(Ease.Linear).OnComplete(() => {
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

    public void OnFallingObjectHitPlayer() {
        Debug.Log("Falling object hit player!");
        Image imageRefrence = this.CurrentCharacterSprite.ImageRefrence;
        imageRefrence.transform.localScale = this.CurrentCharacterSprite.HitSpriteScale;
        imageRefrence.sprite = this.CurrentCharacterSprite.HitSprite;
        EndMinigame(false);
    }
}

[System.Serializable]
public struct CharaterSprite {
    public EnumBiomes Biome;
    public Image ImageRefrence;

    public Sprite[] IdleCycleSprite;

    [Header("Hit Sprite")]
    public Sprite HitSprite;
    public Vector3 HitSpriteScale;

    [Header("Walk Cycle")]
    public bool isWalkCycleFacingRight;
    public Vector3 WalkCycleScale;
    public Sprite[] WalkCycle;
}
