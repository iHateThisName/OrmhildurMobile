using Assets._Scripts.Utilities.Singleton;
using Gaskellgames;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : RegulatorSingelton<GameManager> {
    [field: SerializeField, ReadOnly] public SaveData SaveData { get; private set; }
    public EnumBiomes CurrentBiomeSelected { get; set; } = EnumBiomes.Cliffs;

    [SerializeField] private bool DisplaySafeAreaGizmos = true;
    [SerializeField] private bool DisplayMaxAreaGizmos = true;


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

    [ContextMenu("Increase Test Score")]
    public void IncreaseTestScore() {
        this.SaveData.score++;
        Debug.Log($"Score increased to: {this.SaveData.score}");
    }

    public CreatureSaveData GetCreatureSaveData(EnumCreatureName creatureName) {
        if (!this.SaveData.creatureSaveDataLookup.TryGetValue(creatureName, out CreatureSaveData creatureSaveData)) {
            creatureSaveData = new CreatureSaveData { creatureName = creatureName, amount = 0 };
            this.SaveData.creatureSaveDataLookup[creatureName] = creatureSaveData;
        }
        return creatureSaveData;
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
