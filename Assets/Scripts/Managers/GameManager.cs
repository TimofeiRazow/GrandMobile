using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private float dayDuration = 300f; // 5 minutes
    [SerializeField] private float nightDuration = 180f; // 3 minutes
    [SerializeField] private float votingDuration = 60f; // 1 minute
    [SerializeField] private int minPlayersToStart = 4;

    [Header("Role Settings")]
    [SerializeField] private int numberOfManiacs = 1;
    [SerializeField] private int numberOfDetectives = 1;

    public enum GamePhase
    {
        Waiting,
        Day,
        Night,
        Voting,
        GameOver
    }

    public enum GameRole
    {
        Civilian,
        Maniac,
        Detective
    }

    private GamePhase currentPhase = GamePhase.Waiting;
    private float phaseTimer = 0f;
    private Dictionary<string, GameRole> playerRoles = new Dictionary<string, GameRole>();
    private Dictionary<string, bool> playerAlive = new Dictionary<string, bool>();
    private List<string> deadPlayers = new List<string>();

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
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeGame();
        }
    }

    private void Update()
    {
        if (currentPhase != GamePhase.Waiting && currentPhase != GamePhase.GameOver)
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                AdvancePhase();
            }
        }
    }

    private void InitializeGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayersToStart)
        {
            Debug.LogWarning("Not enough players to start the game");
            return;
        }

        AssignRoles();
        StartDayPhase();
    }

    private void AssignRoles()
    {
        List<string> playerIds = new List<string>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerIds.Add(player.UserId);
            playerAlive[player.UserId] = true;
        }

        // Shuffle player list
        System.Random rng = new System.Random();
        int n = playerIds.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string temp = playerIds[k];
            playerIds[k] = playerIds[n];
            playerIds[n] = temp;
        }

        // Assign roles
        for (int i = 0; i < playerIds.Count; i++)
        {
            if (i < numberOfManiacs)
            {
                playerRoles[playerIds[i]] = GameRole.Maniac;
            }
            else if (i < numberOfManiacs + numberOfDetectives)
            {
                playerRoles[playerIds[i]] = GameRole.Detective;
            }
            else
            {
                playerRoles[playerIds[i]] = GameRole.Civilian;
            }
        }

        // Notify players of their roles
        foreach (var player in PhotonNetwork.PlayerList)
        {
            photonView.RPC("AssignRole", player, playerRoles[player.UserId]);
        }
    }

    [PunRPC]
    private void AssignRole(GameRole role)
    {
        playerRoles[PhotonNetwork.LocalPlayer.UserId] = role;
        // TODO: Update UI to show role
    }

    private void StartDayPhase()
    {
        currentPhase = GamePhase.Day;
        phaseTimer = dayDuration;
        // TODO: Update UI for day phase
    }

    private void StartNightPhase()
    {
        currentPhase = GamePhase.Night;
        phaseTimer = nightDuration;
        // TODO: Update UI for night phase
    }

    private void StartVotingPhase()
    {
        currentPhase = GamePhase.Voting;
        phaseTimer = votingDuration;
        // TODO: Update UI for voting phase
    }

    private void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Day:
                StartNightPhase();
                break;
            case GamePhase.Night:
                StartVotingPhase();
                break;
            case GamePhase.Voting:
                ProcessVotingResults();
                break;
        }
    }

    private void ProcessVotingResults()
    {
        // TODO: Implement voting results processing
        CheckGameEnd();
        if (currentPhase != GamePhase.GameOver)
        {
            StartDayPhase();
        }
    }

    private void CheckGameEnd()
    {
        int aliveManiacs = 0;
        int aliveCivilians = 0;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (playerAlive[player.UserId])
            {
                if (playerRoles[player.UserId] == GameRole.Maniac)
                {
                    aliveManiacs++;
                }
                else
                {
                    aliveCivilians++;
                }
            }
        }

        if (aliveManiacs == 0)
        {
            EndGame(false); // Civilians win
        }
        else if (aliveManiacs >= aliveCivilians)
        {
            EndGame(true); // Maniacs win
        }
    }

    private void EndGame(bool maniacsWin)
    {
        currentPhase = GamePhase.GameOver;
        // TODO: Show end game UI with results
    }

    [PunRPC]
    public void ReportBody(string victimId)
    {
        if (playerAlive.ContainsKey(victimId) && playerAlive[victimId])
        {
            playerAlive[victimId] = false;
            deadPlayers.Add(victimId);
            StartVotingPhase();
        }
    }

    public bool IsPlayerAlive(string playerId)
    {
        return playerAlive.ContainsKey(playerId) && playerAlive[playerId];
    }

    public GameRole GetPlayerRole(string playerId)
    {
        return playerRoles.ContainsKey(playerId) ? playerRoles[playerId] : GameRole.Civilian;
    }

    public GamePhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public float GetPhaseTimeRemaining()
    {
        return phaseTimer;
    }
} 