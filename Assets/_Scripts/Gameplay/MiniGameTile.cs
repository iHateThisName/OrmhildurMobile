using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class MiniGameTile : TileEntityBase
{
    [SerializeField] bool isMinigameActive = false;
    [SerializeField] int minigameClicksRequired = 10;
    [SerializeField] GameObject trapMinigamePrefab;

    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip miniGameClip;

    [SerializeField] Camera mainCamera;

    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
        mainCamera = Camera.main;
    }

    public override void OnTileClicked()
    {
        VisualRenderer.color = Color.red;

        isMinigameActive = true;
        minigameClicksRequired = 10;

        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 2f;
        Instantiate(trapMinigamePrefab, spawnPos, Quaternion.identity);
        ProcessMinigameClick();
    }

    //MINIGAME MESS
    private void ProcessMinigameClick()
    {
        minigameClicksRequired--;
        trapMinigamePrefab.transform.localScale *= 0.9f;

        if (miniGameClip != null) audioSource.PlayOneShot(miniGameClip);

        if (minigameClicksRequired <= 0)
        {
            Destroy(trapMinigamePrefab);
            isMinigameActive = false;

            // Re-check end game here if hit a trap on the last dig!
            //CheckEndGame();
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Block all input if the game is over
        //if (isGameOver) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (isMinigameActive)
                {
                    if (hit.collider.gameObject == trapMinigamePrefab)
                    {
                        ProcessMinigameClick();
                    }
                    return;
                }

                //if (hit.collider.TryGetComponent(out ProtoTile tappedTile))
                //{
                //    ProcessDig(tappedTile);
                //}
            }
        }
    }
}
