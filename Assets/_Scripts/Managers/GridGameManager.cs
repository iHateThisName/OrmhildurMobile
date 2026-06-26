using Assets._Scripts.Utilities.Singleton;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGameManager : Singleton<GridGameManager> {

    public static event System.Action<EnumGridGameState> OnGameStateChanged; // Might need the previouse sate probally fine with using tuple if we need it.
    public EnumGridGameState CurrentState { get; private set; } = EnumGridGameState.Loading;
    public HashSet<EnumGridGameState> AllowedTileInteractionStates { get; private set; } = new HashSet<EnumGridGameState>() {
        EnumGridGameState.PlayerTurn,
        //EnumGridGameState.EnemyTurn
    };

    [SerializeField] private EnumGridTool _currentTool;

    [field: SerializeField] public bool IsResolvingInteraction { get; set; } = false;

    private EnumGridTool _previousTool;
    public EnumGridTool CurrentTool { get => _currentTool; set { ValidateToolChange(value); } }

    public static event System.Action<EnumGridTool> OnToolChanged;

    private void Start() {
        ChangeGameState(EnumGridGameState.GeneratingGrid);
        CurrentTool = EnumGridTool.Hand;
    }
    private void OnValidate() {
        if (this._currentTool != this._previousTool) {
            this.CurrentTool = this._currentTool;
        }
    }

    private void ValidateToolChange(EnumGridTool value) {
        if (_currentTool == value) return;
        this._currentTool = value;
        this._previousTool = value;
        OnToolChanged?.Invoke(this._currentTool);
    }

    private void OnEnable()
    {
        // Listen to the inventory updates
        InventoryManager.OnToolChargeChanged += HandleToolChargeChanged;
        CreatureTracker.OnAllCreaturesFound += HandleLevelComplete;
    }



    private void OnDisable()
    {
        // Always unsubscribe when destroyed to prevent memory leaks
        InventoryManager.OnToolChargeChanged -= HandleToolChargeChanged;
        CreatureTracker.OnAllCreaturesFound -= HandleLevelComplete;
    }

    private void HandleLevelComplete()
    {
        ChangeGameState(EnumGridGameState.Win);
    }

    private void HandleToolChargeChanged(EnumGridTool tool, int remainingCharges)
    {
        if (tool == EnumGridTool.IcePick)
        {
            CheckGameOverConditionAsync();
        }
    }

    private async void CheckGameOverConditionAsync()
    {
        //Pause the check while a minigame, animation, or treasure is resolving
        while (IsResolvingInteraction)
        {
            await Awaitable.NextFrameAsync();
        }

        //Wait for the end of the current frame just to be safe
        await Awaitable.EndOfFrameAsync();

        //Check first if player won
        if (CreatureTracker.Instance.AreAllCreaturesComplete)
        {
            return;
        }

        //Lose if out of picks
        int picks = InventoryManager.Instance.ToolCharges.ContainsKey(EnumGridTool.IcePick) ? InventoryManager.Instance.ToolCharges[EnumGridTool.IcePick] : 0;

        if (picks <= 0 && this.CurrentState != EnumGridGameState.GameOver && this.CurrentState != EnumGridGameState.Win)
        {
            Debug.Log("<color=red>[Game Over]</color> Player ran out of Ice Picks!");
            ChangeGameState(EnumGridGameState.GameOver);
        }
    }

    public async void ChangeGameState(EnumGridGameState newState) {
        await Awaitable.NextFrameAsync();
        //if (newState != EnumGridGameState.SimulatingPlayerEnd) await Awaitable.WaitForSecondsAsync(3f); // TODO; for testing purposes.

        this.CurrentState = newState;

        switch (newState) {
            case EnumGridGameState.Loading:
                // Handle loading state
                Debug.Log("Loading game...");

                // Placeholder should be called by a loading manager or similar.
                await Awaitable.NextFrameAsync();
                ChangeGameState(EnumGridGameState.GeneratingGrid);

                break;
            case EnumGridGameState.GeneratingGrid:
                // Handle generating grid state
                GridManager.Instance.Initialize();
                break;
            case EnumGridGameState.PlayerTurn:
                // Handle player turn state
                break;
            case EnumGridGameState.SimulatingPlayerEnd:
                // Handle simulating state

                // Here simulating the animation or "gravity" effect of the grid after the player has made their move.
                GridManager.Instance.SimulateGrid(isPlayerTurn: true);
                break;
            case EnumGridGameState.EnemyTurn:
                // Handle enemy turn state
                GridManager.Instance.SimulateGrid(isPlayerTurn: false);
                break;
            case EnumGridGameState.Win:
                Debug.Log("YOU WIN");
                break;
            case EnumGridGameState.GameOver:
                Debug.Log("GAME OVER DUDE");
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    public int GetToolDamagePoint(EnumGridTool tool) {
        switch (tool) {
            case EnumGridTool.IcePick:
                return 1;
            case EnumGridTool.Hammer:
                return 3;
            case EnumGridTool.MagnifyingGlass:
                return 0;
            default:
                return 0;
        }
    }
    public CreatureTile GetCreatureTileByGridPosition(Vector2Int position) {

        // Retrieve the tile at the anchor position from the GridManager's TileDictionary.
        if (GridManager.Instance.TileDictionary.TryGetValue(key: position, out TileEntityBase tile)) {

            // Attempt to cast the tile to a CreatureTile assuming that all creature uses the CreatureTile class.
            CreatureTile creature = tile as CreatureTile;
            if (creature == null) {
                Debug.LogError($"GetCreatureTileByGridPosition: Failed to retrieve creature tile at position: {position}");
                return null;
            } else {
                return creature;
            }
        } else {
            Debug.LogError($"GetCreatureTileByGridPosition: Failed to retrieve tile at position: {position}");
            return null;
        }
    }
}
public enum EnumGridGameState {
    Loading,
    GeneratingGrid,
    PlayerTurn,
    SimulatingPlayerEnd,
    EnemyTurn,
    SimulatingEnemyEnd,
    Win,
    GameOver,
    MinigameActive,
}
