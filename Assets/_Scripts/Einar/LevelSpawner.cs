using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets._Scripts.Utilities.Singleton;

public class LevelSpawner : Singleton<LevelSpawner>
{
    [Header("Level Settings")]
    public int TotalCreaturesToSpawn = 5;
    public int MinigameCount = 4;

    [Header("Spawns")]
    public List<WeightedCreature> CreaturePool;
    public GameObject MinigamePrefab;
    public GameObject EmptyPrefab;

    // To track restricted spawn zones for the buffer
    private HashSet<Vector2Int> RestrictedZones = new HashSet<Vector2Int>();

    public void GenerateLevel(Vector2Int gridSize)
    {
        CreatureTracker.Instance.ClearTracker();

        List<Vector2Int> AvailablePool = new List<Vector2Int>();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                AvailablePool.Add(new Vector2Int(x, y));
            }
        }

        SpawnCreatureShapes(AvailablePool);
        SpawnMinigames(AvailablePool);
        FillRemainingWithEmpties(AvailablePool);
    }

    private WeightedCreature PickRandomCreature(int totalWeight)
    {
        int RandomValue = Random.Range(0, totalWeight);
        int CurrentWeight = 0;

        foreach (WeightedCreature item in CreaturePool)
        {
            CurrentWeight += item.Weight;
            if (RandomValue < CurrentWeight)
            {
                return item;
            }
        }

        return CreaturePool[0]; // Fallback
    }

    private void SpawnCreatureShapes(List<Vector2Int> availablePool)
    {
        // 1. Calculate the total weight of the pool once
        int TotalWeight = 0;
        foreach (WeightedCreature item in CreaturePool)
        {
            TotalWeight += item.Weight;
        }

        // 2. Setup a dictionary to track our spawns
        Dictionary<WeightedCreature, int> SpawnTally = new Dictionary<WeightedCreature, int>();

        for (int i = 0; i < TotalCreaturesToSpawn; i++)
        {
            WeightedCreature PickedData = PickRandomCreature(TotalWeight);
            CreatureShape ShapeTemplate = PickedData.Shape;

            bool IsPlaced = false;
            List<Vector2Int> ShuffledPool = availablePool.OrderBy(x => Random.value).ToList();

            foreach (Vector2Int AnchorPos in ShuffledPool)
            {
                bool IsEvenRow = (AnchorPos.y % 2) == 0;
                Vector2Int[] OffsetsToUse = IsEvenRow ? ShapeTemplate.EvenRowOffsets : ShapeTemplate.OddRowOffsets;

                bool CanFitPerfectly = true;
                List<Vector2Int> TargetPositions = new List<Vector2Int>();

                foreach (Vector2Int Offset in OffsetsToUse)
                {
                    Vector2Int CheckPos = AnchorPos + Offset;

                    if (!availablePool.Contains(CheckPos) || RestrictedZones.Contains(CheckPos))
                    {
                        CanFitPerfectly = false;
                        break;
                    }
                    TargetPositions.Add(CheckPos);
                }

                if (CanFitPerfectly)
                {
                    IsPlaced = true;

                    foreach (Vector2Int Pos in TargetPositions)
                    {
                        PlaceTileOnGrid(Pos, ShapeTemplate.CreaturePrefab);
                        availablePool.Remove(Pos);
                        RestrictedZones.Add(Pos);
                    }

                    CreatureTracker.Instance.RegisterNewCreature(ShapeTemplate, TargetPositions);

                    Vector2Int[] NeighborOffsets;
                    foreach (Vector2Int Pos in TargetPositions)
                    {
                        NeighborOffsets = GetHexOffsets(Pos);
                        foreach (Vector2Int Offset in NeighborOffsets)
                        {
                            RestrictedZones.Add(Pos + Offset);
                        }
                    }

                    // ---> TRACK THE SUCCESSFUL SPAWN <---
                    if (SpawnTally.ContainsKey(PickedData))
                    {
                        SpawnTally[PickedData]++;
                    }
                    else
                    {
                        SpawnTally[PickedData] = 1;
                    }

                    break;
                }
            }

            if (!IsPlaced)
            {
                Debug.LogWarning($"Grid too crowded! Could not fit a {ShapeTemplate.ShapeName}.");
            }
        }

        // Print the creature spawns and weights to console
        PrintSpawnSummary(SpawnTally, TotalWeight);
    }

    // Buffer around spawned creatures
    private Vector2Int[] GetHexOffsets(Vector2Int position)
    {
        bool IsEvenRow = (position.y % 2) == 0;

        if (IsEvenRow)
        {
            return new Vector2Int[] {
                new(0, 1),   // Top-Right
                new(-1, 1),  // Top-Left
                new(-1, 0),  // Left
                new(-1, -1), // Bottom-Left
                new(0, -1),  // Bottom-Right
                new(1, 0)    // Right
            };
        }
        else
        {
            return new Vector2Int[] {
                new(1, 1),   // Top-Right
                new(0, 1),   // Top-Left
                new(-1, 0),  // Left
                new(0, -1),  // Bottom-Left
                new(1, -1),  // Bottom-Right
                new(1, 0)    // Right
            };
        }
    }

    private void SpawnMinigames(List<Vector2Int> availablePool)
    {
        int PlacedCount = 0;
        int Attempts = 0;

        // Loop until we place all minigames or run out of safe attempts
        while (PlacedCount < MinigameCount && Attempts < 100)
        {
            Attempts++;
            if (availablePool.Count == 0) break;

            int RandomIndex = Random.Range(0, availablePool.Count);
            Vector2Int ChosenPos = availablePool[RandomIndex];

            // Check if the chosen spot is safely outside the buffer zones
            if (!RestrictedZones.Contains(ChosenPos))
            {
                PlaceTileOnGrid(ChosenPos, MinigamePrefab);
                availablePool.RemoveAt(RandomIndex);

                // Add the minigame and its neighbors to the restricted zones
                RestrictedZones.Add(ChosenPos);
                Vector2Int[] NeighborOffsets = GetHexOffsets(ChosenPos);
                foreach (Vector2Int Offset in NeighborOffsets)
                {
                    RestrictedZones.Add(ChosenPos + Offset);
                }

                PlacedCount++;
            }
        }
    }

    private void FillRemainingWithEmpties(List<Vector2Int> availablePool)
    {
        foreach (Vector2Int Pos in availablePool)
        {
            PlaceTileOnGrid(Pos, EmptyPrefab);
        }
        availablePool.Clear();
    }

    private void PlaceTileOnGrid(Vector2Int pos, GameObject prefab)
    {
        if (prefab == null) return;

        Vector3Int CellPosition = new Vector3Int(pos.x, pos.y, 0);

        // Extract the required TileEntityBase from the GameObject
        if (prefab.TryGetComponent<TileEntityBase>(out TileEntityBase entityPrefab))
        {
            // Bridge into your existing Tilemap logic
            GridTileAsset Node = ScriptableObject.CreateInstance<GridTileAsset>();

            // Assuming default color is white now that the struct is gone
            Node.Initialize(prefab: entityPrefab, color: Color.white);

            GridManager.Instance.Tilemap.SetTile(CellPosition, Node);
        }
        else
        {
            Debug.LogError($"The prefab {prefab.name} does not have a TileEntityBase component attached!");
        }
    }

    [System.Serializable]
    public struct WeightedCreature
    {
        public CreatureShape Shape;

        [Tooltip("Higher number = more common. E.g., Common = 10, Rare = 5")]
        public int Weight;
    }

    private void PrintSpawnSummary(Dictionary<WeightedCreature, int> spawnTally, int totalWeight)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>--- LEVEL SPAWN SUMMARY ---</b>");

        foreach (var kvp in spawnTally)
        {
            WeightedCreature creature = kvp.Key;
            int count = kvp.Value;

            // Calculate the exact percentage chance
            float chance = ((float)creature.Weight / totalWeight) * 100f;

            sb.AppendLine($"- {creature.Shape.ShapeName}: Spawned <b>{count}</b> time(s) (Chance: {chance:F1}%)");
        }

        sb.AppendLine("---------------------------");
        Debug.Log(sb.ToString());
    }
}