using UnityEngine;

public static class SaveManager
{
    public static void SaveJsonData(ISaveable saveable)
    {
        SaveData sd = new SaveData();

        saveable.PopulateSaveData(sd);

        if (FileManager.WriteToFile("SettingsData.dat", sd.SaveToJson()))
        {
            Debug.Log("Save successful");
        }
    }

    public static void LoadJsonData(ISaveable saveable)
    {
        if (FileManager.ReadFromFile("SettingsData.dat", out var json))
        {
            SaveData sd = new SaveData();

            sd.LoadFromJson(json);

            saveable.LoadFromSaveData(sd);

            Debug.Log("Load complete");
        }
    }
}
