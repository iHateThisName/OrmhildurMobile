using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class ProtoManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 5;
    [SerializeField] private int depth = 7;
    [SerializeField] private float tileSize = 1.1f;

    [Header("Game Rules")]
    [SerializeField] private int startingDigs = 15;

    [Header("References")]
    [SerializeField] private ProtoTile tilePrefab;
    [SerializeField] private Camera mainCamera;

    [Header("UI & Minigame")]
    [SerializeField] private GameObject trapMinigamePrefab;
    [SerializeField] private TextMeshProUGUI gameOverText; // NEW: Drag your UI text here

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digEmptyClip;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;
    [SerializeField] private AudioClip trapHitClip;

    private ProtoTile[,] grid;
    private int currentDigs;

    // --- State & Stat Trackers ---
    private Dictionary<int, int> creatureHealth = new Dictionary<int, int>();
    private bool isMinigameActive = false;
    private int minigameClicksRequired = 0;
    private GameObject activeMinigameCube;

    //End game trackers
    private bool isGameOver = false;
    private int trapsHit = 0;
    private int creaturesFullyFound = 0;
    private int totalCreaturesInLevel = 0;

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Ensure text is hidden at the start
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);

        currentDigs = startingDigs;
        GenerateGrid();

        // Count how many unique creatures exist in the level based on the dictionary
        totalCreaturesInLevel = creatureHealth.Count;
    }

    private void Update()
    {
        HandleInput();
    }

    private void GenerateGrid()
    {
        grid = new ProtoTile[width, depth];

        float startX = -((width - 1) * tileSize) / 2f;
        float startZ = -((depth - 1) * tileSize) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float posX = startX + (x * tileSize);
                float posZ = startZ + (z * tileSize);

                Vector3 spawnPos = new Vector3(posX, 0, posZ);
                ProtoTile spawnedTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);

                (TileType type, int id) = DetermineTileSetup(x, z);
                spawnedTile.Setup(x, z, type, id);
                spawnedTile.transform.SetParent(transform);

                grid[x, z] = spawnedTile;
            }
        }
    }

    private (TileType, int) DetermineTileSetup(int x, int z)
    {
        if ((x == 1 || x == 2) && (z == 1 || z == 2))
        {
            RegisterCreaturePart(1);
            return (TileType.Creature, 1);
        }

        if (x == 4 && (z >= 4 && z <= 6))
        {
            RegisterCreaturePart(2);
            return (TileType.Creature, 2);
        }

        if (x == 0 && z == 5) return (TileType.Trap, -1);
        if (x == 3 && z == 2) return (TileType.Trap, -1);

        return (TileType.Empty, -1);
    }

    private void RegisterCreaturePart(int id)
    {
        if (creatureHealth.ContainsKey(id))
            creatureHealth[id]++;
        else
            creatureHealth.Add(id, 1);
    }

    private void HandleInput()
    {
        // Block all input if the game is over
        if (isGameOver) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (isMinigameActive)
                {
                    if (hit.collider.gameObject == activeMinigameCube)
                    {
                        ProcessMinigameClick();
                    }
                    return;
                }

                if (hit.collider.TryGetComponent(out ProtoTile tappedTile))
                {
                    ProcessDig(tappedTile);
                }
            }
        }
    }

    private void ProcessDig(ProtoTile tile)
    {
        if (currentDigs <= 0 || tile.IsRevealed) return;

        tile.Reveal();

        switch (tile.Type)
        {
            case TileType.Empty:
                currentDigs--;
                if (digEmptyClip != null) audioSource.PlayOneShot(digEmptyClip);

                CheckForTracks(tile);
                Debug.Log($"Digs remaining: {currentDigs}");
                break;

            case TileType.Creature:
                creatureHealth[tile.CreatureID]--;

                if (creatureHealth[tile.CreatureID] <= 0)
                {
                    creaturesFullyFound++; // Track the win condition
                    if (creatureCompleteClip != null) audioSource.PlayOneShot(creatureCompleteClip);
                    Debug.Log($"<color=green>SUCCESS: You fully uncovered Creature {tile.CreatureID}!</color>");
                }
                else
                {
                    if (digCreaturePartClip != null) audioSource.PlayOneShot(digCreaturePartClip);
                }
                break;

            case TileType.Trap:
                trapsHit++; // Track the stats
                if (trapHitClip != null) audioSource.PlayOneShot(trapHitClip);
                TriggerTrapMinigame();
                break;
        }

        // Check for Win/Loss after every dig
        CheckEndGame();
    }

    private void CheckForTracks(ProtoTile dugTile)
    {
        ProtoTile closestCreatureTile = null;
        float closestDistance = float.MaxValue;

        foreach (ProtoTile tile in grid)
        {
            if (tile.Type == TileType.Creature && !tile.IsRevealed)
            {
                float dist = Mathf.Abs(dugTile.GridX - tile.GridX) + Mathf.Abs(dugTile.GridZ - tile.GridZ);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestCreatureTile = tile;
                }
            }
        }

        if (closestDistance <= 1f && closestCreatureTile != null)
        {
            Vector3 direction = (closestCreatureTile.transform.position - dugTile.transform.position).normalized;
            dugTile.ShowTrackFeedback(direction);
        }
    }

    private void TriggerTrapMinigame()
    {
        isMinigameActive = true;
        minigameClicksRequired = 10;

        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 2f;
        activeMinigameCube = Instantiate(trapMinigamePrefab, spawnPos, Quaternion.identity);
    }

    private void ProcessMinigameClick()
    {
        minigameClicksRequired--;
        activeMinigameCube.transform.localScale *= 0.9f;

        if (digEmptyClip != null) audioSource.PlayOneShot(digEmptyClip);

        if (minigameClicksRequired <= 0)
        {
            Destroy(activeMinigameCube);
            isMinigameActive = false;

            // Re-check end game here if hit a trap on the last dig!
            CheckEndGame();
        }
    }

    // --- End Game Logic ---
    private void CheckEndGame()
    {
        // Don't trigger game over while in a trap minigame
        if (isMinigameActive) return;

        bool playerWon = (creaturesFullyFound >= totalCreaturesInLevel);
        bool playerLost = (currentDigs <= 0);

        if (playerWon || playerLost)
        {
            isGameOver = true;

            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);

                string title = playerWon ? "<color=green>YOU WIN!</color>" : "<color=red>OUT OF DIGS!</color>";

                gameOverText.text = $"{title}\n\n\n" +
                                    $"Creatures Found: {creaturesFullyFound} / {totalCreaturesInLevel}\n\n" +
                                    $"Traps Hit: {trapsHit}";
            }
        }
    }
}