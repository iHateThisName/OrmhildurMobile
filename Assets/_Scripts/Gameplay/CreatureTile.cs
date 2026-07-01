using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureTile : TileEntityBase, IScannable
{
    [Header("Creature Tile Visuals")]
    [SerializeField] private SpriteRenderer creatureVisualRendere;
    [SerializeField] private SpriteMask mask;

    private Sprite fullyDiscoveredSprite;
    private Vector3 originalScale;
    [field:SerializeField] public EnumCreatureName CreatureName { get; private set; } = EnumCreatureName.None; // Currently only the anchor has the creature name.

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;

    [Header("Magnifying Glass Visuals")]
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite defaultSprite;

    private bool hasBeenDug = false;
    private bool isCreatureAnchor = false;

    private void Awake() {
        // Hide the mask and creature visual by default, they will be later revealed by the player.
        this.mask.gameObject.SetActive(false);
        // Only the anchor tile will show the creature visual, the rest will be hidden behind the mask.
        this.creatureVisualRendere.gameObject.SetActive(false);
    }

    private void OnEnable() => CreatureTracker.OnCreatureDiscovered += HandleCreatureDiscovered;
    private void OnDisable() => CreatureTracker.OnCreatureDiscovered -= HandleCreatureDiscovered;

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        hasBeenDug = false;
    }

    public void SetAsCreatureAnchorTile(CreatureShape creatureInformation)
    {
        this.isCreatureAnchor = true;
        this.creatureVisualRendere.gameObject.SetActive(true);

        // Keep this so the SpriteRenderer respects your raw image aspect ratio 
        // before you apply your custom scaling
        this.creatureVisualRendere.drawMode = SpriteDrawMode.Simple;

        // Set the base information
        this.CreatureName = creatureInformation.CreatureName;
        this.creatureVisualRendere.sprite = creatureInformation.CreatureVisualSprite;
        this.fullyDiscoveredSprite = creatureInformation.CreatureDiscoveredSprite;

        // Apply custom position
        this.creatureVisualRendere.gameObject.transform.localPosition = creatureInformation.CreatureVisualTransform;

        // Apply custom rotation
        this.creatureVisualRendere.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, creatureInformation.CreatureVisualRotation);

        //Apply custom X/Y scale
        this.creatureVisualRendere.gameObject.transform.localScale = new Vector3(
            creatureInformation.CreatureVisualScale.x,
            creatureInformation.CreatureVisualScale.y,
            1f
        );
    }

    public void ApplyScannedVisual()
    {
        if (hasBeenDug) return;

        if (questionMarkSprite != null && this.VisualRenderer != null)
        {
            this.VisualRenderer.sprite = questionMarkSprite;
            this.VisualRenderer.color = Color.white;
        }
    }

    public override void OnTileClicked(EnumGridTool? tool)
    {
        if (hasBeenDug) return;

        if ((tool.HasValue && tool.Value == EnumGridTool.MagnifyingGlass) || (!tool.HasValue && GridGameManager.Instance.CurrentTool == EnumGridTool.MagnifyingGlass))
        {
            if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.MagnifyingGlass))
            {
                PerformRadarScan();
            }
            return;
        }

        if ((tool.HasValue && tool.Value == EnumGridTool.IcePick) || (GridGameManager.Instance.CurrentTool == EnumGridTool.IcePick))
        {
            if (InventoryManager.Instance.TryConsumeToolCharge(EnumGridTool.IcePick))
            {
                hasBeenDug = true;

                base.OnTileClicked(tool);

                //this.VisualRenderer.sprite = defaultSprite;
                //VisualRenderer.color = Color.green;

                this.VisualRenderer.gameObject.SetActive(false);
                this.mask.gameObject.SetActive(true);

                // Ask the tracker if this was the final piece
                bool isFullyDiscovered = CreatureTracker.Instance.ReportTileDug(this.CurrentGridPosition);

                if (isFullyDiscovered)
                {
                    audioSource.PlayOneShot(creatureCompleteClip);

                    //Sparkles
                }
                else
                {
                    audioSource.PlayOneShot(digCreaturePartClip);
                }
            }
            else
            {
                // They clicked the creature with the Hand or no tool
                Debug.Log($"<color=yellow>[CreatureTile]</color> You need a tool to dig this up!");
            }
        }
    }

    private void HandleCreatureDiscovered(HashSet<Vector2Int> creatureCoordinates)
    {
        // If I am the anchor, and my coordinates are in the winning list, swap the sprite!
        if (this.isCreatureAnchor && creatureCoordinates.Contains(this.CurrentGridPosition))
        {
            if (fullyDiscoveredSprite != null)
            {

                StartCoroutine(DiscoverySequence());
            }
        }
    }

    private IEnumerator DiscoverySequence()
    {
        //Lock gamestate
        GridGameManager.Instance.IsResolvingInteraction = true;
        // 1. FIX VISUALS & MASKING
        if (fullyDiscoveredSprite != null)
        {
            this.creatureVisualRendere.sprite = fullyDiscoveredSprite;
            this.creatureVisualRendere.gameObject.transform.localScale = Vector3.one;
        }

        // Break out of the mask so neighboring hexes don't cut it off
        this.creatureVisualRendere.maskInteraction = SpriteMaskInteraction.None;

        // Pop it to the front of the rendering order
        this.creatureVisualRendere.sortingOrder = 100;

        // 2. MOVE TO CENTER OF SCREEN
        Vector3 startPos = this.creatureVisualRendere.transform.position;

        // Find center of the camera
        Camera cam = Camera.main;
        Vector3 targetPos = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        targetPos.z = startPos.z; // Keep the original Z depth

        float moveDuration = 1.0f; // How long it takes to fly to center
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            // Lerp smoothly moves the object from A to B over time
            this.creatureVisualRendere.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        // Ensure it snaps perfectly to center at the end
        this.creatureVisualRendere.transform.position = targetPos;

        // 3. SHAKE EFFECT
        float shakeDuration = 0.5f;
        float shakeIntensity = 0.15f; // Increase this to make the shake more violent
        elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * shakeIntensity;
            this.creatureVisualRendere.transform.position = targetPos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap back to exact center after shaking
        this.creatureVisualRendere.transform.position = targetPos;

        // Optional: Small pause before disappearing
        yield return new WaitForSeconds(0.2f);

        // 4. DISAPPEAR
        this.creatureVisualRendere.gameObject.SetActive(false);

        GridGameManager.Instance.IsResolvingInteraction = false;
    }
}