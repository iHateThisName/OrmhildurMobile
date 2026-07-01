using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureShape", menuName = "Grid/Creature Shape")]
public class CreatureShape : ScriptableObject
{
    [Header("Creature Definition")]
    public Sprite CreatureVisualSprite;
    public Vector2 CreatureVisualTransform;
    public float CreatureVisualRotation;
    public Vector2 CreatureVisualScale = new Vector2(1f, 1f);

    [Tooltip("The sprite to display when the entire creature has been dug up (e.g., eyes open).")]
    public Sprite CreatureDiscoveredSprite;

    public EnumCreatureName CreatureName = EnumCreatureName.None;

    public GameObject CreaturePrefab;

    [Header("Track Definition")]
    [Tooltip("The sprite to show when the track is scanned with the magnifying glass")]
    public Sprite TrackHintSprite;

    [Tooltip("The sprite to show when the track is dug up with the ice pick")]
    public Sprite TrackDugSprite;

    [Header("Tiered Track Visuals (Dug/Revealed)")]
    [Tooltip("1 hex away = Sprite with 3 clues")]
    public Sprite TrackDistance1Sprite;

    [Tooltip("2 hexes away = Sprite with 2 clues")]
    public Sprite TrackDistance2Sprite;

    [Tooltip("3 hexes away = Sprite with 1 clue")]
    public Sprite TrackDistance3Sprite;

    [Header("Shape Definition")]
    [Tooltip("Offsets if the Anchor tile is placed on an EVEN Y row (0, 2, 4)")]
    public Vector2Int[] EvenRowOffsets;

    [Tooltip("Offsets if the Anchor tile is placed on an ODD Y row (1, 3, 5)")]
    public Vector2Int[] OddRowOffsets;

    /// <summary>
    /// Fetches the correct track sprite based on hex distance.
    /// </summary>
    public Sprite GetTrackSpriteForDistance(int hexDistance, Sprite fallbackSprite)
    {
        switch (hexDistance)
        {
            case 1: return TrackDistance1Sprite != null ? TrackDistance1Sprite : fallbackSprite;
            case 2: return TrackDistance2Sprite != null ? TrackDistance2Sprite : fallbackSprite;
            case 3: return TrackDistance3Sprite != null ? TrackDistance3Sprite : fallbackSprite;
            default: return fallbackSprite; // Fallback for distances > 3 or 0
        }
    }

}

[System.Serializable]
public enum EnumCreatureName {
    None,
    BirdMan,

    //SEA
    BabyNenni,
    Mermaid1, 
    Mermaid2,
    SeaBeast,
    Kraki,

    //FOREST
    BearKing,
    Finngalkn,
    NaddiGreen,
    NaddiGray,
    Raven,

    // PlaceHolder
    Snake,
    Troll,
    Dog,
}