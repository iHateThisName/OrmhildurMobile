using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerGridInputHandler : Assets._Scripts.Utilities.Singleton.Singleton<PlayerGridInputHandler> {

    [SerializeField] private InputActionReference tap;
    [SerializeField] private float cooldownDuration = 0.25f;

    private InputAction tapAction;
    private bool isOnCooldown = false;

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
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;

        // Get the integer index of the cell
        Vector3Int cellPosition = GridManager.Instance.Grid.WorldToCell(worldPosition);

        // Pass this to the GridManager
        GridManager.Instance.InteractWithTile(cellPosition);

        // Wait for the cooldown duration before allowing another input
        await Awaitable.WaitForSecondsAsync(this.cooldownDuration);
        this.isOnCooldown = false;
    }
}