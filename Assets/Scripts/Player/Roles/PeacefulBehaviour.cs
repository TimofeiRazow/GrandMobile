using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PeacefulBehaviour : BaseRoleBehaviour
{
    [Header("Peaceful Settings")]
    [SerializeField] private float reportRange = 2f;
    [SerializeField] private LayerMask corpseLayers;

    public override void Initialize()
    {
        UIManager.Instance.ShowNotification("You are a Civilian!", 5f);
    }

    public override void OnDayPhaseStart()
    {
        // Reset UI elements
    }

    public override void OnNightPhaseStart()
    {
        UIManager.Instance.ShowNotification("Night Phase - Be careful!", 3f);
    }

    public override void OnVotingPhaseStart()
    {
        // Show voting UI
    }

    public override void OnGameOver(bool maniacsWin)
    {
        // Show game over UI
    }

    public override void HandleInteraction(IInteractable interactable)
    {
        if (interactable is Corpse corpse)
        {
            ReportCorpse(corpse);
        }
    }

    public override void HandleInvestigation(IInvestigateable investigateable)
    {
        // Civilians can't investigate
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryReportCorpse();
        }
    }

    private void TryReportCorpse()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerController.transform.position, playerController.transform.forward, out hit, reportRange, corpseLayers))
        {
            Corpse corpse = hit.collider.GetComponent<Corpse>();
            if (corpse != null)
            {
                ReportCorpse(corpse);
            }
        }
    }

    private void ReportCorpse(Corpse corpse)
    {
        if (corpse.IsReported) return;

        photonView.RPC("ReportBody", RpcTarget.All, corpse.VictimId);
        UIManager.Instance.ShowNotification("Body reported!", 3f);
    }

    [PunRPC]
    private void ReportBody(string victimId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.ReportBody(victimId);
        }
    }
} 