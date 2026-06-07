using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "GridTileAsset", menuName = "Scriptable Objects/GridTileAsset")]
public class GridTileAsset : TileBase {
    private Color color;
    private TileEntityBase tilePrefab;

    public void Initialize(TileEntityBase prefab, Color color) {
        this.color = color;
        this.tilePrefab = prefab;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        // Tell the tilemap to spawn this game object
        if (tilePrefab != null) {
            tileData.gameObject = tilePrefab.gameObject;
        }
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instancedGameObject) {
        if (instancedGameObject != null) {
            // Grab the TileEntityBase script from the newly spawned GameObject
            if (instancedGameObject.TryGetComponent<TileEntityBase>(out var entity)) {
                // Register it directly to our dictionary in the GridManager!
                GridManager.Instance.RegisterTile(position, entity);
                entity.Initialize(new Vector2Int(position.x, position.y), this.color);
            }
            // Apply the sprite and color to the SpriteRenderer of the instanced gameObject
            instancedGameObject.name = $"Generated Tile ({position.x}, {position.y})";
        }

        return true;
    }
}
