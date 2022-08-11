using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour, ISaveable
{
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text selectedResolutionText;
    private AudioSource musicPlayer;
    private int selectedResolution;
    private Resolution[] resolutions;

    private void Awake()
    {
        musicPlayer = MusicPlayer.Instance;
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        SaveManager.LoadJsonData(this);
        Resolution currentResolution = new Resolution { width = Screen.width, height = Screen.height };
        selectedResolution = System.Array.IndexOf(resolutions, currentResolution);
        selectedResolutionText.text = resolutions[selectedResolution].width.ToString() + "x" + resolutions[selectedResolution].height.ToString();
        ApplySettings();
    }

    public void PreviousResolution()
    {
        if (selectedResolution != 0) selectedResolution--;
        selectedResolutionText.text = resolutions[selectedResolution].width.ToString() + "x" + resolutions[selectedResolution].height.ToString();
    }

    public void NextResolution()
    {
        if (selectedResolution != resolutions.Length - 1) selectedResolution++;
        selectedResolutionText.text = resolutions[selectedResolution].width.ToString() + "x" + resolutions[selectedResolution].height.ToString();
    }

    private void SetVolume()
    {
        musicPlayer.volume = musicVolumeSlider.value;
    }

    private void SetResolution()
    {
        Resolution resolution = resolutions[selectedResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, Screen.currentResolution.refreshRate);
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void ApplySettings()
    {
        EventSystem.current.SetSelectedGameObject(null);
        SetVolume();
        SetResolution();
        SaveManager.SaveJsonData(this);
    }

    public void PopulateSaveData(SaveData saveData)
    {
        saveData.settingsData.fullScreen = fullscreenToggle.isOn;
        saveData.settingsData.musicVolume = musicVolumeSlider.value;
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        SaveData.SettingsData settingsData = saveData.settingsData;
        fullscreenToggle.isOn = settingsData.fullScreen;
        musicVolumeSlider.value = settingsData.musicVolume;
    }
}
