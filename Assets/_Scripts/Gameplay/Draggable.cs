using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    [SerializeField] private Image draggableImage;
    [field: SerializeField] public EnumGridTool IsGridTool { get; private set; } = EnumGridTool.None;

    private Transform parentTransform;
    private Vector3 originalPosition;

    private void Start() {
        if (draggableImage == null) {
            draggableImage = GetComponent<Image>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        this.parentTransform = this.transform.parent;
        this.originalPosition = this.transform.position;

        transform.SetParent(transform.root); // canvas root
        transform.SetAsLastSibling(); // Ensure the dragged object is rendered on top of other UI elements

        this.draggableImage.raycastTarget = false; // Disable raycast target to allow events to pass through

        GridGameManager.Instance.CurrentTool = IsGridTool;

        Debug.Log($"OnBeginDrag: {this.name}, IsGridTool: {this.IsGridTool}");
    }

    public void OnDrag(PointerEventData eventData) {
        //Pointer.current.position.ReadValue()
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.SetParent(this.parentTransform);
        this.transform.position = this.originalPosition;
        this.draggableImage.raycastTarget = true; // Re-enable raycast target

        PlayerGridInputHandler.Instance.ProcessPointerClick(tool: IsGridTool != EnumGridTool.None ? IsGridTool : null); // Process the click event after dragging ends
        GridGameManager.Instance.CurrentTool = EnumGridTool.Hand;
    }
}
