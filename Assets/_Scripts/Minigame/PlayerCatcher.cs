using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCatcher : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [SerializeField] private Transform catcherTransform;
    [SerializeField] private float catchRadius = 0.5f;
    private Vector2 currentTargetPosition = Vector2.zero;

    private bool isMoving = false;
    public void OnBeginDrag(PointerEventData eventData) {
        this.currentTargetPosition = eventData.position;
        this.isMoving = true;
        Move();
    }

    public void OnDrag(PointerEventData eventData) {
        this.currentTargetPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        this.currentTargetPosition = eventData.position;
        this.isMoving = false;


    }

    public bool CheckCatch(Vector2 objectPosition) {
        return Vector2.Distance(this.catcherTransform.position, objectPosition) < this.catchRadius;
    }

    private void Move() {
        while (this.isMoving) {
            // Move the catcher towards the current target position
            Vector3 targetWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(this.currentTargetPosition.x, this.currentTargetPosition.y, 0f));
            targetWorldPosition.z = 0f; // Keep the catcher on the same Z plane
            this.catcherTransform.position = Vector3.MoveTowards(this.catcherTransform.position, targetWorldPosition, Time.deltaTime * 5f);
        }
    }

}