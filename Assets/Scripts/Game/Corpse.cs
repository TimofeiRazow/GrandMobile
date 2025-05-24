using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Game.Interfaces;

public class Corpse : MonoBehaviourPunCallbacks, IInteractable, IInvestigateable
{
    [Header("Corpse Settings")]
    [SerializeField] private string victimId;
    [SerializeField] private string victimName;
    [SerializeField] private bool isReported = false;
    [SerializeField] private bool isBeingDragged = false;

    private List<string> clues = new List<string>();
    private PhotonView photonView;

    public string VictimId => victimId;
    public string VictimName => victimName;
    public bool IsReported => isReported;
    public bool IsBeingDragged => isBeingDragged;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void Initialize(string id, string name)
    {
        victimId = id;
        victimName = name;
        GenerateClues();
    }

    private void GenerateClues()
    {
        // Generate random clues about the victim's death
        clues.Clear();
        
        // Add basic information
        clues.Add($"Victim: {victimName}");
        clues.Add($"Time of death: {System.DateTime.Now.ToString("HH:mm")}");
        
        // Add random clues about the killer
        string[] possibleClues = new string[]
        {
            "Found traces of struggle",
            "No signs of forced entry",
            "Victim was caught by surprise",
            "Evidence of a weapon",
            "Multiple wounds found",
            "Clean kill, professional work",
            "Evidence of a struggle",
            "Victim was restrained",
            "Signs of a chase",
            "Evidence of planning"
        };

        // Add 2-3 random clues
        int clueCount = Random.Range(2, 4);
        for (int i = 0; i < clueCount; i++)
        {
            string clue = possibleClues[Random.Range(0, possibleClues.Length)];
            if (!clues.Contains(clue))
            {
                clues.Add(clue);
            }
        }
    }

    public List<string> GetClues()
    {
        return new List<string>(clues);
    }

    public void OnStartDragging()
    {
        isBeingDragged = true;
        // Add any visual effects or animations for dragging
    }

    public void OnStopDragging()
    {
        isBeingDragged = false;
        // Reset any visual effects or animations
    }

    [PunRPC]
    public void Report()
    {
        isReported = true;
        // Add any visual effects or UI updates for reported corpses
    }

    public void Interact(PlayerController player)
    {
        // This will be called when a player interacts with the corpse
        if (!isReported)
        {
            photonView.RPC("Report", RpcTarget.All);
        }
    }

    public void Investigate(PlayerController player)
    {
        // This will be called when a detective investigates the corpse
        if (player.GetComponent<DetectiveBehaviour>() != null)
        {
            // The DetectiveBehaviour will handle the investigation logic
        }
    }
} 