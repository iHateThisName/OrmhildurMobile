using Assets._Scripts.Utilities.Singleton;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : Singleton<GridManager> {

    [SerializeField] private Vector2Int size;
    [field: SerializeField] public Grid Grid { get; private set; }
    [field: SerializeField] public Tilemap Tilemap { get; private set; }
    [SerializeField] private List<Tile> randomTiles;

    public Dictionary<Vector2Int, TileEntityBase> TileDictionary { get; private set; } = new Dictionary<Vector2Int, TileEntityBase>();
    public Queue<(Vector2Int pos, TileEntityBase tile)> TileSelectedQueue { get; private set; } = new Queue<(Vector2Int pos, TileEntityBase tile)>();

    private Camera cam;
    private Bounds GridBounds;

    private bool isAllowingTileInteractions = false;
    private bool isGridLandScapeMode = true;
    private bool isScreenLandscape => Screen.width > Screen.height;

    protected override void Awake() {
        base.Awake();
        if (this.Grid == null) Debug.LogError($"{GetType().Name}: Grid reference is not set in the inspector.");
        if (this.Tilemap == null) Debug.LogError($"{GetType().Name}: Tilemap reference is not set in the inspector.");

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
        //CheckStartUpOrientation();
        //CheckGridRotation();

        // if grid is hex layout then switch the size values
        if (this.Grid.cellLayout == GridLayout.CellLayout.Hexagon) {
            int temp = this.size.x;
            this.size.x = this.size.y;
            this.size.y = temp;
        }

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
        if (Grid == null) return;
        if (Application.isPlaying) return;

        Gizmos.color = Color.green;

        int drawWidth = this.size.x;
        int drawHeight = this.size.y;

        if (this.Grid.cellLayout == GridLayout.CellLayout.Hexagon) {
            drawWidth = this.size.y;
            drawHeight = this.size.x;
        }

        for (int x = 0; x < drawWidth; x++) {
            for (int y = 0; y < drawHeight; y++) {

                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 center = Grid.GetCellCenterWorld(cell);
                Vector3 cellSize = Grid.cellSize;

                if (this.Grid.cellLayout == GridLayout.CellLayout.Rectangle) {
                    Gizmos.DrawWireCube(center, cellSize);

                } else if (this.Grid.cellLayout == GridLayout.CellLayout.Hexagon) {
                    Gizmos.DrawWireSphere(center, cellSize.x / 2);
                }
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

        bool isOrthographic = this.cam.orthographic;

        var vertical = this.GridBounds.size.y;
        var horizontal = this.GridBounds.size.x * (float)this.cam.pixelHeight / (float)this.cam.pixelWidth;
        Vector3 distanceback = Vector3.back * this.size.magnitude;

        if (isOrthographic) {
            this.cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
            this.cam.transform.position = this.GridBounds.center + distanceback;
        } else {
            this.cam.transform.position = this.GridBounds.center + new Vector3(0, 0, this.cam.transform.position.z);
            //this.cam.transform.position = new Vector3((float)this.size.x / 2 - 0.5f, (float)this.size.y / 2 - 0.5f, this.cam.transform.position.z);

        }
    }

    private void GenerateGrid() {
        this.TileDictionary.Clear();
        Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));
        this.GridBounds = new Bounds(center, Vector3.zero);

        // Call the new generation logic
        FindAnyObjectByType<LevelSpawner>().GenerateLevel(this.size);

        // Recalculate bounds based on the newly placed tiles
        CalculateBounds();
    }

    //private void GenerateGrid() {
    //    // Clear the tile dictionary before generating the grid.
    //    this.TileDictionary.Clear();

    //    // Get the center of the grid in world space.
    //    Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));

    //    // StartUp the GridBounds with the center of the grid and a size of zero, we will expand it as we add tiles to it.
    //    this.GridBounds = new Bounds(center, Vector3.zero);

    //    RegisterExistingTiles();

    //    for (int x = 0; x < size.x; x++) {
    //        for (int y = 0; y < size.y; y++) {
    //            Vector3Int cellPosition = new Vector3Int(x, y, 0);

    //            if (TileDictionary.ContainsKey(new Vector2Int(x, y))) {
    //                // If the tile is already registered, then we can just skip.
    //                GridBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
    //                continue;

    //            } else if (!this.Tilemap.HasTile(cellPosition) && !this.TileDictionary.ContainsKey(new Vector2Int(x, y))) {
    //                // Check if the current selected tile is empty, if it is, select a random tile from the list of tile

    //                // Select a random tile
    //                Tile randomTile = randomTiles[Random.Range(0, randomTiles.Count)];

    //                // Create a new GridTileAsset
    //                GridTileAsset node = ScriptableObject.CreateInstance<GridTileAsset>();
    //                node.Initialize(prefab: randomTile.TilePrefab, color: randomTile.TileColor);

    //                // Set the tile at the current cell position
    //                this.Tilemap.SetTile(cellPosition, node);

    //                // Encapsulate the cell position in the GridBounds
    //                GridBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
    //            } else {
    //                // Somthing is wrong, the tile is not empty but also not registered in the TileDictionary, this should not happen, log a warning.
    //                Debug.LogWarning($"Tile at position {cellPosition} is not registered in the TileDictionary.");
    //            }

    //        }
    //    }

    //    CreateGridMargin();
    //}


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
        //this.GridBounds.Expand(this.Grid.cellSize);
    }

    private void RegisterExistingTiles() {
        // Find all pre-existing Tiles
        foreach (Transform child in this.Tilemap.transform) {
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

    public void InteractWithTile(Vector3Int cellPosition, EnumGridTool? tool) {
        Vector2Int cellPositionVector2Int = new(cellPosition.x, cellPosition.y);

        // Only allow interactions during certain game states, this is to prevent interactions during cutscenes or other non-interactive moments.
        if (!this.isAllowingTileInteractions) return;

        // Only interact with tiles that are within the bounds of the grid, this prevents interactions with tiles that are outside of the grid.
        if (!TileDictionary.TryGetValue(cellPositionVector2Int, out TileEntityBase entity)) return;


        if (this.TileSelectedQueue.Contains((cellPositionVector2Int, entity))) {
            // remove from select queue if already selected, this allows the player to deselect a tile by clicking on it again.
            RemoveFromSelectedQueue(entity);
            entity.OnTileClicked(tool); // handles toggle click
        } else {
            this.TileSelectedQueue.Enqueue((cellPositionVector2Int, entity));
            entity.OnTileClicked(tool);
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

    public List<TileEntityBase> GetNeighbors(Vector2Int position) {
        HashSet<TileEntityBase> neighbors = new HashSet<TileEntityBase>();
        Vector2Int[] offsets = GetOffset(position);

        foreach (Vector2Int offset in offsets) {
            Vector2Int currentPosition = position + offset;
            if (TileDictionary.TryGetValue(currentPosition, out TileEntityBase neighbor)) {
                neighbors.Add(neighbor);
            }
        }
        return neighbors.ToList<TileEntityBase>();
    }
    private Vector2Int[] GetOffset(Vector2Int position) { // Make static and Move this in a utility class if we need to use it outside of the GridManager.
        bool isEvenRow = (position.y % 2) == 0;
        bool isHex = GridManager.Instance.Grid.cellLayout == GridLayout.CellLayout.Hexagon;

        Vector2Int[] squareOffsets = {
            Vector2Int.up,      // Up
            Vector2Int.down,    // Down
            Vector2Int.left,    // Left
            Vector2Int.right    // Right
        };
        Vector2Int[] hexEvenOffsets = {
            new(0, 1),   // Up
            new(0, -1),  // Down
            new(-1, 0),  // Left
            new(1, 0),   // Right
            new(-1, 1),  // Upper Left
            new(-1, -1)  // Lower Left
        };
        Vector2Int[] hexOddOffsets = {
            new(0, 1),   // Up
            new(0, -1),  // Down
            new(-1, 0),  // Left
            new(1, 0),   // Right
            new(1, 1),   // Upper Right
            new(1, -1)   // Lower Right
        };

        if (!isHex) {
            return squareOffsets;

        } else if (isEvenRow) {
            return hexEvenOffsets;

        } else {
            return hexOddOffsets;
        }
    }
}

[System.Serializable]
public struct Tile {
    public TileEntityBase TilePrefab;
    public Color TileColor;
}
