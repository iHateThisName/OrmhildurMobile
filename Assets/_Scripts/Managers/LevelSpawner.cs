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
    public int TreasureCount = 2; 
    public int TrackCount = 3;   

    [Header("Spawns")]
    public List<WeightedCreature> CreaturePool;
    public GameObject MinigamePrefab;
    public GameObject EmptyPrefab;
    public GameObject TreasurePrefab;
    public GameObject TrackPrefab;

    [Header("Level Starting Resources")]
    public int StartingIcePicks = 15;
    public int StartingHammers = 5;
    public int StartingMagnifyingGlasses = 2;

    // To track restricted spawn zones for the buffer
    private HashSet<Vector2Int> RestrictedZones = new HashSet<Vector2Int>();

    public void GenerateLevel(Vector2Int gridSize)
    {
        CreatureTracker.Instance.ClearTracker();

        // Feed the level's specific resource limits to the inventory
        InventoryManager.Instance.InitializeLevelResources(StartingIcePicks, StartingHammers, StartingMagnifyingGlasses);

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
        SpawnTreasures(AvailablePool); 
        SpawnTracks(AvailablePool); 
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

        PrintSpawnSummary(SpawnTally, TotalWeight);
    }

    private Vector2Int[] GetHexOffsets(Vector2Int position)
    {
        bool IsEvenRow = (position.y % 2) == 0;

        if (IsEvenRow)
        {
            return new Vector2Int[] {
                new(0, 1),   new(-1, 1),  new(-1, 0),
                new(-1, -1), new(0, -1),  new(1, 0)
            };
        }
        else
        {
            return new Vector2Int[] {
                new(1, 1),   new(0, 1),   new(-1, 0),
                new(0, -1),  new(1, -1),  new(1, 0)
            };
        }
    }

    private void SpawnMinigames(List<Vector2Int> availablePool)
    {
        int PlacedCount = 0;
        int Attempts = 0;

        while (PlacedCount < MinigameCount && Attempts < 100)
        {
            Attempts++;
            if (availablePool.Count == 0) break;

            int RandomIndex = Random.Range(0, availablePool.Count);
            Vector2Int ChosenPos = availablePool[RandomIndex];

            if (!RestrictedZones.Contains(ChosenPos))
            {
                PlaceTileOnGrid(ChosenPos, MinigamePrefab);
                availablePool.RemoveAt(RandomIndex);

                // REVISED: Only restrict the tile itself, removing the neighbor buffer
                RestrictedZones.Add(ChosenPos);

                PlacedCount++;
            }
        }
    }

    // NEW: Spawns Treasure Chests
    private void SpawnTreasures(List<Vector2Int> availablePool)
    {
        int PlacedCount = 0;
        int Attempts = 0;

        while (PlacedCount < TreasureCount && Attempts < 100)
        {
            Attempts++;
            if (availablePool.Count == 0) break;

            int RandomIndex = Random.Range(0, availablePool.Count);
            Vector2Int ChosenPos = availablePool[RandomIndex];

            if (!RestrictedZones.Contains(ChosenPos))
            {
                PlaceTileOnGrid(ChosenPos, TreasurePrefab);
                availablePool.RemoveAt(RandomIndex);

                // Restrict the tile so nothing else overlaps it
                RestrictedZones.Add(ChosenPos);

                PlacedCount++;
            }
        }
    }

    // NEW: Spawns Track Tiles
    private void SpawnTracks(List<Vector2Int> availablePool)
    {
        int PlacedCount = 0;
        int Attempts = 0;

        while (PlacedCount < TrackCount && Attempts < 100)
        {
            Attempts++;
            if (availablePool.Count == 0) break;

            int RandomIndex = Random.Range(0, availablePool.Count);
            Vector2Int ChosenPos = availablePool[RandomIndex];

            if (!RestrictedZones.Contains(ChosenPos))
            {
                PlaceTileOnGrid(ChosenPos, TrackPrefab);
                availablePool.RemoveAt(RandomIndex);

                RestrictedZones.Add(ChosenPos);

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

        if (prefab.TryGetComponent<TileEntityBase>(out TileEntityBase entityPrefab))
        {
            GridTileAsset Node = ScriptableObject.CreateInstance<GridTileAsset>();
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
            float chance = ((float)creature.Weight / totalWeight) * 100f;
            sb.AppendLine($"- {creature.Shape.ShapeName}: Spawned <b>{count}</b> time(s) (Chance: {chance:F1}%)");
        }

        sb.AppendLine("---------------------------");
        Debug.Log(sb.ToString());
    }
}