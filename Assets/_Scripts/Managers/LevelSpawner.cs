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
        FillRemainingWithEmpties(AvailablePool);
    }

    private WeightedCreature PickRandomCreature(List<WeightedCreature> availableCreatures, int totalWeight)
    {
        int RandomValue = Random.Range(0, totalWeight);
        int CurrentWeight = 0;

        foreach (WeightedCreature item in availableCreatures)
        {
            CurrentWeight += item.Weight;
            if (RandomValue < CurrentWeight)
            {
                return item;
            }
        }

        return availableCreatures[0];
    }

    private void SpawnCreatureShapes(List<Vector2Int> availablePool)
    {
        List<WeightedCreature> tempCreaturePool = new List<WeightedCreature>(CreaturePool);
        Dictionary<WeightedCreature, int> SpawnTally = new Dictionary<WeightedCreature, int>();

        for (int i = 0; i < TotalCreaturesToSpawn; i++)
        {
            if (tempCreaturePool.Count == 0)
            {
                Debug.LogWarning("Ran out of unique creatures in the pool! Stopping creature spawns early.");
                break;
            }

            int currentTotalWeight = 0;
            foreach (WeightedCreature item in tempCreaturePool)
            {
                currentTotalWeight += item.Weight;
            }

            WeightedCreature PickedData = PickRandomCreature(tempCreaturePool, currentTotalWeight);
            CreatureShape ShapeTemplate = PickedData.Shape;

            bool IsPlaced = false;
            List<Vector2Int> ShuffledPool = availablePool.OrderBy(x => Random.value).ToList();

            foreach (Vector2Int AnchorPos in ShuffledPool)
            {
                bool IsEvenRow = (AnchorPos.y % 2) == 0;
                Vector2Int[] OffsetsToUse = IsEvenRow ? ShapeTemplate.EvenRowOffsets : ShapeTemplate.OddRowOffsets;

                bool CanFitPerfectly = true;
                List<Vector2Int> TargetPositions = new List<Vector2Int>();

                // Check if the body fits
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
                    // --- NEW LOGIC: Test the tracks virtually BEFORE committing to the grid ---
                    Dictionary<Vector2Int, int> calculatedTracks;
                    bool tracksSucceeded = TryCalculateTracks(TargetPositions, availablePool, out calculatedTracks);

                    if (tracksSucceeded)
                    {
                        IsPlaced = true;

                        // 1. Commit the Creature Body to the Grid
                        foreach (Vector2Int Pos in TargetPositions)
                        {
                            PlaceTileOnGrid(Pos, ShapeTemplate.CreaturePrefab);
                            availablePool.Remove(Pos);
                            RestrictedZones.Add(Pos);
                        }

                        CreatureTracker.Instance.RegisterNewCreature(ShapeTemplate, TargetPositions);
                        RegisterCreatureAnchorPosition(AnchorPos, ShapeTemplate);

                        // 2. Commit the Tracks to the Grid
                        foreach (var kvp in calculatedTracks)
                        {
                            Vector2Int trackPos = kvp.Key;
                            int trackTier = kvp.Value;

                            PlaceTileOnGrid(trackPos, TrackPrefab);

                            TrackTile.TrackDistances[trackPos] = trackTier;
                            TrackTile.TrackSources[trackPos] = ShapeTemplate;

                            availablePool.Remove(trackPos);
                            RestrictedZones.Add(trackPos);
                        }

                        // 3. Tally and break the loop to move to the next creature
                        if (SpawnTally.ContainsKey(PickedData))
                        {
                            SpawnTally[PickedData]++;
                        }
                        else
                        {
                            SpawnTally[PickedData] = 1;
                        }

                        tempCreaturePool.Remove(PickedData);
                        break;
                    }
                    // If tracksSucceeded is false, the loop simply continues and tries the next AnchorPos!
                }
            }

            if (!IsPlaced)
            {
                Debug.LogWarning($"Grid too crowded! Could not find a valid body AND track combination for {ShapeTemplate.CreatureName}.");
            }
        }

        int originalTotalWeight = 0;
        foreach (WeightedCreature item in CreaturePool) originalTotalWeight += item.Weight;

        PrintSpawnSummary(SpawnTally, originalTotalWeight);
    }

    /// <summary>
    /// Calculates the tracks virtually. Returns true if a path is found, outputting the path dictionary. 
    /// Does NOT place anything on the physical grid.
    /// </summary>
    private bool TryCalculateTracks(List<Vector2Int> creatureOccupiedTiles, List<Vector2Int> availablePool, out Dictionary<Vector2Int, int> proposedTracks)
    {
        proposedTracks = new Dictionary<Vector2Int, int>();

        if (TrackSpawnRules == null || TrackSpawnRules.Count == 0) return true; // No rules = instant success

        // 1. BFS mapping to get absolute true distances from the creature's footprint
        Dictionary<int, List<Vector2Int>> validSpotsByDistance = new Dictionary<int, List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>(creatureOccupiedTiles);
        List<Vector2Int> currentRing = new List<Vector2Int>(creatureOccupiedTiles);

        int currentDistance = 1;
        int maxDistance = TrackSpawnRules.Max(rule => rule.Distance);

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

                    if (!GridManager.Instance.IsInBounds(absoluteNeighbor)) continue;

                    if (!visited.Contains(absoluteNeighbor))
                    {
                        visited.Add(absoluteNeighbor);
                        nextRing.Add(absoluteNeighbor);

                        // Because the creature body hasn't been removed from availablePool yet, it's technically still there.
                        // The `visited` hashset prevents us from putting tracks ON the creature body.
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

        // 2. Flatten track rules into a sequence for DFS Backtracking (The "Snake" Approach)
        var sortedRules = TrackSpawnRules.OrderBy(r => r.Distance).ToList();
        List<(bool requiresSpecificDistance, int targetDistance, int visualTier)> trackSequence = new List<(bool, int, int)>();

        foreach (var rule in sortedRules)
        {
            for (int i = 0; i < rule.Amount; i++)
            {
                bool isFirst = (i == 0);
                trackSequence.Add((isFirst, rule.Distance, rule.Distance));
            }
        }

        List<Vector2Int> finalPath = new List<Vector2Int>();
        Dictionary<Vector2Int, int> finalDistances = new Dictionary<Vector2Int, int>();

        // 3. Local Recursive DFS Function
        bool FindPath(int currentIndex, List<Vector2Int> currentPath, Dictionary<Vector2Int, int> currentDistances)
        {
            if (currentIndex >= trackSequence.Count)
            {
                finalPath = new List<Vector2Int>(currentPath);
                finalDistances = new Dictionary<Vector2Int, int>(currentDistances);
                return true;
            }

            var req = trackSequence[currentIndex];
            bool requiresSpecificDistance = req.requiresSpecificDistance;
            int targetDistance = req.targetDistance;
            int visualTier = req.visualTier;

            List<Vector2Int> candidatePool;

            if (requiresSpecificDistance)
            {
                if (!validSpotsByDistance.ContainsKey(targetDistance)) return false;
                candidatePool = validSpotsByDistance[targetDistance]
                    .Where(p => !currentPath.Contains(p))
                    .OrderBy(x => Random.value)
                    .ToList();
            }
            else
            {
                // Must ensure "Tail" spots don't accidentally fall ON the currently testing creature body
                candidatePool = availablePool
                    .Where(p => !currentPath.Contains(p)
                             && !RestrictedZones.Contains(p)
                             && !creatureOccupiedTiles.Contains(p))
                    .OrderBy(x => Random.value)
                    .ToList();
            }

            foreach (Vector2Int spot in candidatePool)
            {
                bool isValid = true;

                if (currentIndex > 0)
                {
                    Vector2Int prevSpot = currentPath[currentIndex - 1];

                    bool touchesPrev = IsAdjacentToAny(spot, new List<Vector2Int> { prevSpot });
                    if (!touchesPrev) isValid = false;

                    if (isValid && currentIndex >= 2)
                    {
                        List<Vector2Int> olderTracks = currentPath.GetRange(0, currentIndex - 1);
                        if (IsAdjacentToAny(spot, olderTracks))
                        {
                            isValid = false;
                        }
                    }
                }

                if (isValid)
                {
                    currentPath.Add(spot);
                    currentDistances[spot] = visualTier;

                    if (FindPath(currentIndex + 1, currentPath, currentDistances))
                    {
                        return true;
                    }

                    currentPath.RemoveAt(currentPath.Count - 1);
                    currentDistances.Remove(spot);
                }
            }

            return false;
        }

        // 4. Start DFS
        bool success = FindPath(0, new List<Vector2Int>(), new Dictionary<Vector2Int, int>());

        // 5. Output Result
        if (success)
        {
            proposedTracks = finalDistances;
            return true;
        }

        return false;
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

    private bool IsAdjacentToAny(Vector2Int position, List<Vector2Int> targetList)
    {
        if (targetList == null || targetList.Count == 0) return false;

        Vector2Int[] neighbors = GetHexOffsets(position);
        foreach (Vector2Int offset in neighbors)
        {
            if (targetList.Contains(position + offset)) return true;
        }
        return false;
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