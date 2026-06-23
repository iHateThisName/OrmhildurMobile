using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureShape", menuName = "Grid/Creature Shape")]
public class CreatureShape : ScriptableObject
{
    public string ShapeName;
    //public Tile CreatureTileAsset; // Using your existing Tile struct
    public GameObject CreaturePrefab;

    [Header("Shape Definition")]
    [Tooltip("Offsets if the Anchor tile is placed on an EVEN Y row (0, 2, 4)")]
    public Vector2Int[] EvenRowOffsets;

    [Tooltip("Offsets if the Anchor tile is placed on an ODD Y row (1, 3, 5)")]
    public Vector2Int[] OddRowOffsets;
}