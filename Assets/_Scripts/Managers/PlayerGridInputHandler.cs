using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerGridInputHandler : Assets._Scripts.Utilities.Singleton.Singleton<PlayerGridInputHandler> {

    [SerializeField] private InputActionReference tap;
    [SerializeField] private float cooldownDuration = 0.25f;

    private InputAction tapAction;
    private bool isOnCooldown = false;

    private Plane raycastHitPlane;
    private void OnEnable() {
        this.tapAction = tap.action;
        this.tapAction.Enable();
    }

    private void OnDisable() {
        this.tapAction.Disable();
    }

    private void Update() {
        if (this.tapAction.WasPerformedThisFrame()) {
            ProcessPointerClick();
        }
    }

    private void Start() {
        this.raycastHitPlane = CreateGridPlaneToRaycastHit();
    }

    /// <summary>
    /// Processes the pointer click.
    /// We call this method from Update() rather than subscribing to the InputAction's 'performed' event.
    /// Polling via WasPerformedThisFrame() is necessary because calling EventSystem.current.IsPointerOverGameObject() 
    /// directly inside an input event callback causes a warning, as it queries the UI state from the previous frame.
    /// Moving it to Update() ensures the EventSystem has processed the current frame before we check for UI clicks.
    /// </summary>
    private async void ProcessPointerClick() {
        if (this.isOnCooldown) return;
        this.isOnCooldown = true;

        // Check if pointer is over a UI element. If so, ignore the world click.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            this.isOnCooldown = false;
            return;
        }

        // Read pointer position from the New Input System (works for Touch and Mouse)
        Vector2 screenPosition = Pointer.current.position.ReadValue();

        // Convert to World, then to Grid 
        Camera mainCamera = Camera.main;
        Vector3 worldPosition;

        if (mainCamera.orthographic) {
            worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
        } else {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (this.raycastHitPlane.Raycast(ray, out float distance)) {
                worldPosition = ray.GetPoint(distance);
            } else {
                this.isOnCooldown = false;
                return;
            }
        }

        // Get the integer index of the cell
        Vector3Int cellPosition = GridManager.Instance.Grid.WorldToCell(worldPosition);

        // Check if the cell exists in the grid dictionary
        if (!GridManager.Instance.TileDictionary.ContainsKey(new Vector2Int(cellPosition.x, cellPosition.y))) {
            this.isOnCooldown = false;
            return;
        }

        // Pass this to the GridManager
        GridManager.Instance.InteractWithTile(cellPosition);

        // Wait for the cooldown duration before allowing another input
        await Awaitable.WaitForSecondsAsync(this.cooldownDuration);
        this.isOnCooldown = false;
    }

    private static Plane CreateGridPlaneToRaycastHit() {
        // Create a plane based on the Grid's position and forward direction
        Transform gridTransform = GridManager.Instance.Grid.transform;
        Plane gridPlane = new Plane(gridTransform.forward, gridTransform.position);
        return gridPlane;
    }
}