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
    //public int TracksPerCreature = 3;

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

    [Header("Track Settings")]
    [Tooltip("Define how many tracks spawn at specific distances from the creature.")]
    public List<TrackSpawnRule> TrackSpawnRules;

    // To track restricted spawn zones for the buffer
    private HashSet<Vector2Int> RestrictedZones = new HashSet<Vector2Int>();

    public void GenerateLevel(Vector2Int gridSize)
    {
        CreatureTracker.Instance.ClearTracker();
        TrackTile.TrackDistances.Clear();
        TrackTile.TrackSources.Clear();
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
        // Removed SpawnTracks() from here
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
        int TotalWeight = 0;
        foreach (WeightedCreature item in CreaturePool)
        {
            TotalWeight += item.Weight;
        }

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

                    // 1. Lock down the exact creature tiles first
                    foreach (Vector2Int Pos in TargetPositions)
                    {
                        PlaceTileOnGrid(Pos, ShapeTemplate.CreaturePrefab);
                        availablePool.Remove(Pos);
                        RestrictedZones.Add(Pos);
                    }

                    CreatureTracker.Instance.RegisterNewCreature(ShapeTemplate, TargetPositions);
                    RegisterCreatureAnchorPosition(AnchorPos, ShapeTemplate);

                    // 2. Immediately spawn tracks based on this creature's footprint
                    SpawnTracksForCreature(TargetPositions, availablePool, ShapeTemplate);

                    // 3. NOW apply the 1-hex buffer restricted zone around the creature
                    // Tracks placed above bypass this because they already snagged their spots.
                    foreach (Vector2Int Pos in TargetPositions)
                    {
                        Vector2Int[] NeighborOffsets = GetHexOffsets(Pos);
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
                Debug.LogWarning($"Grid too crowded! Could not fit a {ShapeTemplate.CreatureName}.");
            }
        }

        PrintSpawnSummary(SpawnTally, TotalWeight);
    }

    private void SpawnTracksForCreature(List<Vector2Int> creatureOccupiedTiles, List<Vector2Int> availablePool, CreatureShape creatureSource)
    {
        // Safety check in case the list is empty in the inspector
        if (TrackSpawnRules == null || TrackSpawnRules.Count == 0) return;

        Dictionary<int, List<Vector2Int>> validSpotsByDistance = new Dictionary<int, List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(creatureOccupiedTiles);
        List<Vector2Int> currentRing = new List<Vector2Int>(creatureOccupiedTiles);

        int currentDistance = 1;
        // Dynamically find the furthest distance we need to calculate based on the inspector list
        int maxDistance = TrackSpawnRules.Max(rule => rule.Distance);

        // 1. Map out the hex radius using Breadth-First Search
        while (currentDistance <= maxDistance && currentRing.Count > 0)
        {
            List<Vector2Int> nextRing = new List<Vector2Int>();
            validSpotsByDistance[currentDistance] = new List<Vector2Int>();

            foreach (Vector2Int pos in currentRing)
            {
                Vector2Int[] offsets = GetHexOffsets(pos);
                foreach (Vector2Int offset in offsets)
                {
                    Vector2Int absoluteNeighbor = pos + offset;

                    if (!visited.Contains(absoluteNeighbor))
                    {
                        visited.Add(absoluteNeighbor);
                        nextRing.Add(absoluteNeighbor);

                        if (availablePool.Contains(absoluteNeighbor) && !RestrictedZones.Contains(absoluteNeighbor))
                        {
                            validSpotsByDistance[currentDistance].Add(absoluteNeighbor);
                        }
                    }
                }
            }

            currentRing = nextRing;
            currentDistance++;
        }

        foreach (TrackSpawnRule rule in TrackSpawnRules)
        {
            int distance = rule.Distance;
            int amountToSpawn = rule.Amount;

            if (validSpotsByDistance.ContainsKey(distance))
            {
                List<Vector2Int> validSpots = validSpotsByDistance[distance].OrderBy(x => Random.value).ToList();
                int spawnedCount = 0;

                for (int i = 0; i < validSpots.Count && spawnedCount < amountToSpawn; i++)
                {
                    Vector2Int spawnPos = validSpots[i];

                    PlaceTileOnGrid(spawnPos, TrackPrefab);

                    TrackTile.TrackDistances[spawnPos] = distance;

                    // --- NEW LINE --- Register which creature made this track
                    TrackTile.TrackSources[spawnPos] = creatureSource;

                    availablePool.Remove(spawnPos);
                    RestrictedZones.Add(spawnPos);

                    spawnedCount++;
                }

                if (spawnedCount < amountToSpawn)
                {
                    Debug.LogWarning($"Grid too cramped: Only spawned {spawnedCount}/{amountToSpawn} tracks at distance {distance}.");
                }
            }
        }
    }

    private void RegisterCreatureAnchorPosition(Vector2Int anchorPos, CreatureShape creatureInformation)
    {
        CreatureTile creatureTile = GridGameManager.Instance.GetCreatureTileByGridPosition(anchorPos);
        if (creatureTile != null)
        {
            creatureTile.SetAsCreatureAnchorTile(creatureInformation);
        }
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
                RestrictedZones.Add(ChosenPos);
                PlacedCount++;
            }
        }
    }

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

    [System.Serializable]
    public struct TrackSpawnRule
    {
        [Tooltip("Distance from the creature in hexes (e.g., 1, 2, or 3)")]
        public int Distance;

        [Tooltip("How many tracks to spawn at this distance")]
        public int Amount;
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
            sb.AppendLine($"- {creature.Shape.CreatureName}: Spawned <b>{count}</b> time(s) (Chance: {chance:F1}%)");
        }

        sb.AppendLine("---------------------------");
        Debug.Log(sb.ToString());
    }
}