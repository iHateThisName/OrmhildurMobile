using Assets._Scripts.Utilities.Singleton;
using System; // REQUIRED for Actions
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureTracker : Singleton<CreatureTracker>
{
    public class CreatureInstance
    {
        public CreatureShape Shape;
        public HashSet<Vector2Int> TileCoordinates = new HashSet<Vector2Int>();
        public int FoundParts = 0;

        public bool IsComplete => FoundParts >= TileCoordinates.Count;
    }

    private List<CreatureInstance> ActiveCreatures = new List<CreatureInstance>();

    public static event Action<HashSet<Vector2Int>> OnCreatureDiscovered;

    public static event Action OnAllCreaturesFound;

    public bool AreAllCreaturesComplete => ActiveCreatures.Count > 0 && ActiveCreatures.All(c => c.IsComplete);

    public void RegisterNewCreature(CreatureShape shape, List<Vector2Int> coordinates)
    {
        CreatureInstance newCreature = new CreatureInstance
        {
            Shape = shape,
            TileCoordinates = new HashSet<Vector2Int>(coordinates)
        };
        ActiveCreatures.Add(newCreature);
    }

    public bool ReportTileDug(Vector2Int gridPosition)
    {
        CreatureInstance creature = ActiveCreatures.FirstOrDefault(c => c.TileCoordinates.Contains(gridPosition));

        if (creature != null)
        {
            creature.FoundParts++;
            Debug.Log($"Dug part of {creature.Shape.CreatureName}! {creature.FoundParts}/{creature.TileCoordinates.Count} found.");

            // REVISED: If this specific creature was just finished, check if the whole board is finished
            if (creature.IsComplete)
            {
                OnCreatureDiscovered?.Invoke(creature.TileCoordinates);
                if (AreAllCreaturesComplete)
                {
                    Debug.Log("<color=cyan>[Level]</color> All creatures have been found! Level Complete.");
                    OnAllCreaturesFound?.Invoke();
                    foreach (CreatureInstance creatures in ActiveCreatures)
                    {
                        CreatureSaveData creatureSaveData = GameManager.Instance.GetCreatureSaveData(creatures.Shape.CreatureName);
                        creatureSaveData.amount++;
                        GameManager.Instance.ForceSave();
                    }
                }

                return true;
            }
        }

        return false;
    }

    public void ClearTracker()
    {
        ActiveCreatures.Clear();
    }
}