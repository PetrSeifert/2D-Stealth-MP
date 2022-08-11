using UnityEngine;

[System.Serializable]
public class SaveData
{
    [System.Serializable]
    public struct SettingsData
    {
        public bool fullScreen;
        public float musicVolume;
        public float sfxVolume;
    }

    public SettingsData settingsData = new SettingsData();

    public string SaveToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}