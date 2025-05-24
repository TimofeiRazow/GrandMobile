using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

// Dummy Evidence class definition (replace with actual implementation if it exists elsewhere)
public class Evidence : MonoBehaviour
{
    public string SuspectId;
    public string Clue;
    public string Description;
}

public class DetectiveBehaviour : BaseRoleBehaviour
{
    [Header("Detective Settings")]
    [SerializeField] private float investigationRange = 2f;
    [SerializeField] private LayerMask evidenceLayers;
    [SerializeField] private int maxClues = 3;

    private List<Evidence> collectedEvidence = new List<Evidence>();
    private Dictionary<string, List<string>> suspectClues = new Dictionary<string, List<string>>();

    public override void Initialize()
    {
        UIManager.Instance.ShowNotification("You are the Detective!", 5f);
    }

    public override void OnDayPhaseStart()
    {
        // Reset investigation UI
    }

    public override void OnNightPhaseStart()
    {
        UIManager.Instance.ShowNotification("Night Phase - You can investigate!", 3f);
    }

    public override void OnVotingPhaseStart()
    {
        // Show collected evidence during voting
        DisplayCollectedEvidence();
    }

    public override void OnGameOver(bool maniacsWin)
    {
        // Show final investigation results
        DisplayFinalResults();
    }

    public override void HandleInteraction(IInteractable interactable)
    {
        if (interactable is Corpse corpse)
        {
            InvestigateCorpse(corpse);
        }
    }

    public override void HandleInvestigation(IInvestigateable investigateable)
    {
        if (investigateable is Evidence evidence)
        {
            CollectEvidence(evidence);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInvestigate();
        }
    }

    private void TryInvestigate()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerController.transform.position, playerController.transform.forward, out hit, investigationRange, evidenceLayers))
        {
            IInvestigateable investigateable = hit.collider.GetComponent<IInvestigateable>();
            if (investigateable != null)
            {
                HandleInvestigation(investigateable);
            }
        }
    }

    private void InvestigateCorpse(Corpse corpse)
    {
        if (collectedEvidence.Count >= maxClues) return;

        // Get clues from the corpse
        List<string> clues = corpse.GetClues();
        foreach (string clue in clues)
        {
            AddClue(corpse.VictimId, clue);
        }

        UIManager.Instance.ShowNotification("Evidence collected from the body", 3f);
    }

    private void CollectEvidence(Evidence evidence)
    {
        if (collectedEvidence.Count >= maxClues) return;

        collectedEvidence.Add(evidence);
        AddClue(evidence.SuspectId, evidence.Clue);

        UIManager.Instance.ShowNotification("New evidence collected!", 3f);
    }

    private void AddClue(string suspectId, string clue)
    {
        if (!suspectClues.ContainsKey(suspectId))
        {
            suspectClues[suspectId] = new List<string>();
        }

        if (!suspectClues[suspectId].Contains(clue))
        {
            suspectClues[suspectId].Add(clue);
            photonView.RPC("SyncClue", RpcTarget.All, suspectId, clue);
        }
    }

    private void DisplayCollectedEvidence()
    {
        string evidenceText = "Collected Evidence:\n";
        foreach (var evidence in collectedEvidence)
        {
            evidenceText += $"- {evidence.Description}\n";
        }
        UIManager.Instance.ShowNotification(evidenceText, 5f);
    }

    private void DisplayFinalResults()
    {
        string resultsText = "Investigation Results:\n";
        foreach (var suspect in suspectClues)
        {
            resultsText += $"Suspect {suspect.Key}:\n";
            foreach (var clue in suspect.Value)
            {
                resultsText += $"- {clue}\n";
            }
        }
        UIManager.Instance.ShowNotification(resultsText, 10f);
    }

    [PunRPC]
    private void SyncClue(string suspectId, string clue)
    {
        if (!suspectClues.ContainsKey(suspectId))
        {
            suspectClues[suspectId] = new List<string>();
        }

        if (!suspectClues[suspectId].Contains(clue))
        {
            suspectClues[suspectId].Add(clue);
        }
    }
} 