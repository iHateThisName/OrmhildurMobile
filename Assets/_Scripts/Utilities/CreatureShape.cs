using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCreatureShape", menuName = "Grid/Creature Shape")]
public class CreatureShape : ScriptableObject
{
    [Header("Creature Definition")]
    public Sprite CreatureVisualSprite;
    public Vector2 CreatureVisualTransform;


    public EnumCreatureName CreatureName = EnumCreatureName.None;

    public GameObject CreaturePrefab;

    [Header("Track Definition")]
    [Tooltip("The sprite to show when the track is scanned with the magnifying glass")]
    public Sprite TrackHintSprite;

    [Tooltip("The sprite to show when the track is dug up with the ice pick")]
    public Sprite TrackDugSprite;

    [Header("Shape Definition")]
    [Tooltip("Offsets if the Anchor tile is placed on an EVEN Y row (0, 2, 4)")]
    public Vector2Int[] EvenRowOffsets;

    [Tooltip("Offsets if the Anchor tile is placed on an ODD Y row (1, 3, 5)")]
    public Vector2Int[] OddRowOffsets;

}

[System.Serializable]
public enum EnumCreatureName {
    None,
    BirdMan,
    Mermaid1,

    // PlaceHolder
    Snake,
    Troll,
    Dog,
}