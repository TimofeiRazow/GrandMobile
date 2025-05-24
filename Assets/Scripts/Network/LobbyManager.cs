using UnityEngine;
using System.Collections.Generic;
using System;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 10;
    [SerializeField] private float autoStartDelay = 5f;
    [SerializeField] private string gameVersion = "1.0";

    private Dictionary<string, PlayerInfo> playersInLobby = new Dictionary<string, PlayerInfo>();
    private bool isHost = false;
    private float autoStartTimer = 0f;

    public class PlayerInfo
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public bool IsReady { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }
    }

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
        PhotonNetwork.AutomaticallySyncScene = true;
        ConnectToPhoton();
    }

    private void Update()
    {
        if (isHost && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
        {
            autoStartTimer += Time.deltaTime;
            if (autoStartTimer >= autoStartDelay)
            {
                StartGame();
            }
        }
    }

    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateLobby()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(null, roomOptions);
        isHost = true;
    }

    public void JoinLobby(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void SetPlayerReady(bool ready)
    {
        if (PhotonNetwork.LocalPlayer != null)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "IsReady", ready }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
    }

    public void StartGame()
    {
        if (isHost && PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerInfo playerInfo = new PlayerInfo
        {
            PlayerId = newPlayer.UserId,
            PlayerName = newPlayer.NickName,
            IsReady = false,
            Role = "Unknown"
        };

        playersInLobby[newPlayer.UserId] = playerInfo;
        // TODO: Update UI to show new player
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playersInLobby.ContainsKey(otherPlayer.UserId))
        {
            playersInLobby.Remove(otherPlayer.UserId);
            // TODO: Update UI to remove player
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (playersInLobby.ContainsKey(targetPlayer.UserId))
        {
            if (changedProps.ContainsKey("IsReady"))
            {
                playersInLobby[targetPlayer.UserId].IsReady = (bool)changedProps["IsReady"];
                // TODO: Update UI to show player ready status
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // TODO: Update available rooms list in UI
    }

    public override void OnJoinedRoom()
    {
        // TODO: Update UI to show lobby screen
    }

    public override void OnLeftRoom()
    {
        playersInLobby.Clear();
        isHost = false;
        autoStartTimer = 0f;
        // TODO: Update UI to show main menu
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
        // TODO: Show reconnection UI
    }
} 