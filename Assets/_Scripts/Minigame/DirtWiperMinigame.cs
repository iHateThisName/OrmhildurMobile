using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DirtWiperMinigame : MinigameBase
{
    [Header("Dirt Wiping Setup")]
    [Tooltip("The main camera the player actually looks through.")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("The camera that strictly renders the brush strokes to the RenderTexture.")]
    [SerializeField] private Camera maskCamera;

    [SerializeField] private GameObject brushPrefab;
    [SerializeField] private Transform brushContainer;
    [SerializeField] private RenderTexture maskRenderTexture;

    [Header("Win Condition")]
    [Tooltip("Percentage of the screen that needs to be wiped clean (0.0 to 1.0)")]
    [SerializeField, Range(0f, 1f)] private float requiredCleanPercentage = 0.85f;
    [Tooltip("How often to check the pixels (Checking every frame is bad for performance)")]
    [SerializeField] private float checkInterval = 0.5f;

    private Texture2D textureCheck;
    private Vector3 lastMousePosition;

    // Optional: To make the brush strokes continuous instead of dotted if the mouse moves fast
    private float brushSpacing = 0.1f;

    public void Start()
    {
        base.StartMinigame(); // Sets isPlaying = true and resets timer

        // Initialize the Texture2D used to read the Render Texture
        // Sized down slightly if performance becomes an issue, but matching RT size is safest for accuracy
        textureCheck = new Texture2D(maskRenderTexture.width, maskRenderTexture.height, TextureFormat.RGBA32, false);

        StartCoroutine(CheckWinConditionRoutine());
    }

    public override void StartMinigame()
    {
        base.StartMinigame(); // Sets isPlaying = true and resets timer

        // Initialize the Texture2D used to read the Render Texture
        // Sized down slightly if performance becomes an issue, but matching RT size is safest for accuracy
        textureCheck = new Texture2D(maskRenderTexture.width, maskRenderTexture.height, TextureFormat.RGBA32, false);

        StartCoroutine(CheckWinConditionRoutine());
    }

    protected override void Update()
    {
        base.Update(); // Critical: Keeps the TimeLimit logic running!

        if (!isPlaying) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f;

            // USE MAIN CAMERA HERE
            lastMousePosition = mainCamera.ScreenToWorldPoint(mousePos);
            Instantiate(brushPrefab, lastMousePosition, Quaternion.identity, brushContainer);
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f;

            // USE MAIN CAMERA HERE
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            if (Vector3.Distance(worldPos, lastMousePosition) > brushSpacing)
            {
                Instantiate(brushPrefab, worldPos, Quaternion.identity, brushContainer);
                lastMousePosition = worldPos;
            }
        }
    }

    private IEnumerator CheckWinConditionRoutine()
    {
        // Periodically check the pixels while the game is active
        while (isPlaying)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckCleanPercentage();
        }
    }

    private void CheckCleanPercentage()
    {
        // 1. Set our mask as the active Render Texture
        RenderTexture.active = maskRenderTexture;

        // 2. Copy the pixels into our Texture2D
        textureCheck.ReadPixels(new Rect(0, 0, maskRenderTexture.width, maskRenderTexture.height), 0, 0);
        textureCheck.Apply();

        // 3. Clear the active Render Texture
        RenderTexture.active = null;

        // 4. Count the pixels
        Color[] pixels = textureCheck.GetPixels();
        int cleanPixels = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            // Assuming your brush prefab is painting white/opaque onto a transparent/black background.
            // Adjust this check depending on how your shader is reading the mask (r channel, alpha channel, etc.)
            if (pixels[i].a > 0.1f)
            {
                cleanPixels++;
            }
        }

        // 5. Calculate percentage
        float currentCleanPercentage = (float)cleanPixels / pixels.Length;

        if (currentCleanPercentage >= requiredCleanPercentage)
        {
            EndMinigame(true);
        }
    }

    protected override void EndMinigame(bool wasVictorious)
    {
        StopAllCoroutines(); // Stop checking pixels
        base.EndMinigame(wasVictorious);
    }
}