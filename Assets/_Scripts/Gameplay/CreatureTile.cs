using UnityEngine;
using UnityEngine.VFX;

public class CreatureTile : TileEntityBase
{
    [Header("Audio (SFX)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip digCreaturePartClip;
    [SerializeField] private AudioClip creatureCompleteClip;
    public override void Initialize(Vector2Int gridPosition, Color? visualColor = null)
    {
        base.Initialize(gridPosition);
    }
    public override void OnTileClicked()
    {
        VisualRenderer.color = Color.green;
    }
}
