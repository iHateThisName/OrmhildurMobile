using Gaskellgames;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour {

    private const string SAVE_KEY = "GAME_SAVE_DATA";

    public static void Save(SaveData data) {
        string json = JsonConvert.SerializeObject(data);

        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static SaveData Load() {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return new SaveData(); // First time playing.

        string json = PlayerPrefs.GetString(SAVE_KEY);

        SaveData saveData = JsonConvert.DeserializeObject<SaveData>(json);

        // Validate the save file
        ValidateSave(saveData);
        return saveData;
    }

    private static void ValidateSave(SaveData saveData) {
        saveData.creatureSaveDataLookup ??= new Dictionary<EnumCreatureName, CreatureSaveData>();
    }
}

/// <summary>
/// This class represents all the data that will be saved and loaded.
/// </summary>
[System.Serializable]
public class SaveData {
    public int score; // Testing
    public Dictionary<EnumCreatureName, CreatureSaveData> creatureSaveDataLookup = new Dictionary<EnumCreatureName, CreatureSaveData>();
}

/// <summary>
/// This class represents the data specific to the creature, which will be saved and loaded.
/// </summary>
[System.Serializable]
public class CreatureSaveData {
    public EnumCreatureName creatureName;
    public int amount;

}
