using UnityEngine;

public enum TileType { Empty, Creature, Trap }

public class ProtoTile : MonoBehaviour
{
    public int GridX { get; private set; }
    public int GridZ { get; private set; }
    public TileType Type { get; private set; }
    public int CreatureID { get; private set; } // Groups multi-tile creatures
    public bool IsRevealed { get; private set; }

    public void Setup(int x, int z, TileType type, int creatureID = -1)
    {
        GridX = x;
        GridZ = z;
        Type = type;
        CreatureID = creatureID;

        gameObject.name = $"ProtoTile_{x}_{z}_{Type}";
    }

    public void Reveal()
    {
        if (IsRevealed) return;
        IsRevealed = true;

        transform.position += Vector3.down * 0.2f;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            switch (Type)
            {
                case TileType.Empty:
                    rend.material.color = new Color(0.6f, 0.4f, 0.2f);
                    break;
                case TileType.Creature:
                    rend.material.color = Color.green;
                    break;
                case TileType.Trap:
                    rend.material.color = Color.red;
                    break;
            }
        }
    }

    // Turns an empty tile into a "Track" pointing in a specific direction
    public void ShowTrackFeedback(Vector3 directionToCreature)
    {
        // Tints yellow to show it's close
        GetComponent<Renderer>().material.color = Color.yellow;
        Debug.Log($"Tracks found at {GridX},{GridZ} pointing towards {directionToCreature}!");
    }
}