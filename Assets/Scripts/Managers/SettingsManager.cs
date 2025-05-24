using UnityEngine;
using UnityEngine.Audio;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("Game Settings")]
    [SerializeField] private float defaultMouseSensitivity = 1f;
    [SerializeField] private bool defaultShowTutorial = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMasterVolume(float volume)
    {
        SetVolume(masterVolumeParam, volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume(musicVolumeParam, volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume(sfxVolumeParam, volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void SetVolume(string paramName, float volume)
    {
        float dbVolume = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat(paramName, dbVolume);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
    }

    public void SetShowTutorial(bool show)
    {
        PlayerPrefs.SetInt("ShowTutorial", show ? 1 : 0);
    }

    public void SetLanguage(string languageCode)
    {
        PlayerPrefs.SetString("Language", languageCode);
        // TODO: Implement language change logic
    }

    private void LoadSettings()
    {
        // Load audio settings
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));

        // Load game settings
        SetMouseSensitivity(PlayerPrefs.GetFloat("MouseSensitivity", defaultMouseSensitivity));
        SetShowTutorial(PlayerPrefs.GetInt("ShowTutorial", defaultShowTutorial ? 1 : 0) == 1);
        SetLanguage(PlayerPrefs.GetString("Language", "en"));
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
    }
} 