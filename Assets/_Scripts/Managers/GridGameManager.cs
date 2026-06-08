using Assets._Scripts.Utilities.Singleton;
using System.Collections.Generic;
using UnityEngine;

public class GridGameManager : Singleton<GridGameManager> {

    public static event System.Action<EnumGridGameState> OnGameStateChanged; // Might need the previouse sate probally fine with using tuple if we need it.
    public EnumGridGameState CurrentState { get; private set; } = EnumGridGameState.Loading;
    public HashSet<EnumGridGameState> AllowedTileInteractionStates { get; private set; } = new HashSet<EnumGridGameState>() {
        EnumGridGameState.PlayerTurn,
        //EnumGridGameState.EnemyTurn
    };

    [field:SerializeField] public EnumGridTool CurrentTool { get; private set; } = EnumGridTool.None;

    private void Start() {
        ChangeGameState(EnumGridGameState.GeneratingGrid);
    }
    public async void ChangeGameState(EnumGridGameState newState) {
        await Awaitable.NextFrameAsync();
        if (newState != EnumGridGameState.SimulatingPlayerEnd) await Awaitable.WaitForSecondsAsync(3f); // TODO; for testing purposes.

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
                // Handle win state
                break;
            case EnumGridGameState.GameOver:
                // Handle game over state
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
            default:
                return 0;
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
}
