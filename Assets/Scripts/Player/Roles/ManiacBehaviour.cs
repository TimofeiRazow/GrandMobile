using UnityEngine;
using Photon.Pun;

public class ManiacBehaviour : BaseRoleBehaviour
{
    [Header("Maniac Settings")]
    [SerializeField] private float killRange = 2f;
    [SerializeField] private float dragSpeed = 2f;
    [SerializeField] private LayerMask killableLayers;

    private bool isDraggingBody = false;
    private Transform draggedBody = null;
    private Vector3 dragOffset;

    public override void Initialize()
    {
        // Initialize maniac-specific UI and abilities
        UIManager.Instance.ShowNotification("You are the Maniac!", 5f);
    }

    public override void OnDayPhaseStart()
    {
        // Hide maniac-specific UI elements
        isDraggingBody = false;
        draggedBody = null;
    }

    public override void OnNightPhaseStart()
    {
        // Show maniac-specific UI elements
        UIManager.Instance.ShowNotification("Night Phase - You can kill!", 3f);
    }

    public override void OnVotingPhaseStart()
    {
        // Handle voting phase start
        isDraggingBody = false;
        draggedBody = null;
    }

    public override void OnGameOver(bool maniacsWin)
    {
        // Handle game over state
        isDraggingBody = false;
        draggedBody = null;
    }

    public override void HandleInteraction(IInteractable interactable)
    {
        if (interactable is Corpse corpse)
        {
            if (!isDraggingBody)
            {
                StartDragging(corpse.transform);
            }
            else
            {
                StopDragging();
            }
        }
    }

    public override void HandleInvestigation(IInvestigateable investigateable)
    {
        // Maniacs can't investigate
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (isDraggingBody && draggedBody != null)
        {
            UpdateDraggedBody();
        }

        if (Input.GetKeyDown(KeyCode.Q) && GameManager.Instance.GetCurrentPhase() == GameManager.GamePhase.Night)
        {
            TryKill();
        }
    }

    private void TryKill()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerController.transform.position, playerController.transform.forward, out hit, killRange, killableLayers))
        {
            PlayerController targetPlayer = hit.collider.GetComponent<PlayerController>();
            if (targetPlayer != null && GameManager.Instance.IsPlayerAlive(targetPlayer.photonView.Owner.UserId))
            {
                photonView.RPC("KillPlayer", RpcTarget.All, targetPlayer.photonView.Owner.UserId);
            }
        }
    }

    private void StartDragging(Transform body)
    {
        isDraggingBody = true;
        draggedBody = body;
        dragOffset = body.position - transform.position;
        
        // Notify the corpse that it's being dragged
        Corpse corpse = body.GetComponent<Corpse>();
        if (corpse != null)
        {
            corpse.OnStartDragging();
        }
    }

    private void StopDragging()
    {
        isDraggingBody = false;
        
        // Notify the corpse that it's no longer being dragged
        if (draggedBody != null)
        {
            Corpse corpse = draggedBody.GetComponent<Corpse>();
            if (corpse != null)
            {
                corpse.OnStopDragging();
            }
        }
        
        draggedBody = null;
    }

    private void UpdateDraggedBody()
    {
        if (draggedBody != null)
        {
            Vector3 targetPosition = transform.position + transform.forward * dragOffset.magnitude;
            draggedBody.position = Vector3.Lerp(draggedBody.position, targetPosition, dragSpeed * Time.deltaTime);
        }
    }

    [PunRPC]
    private void KillPlayer(string victimId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.ReportBody(victimId);
        }
    }
} 