using UnityEngine;
using Photon.Pun;

public abstract class BaseRoleBehaviour : MonoBehaviourPunCallbacks
{
    protected PlayerController playerController;
    protected PhotonView photonView;

    protected virtual void Awake()
    {
        playerController = GetComponent<PlayerController>();
        photonView = GetComponent<PhotonView>();
    }

    public abstract void Initialize();
    public abstract void OnDayPhaseStart();
    public abstract void OnNightPhaseStart();
    public abstract void OnVotingPhaseStart();
    public abstract void OnGameOver(bool maniacsWin);
    public abstract void HandleInteraction(IInteractable interactable);
    public abstract void HandleInvestigation(IInvestigateable investigateable);
} 