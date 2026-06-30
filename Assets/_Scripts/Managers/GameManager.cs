using Assets._Scripts.Utilities.Singleton;
using Gaskellgames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : RegulatorSingelton<GameManager> {
    [field: SerializeField, ReadOnly] public SaveData SaveData { get; private set; }
    [field: SerializeField] public EnumBiomes CurrentBiomeSelected { get; set; } = EnumBiomes.Cliffs;

    [SerializeField] private bool DisplaySafeAreaGizmos = true;
    [SerializeField] private bool DisplayMaxAreaGizmos = true;

    public static readonly List<EnumCreatureName> CliffCreatures = new List<EnumCreatureName> { EnumCreatureName.BirdMan };
    public static readonly List<EnumCreatureName> SeaCreatures = new List<EnumCreatureName> { EnumCreatureName.Mermaid1 };

    protected override void Awake() {
        base.Awake();
        this.SaveData = SaveSystem.Load();
    }

    private void OnApplicationQuit() {
        SaveSystem.Save(this.SaveData);
        Debug.Log("GameManager: Application quitting, save data.");
    }

    private void OnApplicationPause(bool pause) {
        if (pause) {
            SaveSystem.Save(this.SaveData);
            Debug.Log("GameManager: Application paused, save data.");
        }
    }

    public void ForceSave() {
        SaveSystem.Save(this.SaveData);
        Debug.Log("GameManager: Force save data.");
        Debug.Log(this.SaveData.ToString());
    }

    [Button]
    public void IncreaseBirdAmount() {
        CreatureSaveData birdData = GetCreatureSaveData(EnumCreatureName.BirdMan);
        birdData.amount++;
        Debug.Log($"Increased BirdMan amount to: {birdData.amount}");
    }

    private void OnDrawGizmos() {
        if (!(this.DisplaySafeAreaGizmos || this.DisplayMaxAreaGizmos)) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic) return;

        // Height visible in world units
        float worldHeight = mainCamera.orthographicSize * 2f;

        // Center of the camera in world space
        Vector3 center = mainCamera.transform.position;
        center.z = 0f;

        // Draw 16:9 safe area
        if (this.DisplaySafeAreaGizmos)
            DrawAspectRect(center, worldHeight, 16f / 9f, new Color(0f, 1f, 0f, 0.15f), Color.green);

        // Draw 20:9 max area
        if (this.DisplayMaxAreaGizmos)
            DrawAspectRect(center, worldHeight, 20f / 9f, new Color(1f, 0f, 0f, 0.15f), Color.yellow);
    }

    private void DrawAspectRect(Vector3 center, float worldHeight, float aspect, Color fill, Color border) {
        float width = worldHeight * aspect;

        Vector3 size = new Vector3(width, worldHeight, 0);

        Gizmos.color = fill;
        Gizmos.DrawCube(center, size);

        Gizmos.color = border;
        Gizmos.DrawWireCube(center, size);
    }
    public CreatureSaveData GetCreatureSaveData(EnumCreatureName creatureName) {
        if (!this.SaveData.creatureSaveDataLookup.TryGetValue(creatureName, out CreatureSaveData creatureSaveData)) {
            creatureSaveData = new CreatureSaveData { creatureName = creatureName, amount = 0 };
            this.SaveData.creatureSaveDataLookup[creatureName] = creatureSaveData;
        }
        return creatureSaveData;
    }

    public static EnumBiomes NextBiome(EnumBiomes biome, bool setValue = false) {
        List<EnumBiomes> biomes = Enum.GetValues(typeof(EnumBiomes)).Cast<EnumBiomes>().Where(b => b != EnumBiomes.None).ToList();
        EnumBiomes nextBiome;

        foreach (var item in biomes) {
            Debug.Log($"{biomes.IndexOf(item)}, {item}");
        }

        if (biome == EnumBiomes.None) nextBiome = biomes.First();
        else if (biome == biomes.Last()) nextBiome = biomes.First();
        else nextBiome = biomes[biomes.IndexOf(biome) + 1];

        if (setValue) GameManager.Instance.CurrentBiomeSelected = nextBiome;
        return nextBiome;
    }

    public static EnumBiomes PreviousBiome(EnumBiomes biome, bool setValue = false) {
        List<EnumBiomes> biomes = Enum.GetValues(typeof(EnumBiomes)).Cast<EnumBiomes>().Where(b => b != EnumBiomes.None).ToList();
        EnumBiomes previousBiome;

        if (biome == EnumBiomes.None) previousBiome = biomes.Last();
        else if (biome == biomes.First()) previousBiome = biomes.Last();
        else previousBiome = biomes[biomes.IndexOf(biome) - 1];

        if (setValue) GameManager.Instance.CurrentBiomeSelected = previousBiome;
        return previousBiome;
    }

    public static EnumBiomes GetCreatureBiome(EnumCreatureName creatureName) {

        if (CliffCreatures.Contains(creatureName)) return EnumBiomes.Cliffs;
        else if (SeaCreatures.Contains(creatureName)) return EnumBiomes.Sea;
        else return EnumBiomes.None;
    }

    [ContextMenu("Increase Test Score")]
    public void IncreaseTestScore() {
        this.SaveData.score++;
        Debug.Log($"Score increased to: {this.SaveData.score}");
    }


    [Button, ContextMenu("Debug Creature Data")]
    public void DebugCreatureData() {
        System.Collections.Generic.List<CreatureSaveData> creatureSaveDatas = this.SaveData.creatureSaveDataLookup.Values.ToList();

        if (creatureSaveDatas.Count == 0) Debug.Log("No creature data found.");

        creatureSaveDatas.ForEach(data => {
            Debug.Log($"Creature: {data.creatureName}, Amount: {data.amount}");
        });
    }
}
