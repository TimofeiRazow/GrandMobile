using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Photon.Pun;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Transform playerListContent;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI lobbyTimerText;

    [Header("In-Game UI")]
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private TextMeshProUGUI phaseTimerText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private GameObject rolePanel;
    [SerializeField] private TextMeshProUGUI roleText;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;

    [Header("Voting")]
    [SerializeField] private GameObject votingPanel;
    [SerializeField] private Transform votingListContent;
    [SerializeField] private GameObject votingItemPrefab;
    [SerializeField] private Button skipVoteButton;
    [SerializeField] private TextMeshProUGUI votingTimerText;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Transform resultsListContent;
    [SerializeField] private GameObject resultItemPrefab;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private Dictionary<string, GameObject> playerListItems = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> votingItems = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeUI();
        ShowMainMenu();
    }

    private void InitializeUI()
    {
        // Initialize button listeners
        playButton.onClick.AddListener(OnPlayClicked);
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);
        skipVoteButton.onClick.AddListener(OnSkipVoteClicked);
        playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        // Hide all panels initially
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        votingPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    public void ShowLobby()
    {
        HideAllPanels();
        lobbyPanel.SetActive(true);
        UpdateLobbyUI();
    }

    public void ShowGameUI()
    {
        HideAllPanels();
        gameUIPanel.SetActive(true);
        UpdateGameUI();
    }

    public void ShowVotingUI()
    {
        HideAllPanels();
        votingPanel.SetActive(true);
        UpdateVotingUI();
    }

    public void ShowGameOver(bool maniacsWin)
    {
        HideAllPanels();
        gameOverPanel.SetActive(true);
        winnerText.text = maniacsWin ? "Maniacs Win!" : "Civilians Win!";
        UpdateGameOverUI();
    }

    public void UpdateLobbyUI()
    {
        // Clear existing player list
        foreach (var item in playerListItems.Values)
        {
            Destroy(item);
        }
        playerListItems.Clear();

        // Add current players
        foreach (var player in PhotonNetwork.PlayerList)
        {
            AddPlayerToList(player);
        }
    }

    private void AddPlayerToList(Photon.Realtime.Player player)
    {
        GameObject item = Instantiate(playerListItemPrefab, playerListContent);
        TextMeshProUGUI nameText = item.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = item.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();

        nameText.text = player.NickName;
        statusText.text = "Waiting";
        if (player.CustomProperties.ContainsKey("IsReady"))
        {
            statusText.text = (bool)player.CustomProperties["IsReady"] ? "Ready" : "Waiting";
        }

        playerListItems[player.UserId] = item;
    }

    public void UpdateGameUI()
    {
        GameManager.GamePhase currentPhase = GameManager.Instance.GetCurrentPhase();
        float timeRemaining = GameManager.Instance.GetPhaseTimeRemaining();

        phaseText.text = currentPhase.ToString();
        phaseTimerText.text = FormatTime(timeRemaining);

        // Update role panel visibility based on phase
        rolePanel.SetActive(currentPhase == GameManager.GamePhase.Night);
    }

    public void UpdateVotingUI()
    {
        // Clear existing voting list
        foreach (var item in votingItems.Values)
        {
            Destroy(item);
        }
        votingItems.Clear();

        // Add alive players to voting list
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (GameManager.Instance.IsPlayerAlive(player.UserId))
            {
                AddPlayerToVotingList(player);
            }
        }
    }

    private void AddPlayerToVotingList(Photon.Realtime.Player player)
    {
        GameObject item = Instantiate(votingItemPrefab, votingListContent);
        TextMeshProUGUI nameText = item.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        Button voteButton = item.transform.Find("VoteButton").GetComponent<Button>();

        nameText.text = player.NickName;
        voteButton.onClick.AddListener(() => OnVoteClicked(player.UserId));

        votingItems[player.UserId] = item;
    }

    public void ShowInteractionPrompt(string text)
    {
        interactionPrompt.SetActive(true);
        interactionText.text = text;
    }

    public void HideInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    public void ShowNotification(string text, float duration = 3f)
    {
        notificationPanel.SetActive(true);
        notificationText.text = text;
        Invoke(nameof(HideNotification), duration);
    }

    private void HideNotification()
    {
        notificationPanel.SetActive(false);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Button click handlers
    private void OnPlayClicked()
    {
        // TODO: Implement quick play
    }

    private void OnCreateLobbyClicked()
    {
        LobbyManager.Instance.CreateLobby();
    }

    private void OnJoinClicked()
    {
        // TODO: Show room list
    }

    private void OnSettingsClicked()
    {
        // TODO: Show settings panel
    }

    private void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnReadyClicked()
    {
        bool isReady = readyButton.GetComponentInChildren<TextMeshProUGUI>().text == "Ready";
        LobbyManager.Instance.SetPlayerReady(!isReady);
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Not Ready" : "Ready";
    }

    private void OnStartGameClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            LobbyManager.Instance.StartGame();
        }
    }

    private void OnVoteClicked(string targetId)
    {
        // TODO: Implement voting
    }

    private void OnSkipVoteClicked()
    {
        // TODO: Implement skip vote
    }

    private void OnPlayAgainClicked()
    {
        // TODO: Implement play again
    }

    private void OnMainMenuClicked()
    {
        PhotonNetwork.LeaveRoom();
        ShowMainMenu();
    }

    private void UpdateGameOverUI()
    {
        // Clear existing results list
        foreach (Transform child in resultsListContent)
        {
            Destroy(child.gameObject);
        }

        // Add results for each player
        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject resultItem = Instantiate(resultItemPrefab, resultsListContent);
            TextMeshProUGUI nameText = resultItem.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI roleText = resultItem.transform.Find("RoleText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = resultItem.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();

            nameText.text = player.NickName;
            roleText.text = GameManager.Instance.GetPlayerRole(player.UserId).ToString();
            statusText.text = GameManager.Instance.IsPlayerAlive(player.UserId) ? "Survived" : "Died";
        }
    }
} 