using Assets._Scripts.Utilities.Singleton;
using Gaskellgames;
using System;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class GameManager : RegulatorSingelton<GameManager> {
    [field:SerializeField, ReadOnly] public SaveData SaveData { get; private set; }

    protected override void Awake() {
        base.Awake();
        this.SaveData = SaveSystem.Load();
    }

    private void OnApplicationQuit() {
        SaveSystem.Save(this.SaveData);
    }

    private void OnApplicationPause(bool pause) {
        if (pause) {
            SaveSystem.Save(this.SaveData);
        }
    }

    [ContextMenu("Increase Test Score")]
    public void IncreaseTestScore() {
        this.SaveData.score++;
        Debug.Log($"Score increased to: {this.SaveData.score}");
    }
}
