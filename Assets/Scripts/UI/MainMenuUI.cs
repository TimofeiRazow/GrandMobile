using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Menu Elements")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TextMeshProUGUI gameTitleText;

    private void Start()
    {
        InitializeButtons();
        SetupAudio();
    }

    private void InitializeButtons()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void SetupAudio()
    {
        // TODO: Initialize background music and ambient sounds
    }

    private void OnPlayClicked()
    {
        // TODO: Implement quick play functionality
        Debug.Log("Play button clicked");
    }

    private void OnCreateLobbyClicked()
    {
        // TODO: Implement lobby creation
        Debug.Log("Create Lobby button clicked");
    }

    private void OnJoinClicked()
    {
        // TODO: Implement join lobby functionality
        Debug.Log("Join button clicked");
    }

    private void OnSettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void ReturnToMainMenu()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
} 