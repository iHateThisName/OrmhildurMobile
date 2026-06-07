using Assets._Scripts.Utilities.Singleton;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class GridManager : Singleton<GridManager> {

    [SerializeField] private Vector2Int size;
    [field: SerializeField] public Grid Grid { get; private set; }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<Tile> randomTiles;

    public Dictionary<Vector2Int, TileEntityBase> TileDictionary { get; private set; } = new Dictionary<Vector2Int, TileEntityBase>();
    public Queue<(Vector2Int pos, TileEntityBase tile)> TileSelectedQueue { get; private set; } = new Queue<(Vector2Int pos, TileEntityBase tile)>();

    private Camera cam;
    private Bounds GridBounds;

    private bool isAllowingTileInteractions = false;
    private bool isGridLandScapeMode = false;
    private bool isScreenLandscape => Screen.width > Screen.height;

    protected override void Awake() {
        base.Awake();
        if (Grid == null) Grid = GetComponent<Grid>();
        this.cam = Camera.main;
    }

    private void OnEnable() {
        GridGameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable() {
        GridGameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(EnumGridGameState state) {
        if (GridGameManager.Instance.AllowedTileInteractionStates.Contains(GridGameManager.Instance.CurrentState)) {
            this.isAllowingTileInteractions = true;
        } else {
            this.isAllowingTileInteractions = false;
        }
    }

    public void Initialize() {
        CheckStartUpOrientation();
        CheckGridRotation();
        GenerateGrid();
        SetCamera();
        GridGameManager.Instance.ChangeGameState(EnumGridGameState.PlayerTurn);
    }

    private void CheckStartUpOrientation() {
        if (!this.isScreenLandscape) {
            // Portrait
            if (!(this.size.x < this.size.y)) {
                int temp = this.size.x;
                this.size.x = this.size.y;
                this.size.y = temp;
            }

            this.isGridLandScapeMode = false;

        } else if (this.isScreenLandscape && this.size.x > this.size.y) {
            // Landscape
            if (this.size.y < this.size.x) {
                int temp = this.size.x;
                this.size.x = this.size.y;
                this.size.y = temp;
            }
            this.isGridLandScapeMode = true;
        }
    }

    private void OnDrawGizmos() {
        if (Grid == null) Grid = GetComponent<Grid>();

        Gizmos.color = Color.green;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 center = Grid.GetCellCenterWorld(cell);
                Vector3 cellSize = Grid.cellSize;
                Gizmos.DrawWireCube(center, cellSize);
            }
        }
    }

    public void RefreshGridAndCamera() {
        CheckGridRotation();
        CalculateBounds();
        SetCamera();
    }

    private void CheckGridRotation() {
        // Check the device orientation.
        if (this.isScreenLandscape && !this.isGridLandScapeMode) {
            // Landscape
            this.Grid.transform.RotateAround(this.GridBounds.center, Vector3.forward, 90f);
            this.isGridLandScapeMode = true;

        } else if (!this.isScreenLandscape && this.isGridLandScapeMode) {
            // Portrait
            if (this.Grid.transform.rotation.z != 0) {
                this.Grid.transform.RotateAround(this.GridBounds.center, Vector3.forward, -90f);
            }
            this.isGridLandScapeMode = false;
        }
    }

    private void SetCamera() {

        bool isOrthographic = cam.orthographic;

        var vertical = this.GridBounds.size.y;
        var horizontal = this.GridBounds.size.x * (float)cam.pixelHeight / (float)cam.pixelWidth;
        Vector3 distanceback = Vector3.back * this.size.magnitude;
        this.cam.transform.position = this.GridBounds.center + distanceback;

        if (isOrthographic) {
            this.cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
        } else {
            // Not the best way to do this esier with a orthographic camera.
            this.cam.transform.LookAt(this.GridBounds.center);
        }
    }

    private void GenerateGrid() {
        // Clear the tile dictionary before generating the grid.
        this.TileDictionary.Clear();

        // Get the center of the grid in world space.
        Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));

        // StartUp the GridBounds with the center of the grid and a size of zero, we will expand it as we add tiles to it.
        this.GridBounds = new Bounds(center, Vector3.zero);

        RegisterExistingTiles();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                if (TileDictionary.ContainsKey(new Vector2Int(x, y))) {
                    // If the tile is already registered, then we can just skip.
                    GridBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
                    continue;

                } else if (!this.tilemap.HasTile(cellPosition) && !this.TileDictionary.ContainsKey(new Vector2Int(x, y))) {
                    // Check if the current selected tile is empty, if it is, select a random tile from the list of tile

                    // Select a random tile
                    Tile randomTile = randomTiles[Random.Range(0, randomTiles.Count)];

                    // Create a new GridTileAsset
                    GridTileAsset node = ScriptableObject.CreateInstance<GridTileAsset>();
                    node.Initialize(prefab: randomTile.TilePrefab, color: randomTile.TileColor);

                    // Set the tile at the current cell position
                    this.tilemap.SetTile(cellPosition, node);

                    // Encapsulate the cell position in the GridBounds
                    GridBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
                } else {
                    // Somthing is wrong, the tile is not empty but also not registered in the TileDictionary, this should not happen, log a warning.
                    Debug.LogWarning($"Tile at position {cellPosition} is not registered in the TileDictionary.");
                }

            }
        }

        CreateGridMargin();
    }


    private void CalculateBounds() {

        // Get the center of the grid in world space.
        Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));
        Bounds newBounds = new Bounds(center, Vector3.zero);

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                newBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
            }
        }

        this.GridBounds = newBounds; // Update the GridBounds with the new calculated bounds.
        CreateGridMargin(); // Add a margin around the grid.
    }
    private void CreateGridMargin() {
        // Expand the GridBounds by the size of the cell to make a margin around the grid.
        this.GridBounds.Expand(this.Grid.cellSize);
        this.GridBounds.Expand(this.Grid.cellSize);
    }

    private void RegisterExistingTiles() {
        // Find all pre-existing Tiles
        foreach (Transform child in this.tilemap.transform) {
            Vector3Int item = this.Grid.WorldToCell(child.position);
            child.name = child.name + $" ({item.x}, {item.y})";

            if (child.TryGetComponent<TileEntityBase>(out TileEntityBase tileEntity)) {
                tileEntity.Initialize(new Vector2Int(item.x, item.y));
                this.TileDictionary[new Vector2Int(item.x, item.y)] = tileEntity;
            } else {
                Debug.LogWarning($"Child at {child.position} does not have a TileEntityBase component.");
            }
        }
    }

    public void InteractWithTile(Vector3Int cellPosition) {
        // Only allow interactions during certain game states, this is to prevent interactions during cutscenes or other non-interactive moments.
        if (!this.isAllowingTileInteractions) return;

        if (TileDictionary.TryGetValue(new Vector2Int(cellPosition.x, cellPosition.y), out TileEntityBase entity)) {
            if (this.TileSelectedQueue.Contains((new Vector2Int(cellPosition.x, cellPosition.y), entity))) {
                // remove from select queue if already selected, this allows the player to deselect a tile by clicking on it again.
                RemoveFromSelectedQueue(entity);
                entity.OnTileClicked(); // handles toggle click
            } else {
                this.TileSelectedQueue.Enqueue((new Vector2Int(cellPosition.x, cellPosition.y), entity));
                entity.OnTileClicked();
            }
        } else {
            //Debug.Log("Tapped out of GridBounds or empty tile!");
        }
    }

    private void RemoveFromSelectedQueue(TileEntityBase entityToRemove) {
        // Rebuild the queue without the removed entity to preserve exact click order
        Queue<(Vector2Int pos, TileEntityBase tile)> updatedQueue = new Queue<(Vector2Int pos, TileEntityBase tile)>();
        foreach (var item in this.TileSelectedQueue) {
            if (item.tile != entityToRemove) {
                updatedQueue.Enqueue(item);
            }
        }
        this.TileSelectedQueue = updatedQueue;
    }

    public void RegisterTile(Vector3Int position, TileEntityBase entity) {
        if (entity == null) return;
        TileDictionary[new Vector2Int(position.x, position.y)] = entity;
    }

    internal void SimulateGrid(bool isPlayerTurn) {
        StringBuilder builder = new StringBuilder();

        foreach ((Vector2Int pos, TileEntityBase tile) in TileSelectedQueue) {
            builder.AppendLine($"Tile: {tile.name}");
            tile.StopAnimations();
        }
        this.TileSelectedQueue.Clear();

        if (isPlayerTurn) {
            Debug.Log($"{GetType().Name}: SimulatingPlayerEnd Grid... \n {builder} \n");
            GridGameManager.Instance.ChangeGameState(EnumGridGameState.EnemyTurn);
        } else {
            Debug.Log($"{GetType().Name}: SimulatingEnemyEnd Grid... \n {builder} \n");
            GridGameManager.Instance.ChangeGameState(EnumGridGameState.PlayerTurn);
        }
    }
}

[System.Serializable]
public struct Tile {
    public TileEntityBase TilePrefab;
    public Color TileColor;
}
