using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets._Scripts.Utilities.Singleton;

public class CreatureTracker : Singleton<CreatureTracker>
{
    // A simple data class to hold the state of a single spawned creature
    public class CreatureInstance
    {
        public CreatureShape Shape;
        public HashSet<Vector2Int> TileCoordinates = new HashSet<Vector2Int>();
        public int FoundParts = 0;

        public bool IsComplete => FoundParts >= TileCoordinates.Count;
    }

    private List<CreatureInstance> ActiveCreatures = new List<CreatureInstance>();

    public void RegisterNewCreature(CreatureShape shape, List<Vector2Int> coordinates)
    {
        CreatureInstance newCreature = new CreatureInstance
        {
            Shape = shape,
            TileCoordinates = new HashSet<Vector2Int>(coordinates)
        };
        ActiveCreatures.Add(newCreature);
    }

    // Returns TRUE if this dig completed the entire creature
    public bool ReportTileDug(Vector2Int gridPosition)
    {
        // Find which creature owns this specific grid coordinate
        CreatureInstance creature = ActiveCreatures.FirstOrDefault(c => c.TileCoordinates.Contains(gridPosition));

        if (creature != null)
        {
            creature.FoundParts++;
            Debug.Log($"Dug part of {creature.Shape.ShapeName}! {creature.FoundParts}/{creature.TileCoordinates.Count} found.");

            return creature.IsComplete;
        }

        return false;
    }

    // Call this from LevelSpawner right before you generate a new level
    public void ClearTracker()
    {
        ActiveCreatures.Clear();
    }
}