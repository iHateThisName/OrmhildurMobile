using DG.Tweening;
using Gaskellgames;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMoverFallingObjects : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {

    [Header("References")]
    [field: SerializeField] public Transform CatcherTransform { get; private set; }
    [SerializeField] private FallingObjectsMiniGame miniGameController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 targetWorldPosition;
    private bool isMoving;

    [field: SerializeField, ReadOnly] public Vector2 xBounds { get; private set; } = new Vector2(-7.5f, 7.5f);
    [field: SerializeField, ReadOnly] public Vector2 yBounds { get; private set; } = new Vector2(-4.5f, 4.5f);

    private RectTransform walkableRectTransform;

    private Camera cam;
    private Sequence walkSequence;
    [SerializeField, ReadOnly] private bool isFacingRight;

    private void Awake() {
        this.cam = miniGameController.FallingObjectsCamera;
        this.targetWorldPosition = this.CatcherTransform.position;
    }

    private void Start() {

        this.walkableRectTransform = this.GetComponent<RectTransform>();

        //this.xBounds = new Vector2(-this.walkableRectTransform.rect.width / 2f, this.walkableRectTransform.rect.width / 2f);
        //this.yBounds = new Vector2(-this.walkableRectTransform.rect.height / 2f, this.walkableRectTransform.rect.height / 2f);

        // WORLD SPACE bounds from UI element
        Vector3[] corners = new Vector3[4];
        walkableRectTransform.GetWorldCorners(corners);

        xBounds = new Vector2(corners[0].x, corners[2].x);
        yBounds = new Vector2(corners[0].y, corners[2].y);

        Image reference = this.miniGameController.CurrentCharacterSprite.ImageRefrence;
        Sprite[] walkSprite = this.miniGameController.CurrentCharacterSprite.WalkCycle;
        reference.transform.localScale = this.miniGameController.CurrentCharacterSprite.WalkCycleScale;
        CreateWalkCycle(reference, walkSprite, 0.25f);

        this.isFacingRight = this.miniGameController.CurrentCharacterSprite.isWalkCycleFacingRight;
    }

    private void Update() {
        if (!isMoving) return;

        this.CatcherTransform.position = Vector3.MoveTowards(
            this.CatcherTransform.position,
            this.targetWorldPosition,
            this.moveSpeed * Time.deltaTime);

        DetectDirection();
    }

    private void DetectDirection() {
        float deltax = this.targetWorldPosition.x - this.CatcherTransform.position.x;
        if (Mathf.Abs(deltax) > 0.01f) {
            Transform spriteTransform = this.miniGameController.CurrentCharacterSprite.ImageRefrence.transform;
            Vector3 currentLocalScale = spriteTransform.localScale;
            if (deltax > 0 && !this.isFacingRight) {
                // Right
                currentLocalScale.x = Mathf.Abs(currentLocalScale.x);
                spriteTransform.localScale = currentLocalScale;
                this.isFacingRight = true;

            } else if (deltax < 0 && this.isFacingRight) {
                // Left
                currentLocalScale.x = -Mathf.Abs(currentLocalScale.x);
                spriteTransform.localScale = currentLocalScale;
                this.isFacingRight = false;
            }
        }
    }

    private void CreateWalkCycle(Image image, Sprite[] cycle, float frameTime) {
        walkSequence?.Kill();

        walkSequence = DOTween.Sequence()
            //.SetAutoKill(false)
            .Pause();

        for (int i = 0; i < cycle.Length; i++) {
            int index = i;

            walkSequence.AppendCallback(() => {
                image.sprite = cycle[index];
            });

            walkSequence.AppendInterval(frameTime);
        }

        walkSequence.SetLoops(-1);
    }

    public void OnPointerDown(PointerEventData eventData) {
        isMoving = true;
        this.targetWorldPosition = UpdateTarget(eventData.position);
        this.walkSequence.Play();
    }

    public void OnDrag(PointerEventData eventData) {
        this.targetWorldPosition = UpdateTarget(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData) {
        isMoving = false;
        this.walkSequence.Pause();
    }

    private Vector2 UpdateTarget(Vector2 screenPos) {
        float zDistance = Mathf.Abs(CatcherTransform.position.z - cam.transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));

        worldPos.x = Mathf.Clamp(worldPos.x, this.xBounds.x, this.xBounds.y);
        worldPos.y = Mathf.Clamp(worldPos.y, this.yBounds.x, this.yBounds.y);
        worldPos.z = this.CatcherTransform.position.z;
        return worldPos;
    }
}