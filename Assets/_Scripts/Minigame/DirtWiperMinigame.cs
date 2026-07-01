using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DirtWiperMinigame : MinigameBase
{
    [Header("Cameras")]
    [Tooltip("The camera the player looks through (can be a local minigame camera)")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("The camera strictly rendering to the RenderTexture")]
    [SerializeField] private Camera maskCamera;

    [Header("Brushes & Masking")]
    [SerializeField] private GameObject brushPrefab;
    [SerializeField] private Transform brushContainer;
    [SerializeField] private RenderTexture maskRenderTexture;
    private float brushSpacing = 0.1f;

    [Header("Sponge Cursor Visual")]
    [Tooltip("The cloth graphic that follows the mouse/finger")]
    [SerializeField] private GameObject spongePrefab;
    private GameObject activeSponge;

    [Header("Win Condition")]
    [Tooltip("Lower this! The sprite doesn't fill the whole screen. Use the debug log to find the sweet spot.")]
    [SerializeField, Range(0f, 1f)] private float requiredCleanPercentage = 0.15f;
    [SerializeField] private float checkInterval = 0.5f;

    private Texture2D textureCheck;
    private Vector3 lastPointerPosition;

    // Tracks the current percentage for the debug log
    private float currentCleanPercentage = 0f;

    public override void StartMinigame()
    {
        ClearRenderTexture();

        if (spongePrefab != null)
        {
            activeSponge = Instantiate(spongePrefab, transform);
            // Hiding the cursor only matters for PC, but is harmless on mobile
            Cursor.visible = false;
        }

        textureCheck = new Texture2D(maskRenderTexture.width, maskRenderTexture.height, TextureFormat.RGBA32, false);
        currentCleanPercentage = 0f;

        base.StartMinigame();
        StartCoroutine(CheckWinConditionRoutine());
    }

    private void ClearRenderTexture()
    {
        RenderTexture currentActive = RenderTexture.active;
        RenderTexture.active = maskRenderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = currentActive;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPlaying) return;

        UpdateSpongePosition();
        HandleInput();
    }

    private void UpdateSpongePosition()
    {
        // Changed Mouse.current to Pointer.current
        if (activeSponge == null || Pointer.current == null) return;

        // Optionally, hide the sponge when not touching on mobile so it doesn't awkwardly float
        if (!Pointer.current.press.isPressed && Touchscreen.current != null)
        {
            activeSponge.SetActive(false);
            return;
        }

        activeSponge.SetActive(true);
        Vector3 pointerPos = Pointer.current.position.ReadValue();
        pointerPos.z = 10f;
        activeSponge.transform.position = mainCamera.ScreenToWorldPoint(pointerPos);
    }

    private void HandleInput()
    {
        // Changed Mouse.current to Pointer.current
        if (Pointer.current == null) return;

        Vector3 pointerPos = Pointer.current.position.ReadValue();
        pointerPos.z = 10f;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(pointerPos);

        // Pointer.current.press replaces Mouse.current.leftButton
        if (Pointer.current.press.wasPressedThisFrame)
        {
            lastPointerPosition = worldPos;
            Instantiate(brushPrefab, lastPointerPosition, Quaternion.identity, brushContainer);
        }
        else if (Pointer.current.press.isPressed)
        {
            if (Vector3.Distance(worldPos, lastPointerPosition) > brushSpacing)
            {
                Instantiate(brushPrefab, worldPos, Quaternion.identity, brushContainer);
                lastPointerPosition = worldPos;
            }
        }
    }

    private IEnumerator CheckWinConditionRoutine()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckCleanPercentage();
        }
    }

    private void CheckCleanPercentage()
    {
        RenderTexture.active = maskRenderTexture;
        textureCheck.ReadPixels(new Rect(0, 0, maskRenderTexture.width, maskRenderTexture.height), 0, 0);
        textureCheck.Apply();
        RenderTexture.active = null;

        Color[] pixels = textureCheck.GetPixels();
        int cleanPixels = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a > 0.1f) cleanPixels++;
        }

        currentCleanPercentage = (float)cleanPixels / pixels.Length;

        if (currentCleanPercentage >= requiredCleanPercentage)
        {
            EndMinigame(true);
        }
    }

    protected override void EndMinigame(bool wasVictorious)
    {
        StopAllCoroutines();

        if (activeSponge != null) Destroy(activeSponge);
        Cursor.visible = true;

        Debug.Log($"<color=cyan>[DirtWiperMinigame]</color> Game Ended. Player cleaned: <b>{(currentCleanPercentage * 100f):0.00}%</b> of the total Render Texture area.");

        base.EndMinigame(wasVictorious);
    }
}