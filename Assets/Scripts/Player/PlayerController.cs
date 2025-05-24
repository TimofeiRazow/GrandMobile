using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Game.Interfaces;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private float dragSpeed = 2f;

    private CharacterController characterController;
    private PhotonView photonView;
    private Camera playerCamera;
    private Vector3 moveDirection;
    private float verticalRotation = 0f;
    private bool isDragging = false;
    private Transform draggedObject = null;
    private Vector3 dragOffset;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();
        playerCamera = GetComponentInChildren<Camera>();

        if (!photonView.IsMine)
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        HandleMovement();
        HandleMouseLook();
        HandleInteractions();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (characterController.isGrounded)
        {
            moveDirection.y = -0.5f;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
            }
        }
        else
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        Vector3 movement = move * currentSpeed * Time.deltaTime;
        movement.y = moveDirection.y * Time.deltaTime;

        characterController.Move(movement);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleInteractions()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isDragging)
            {
                StopDragging();
            }
            else
            {
                TryInteract();
            }
        }

        if (isDragging && draggedObject != null)
        {
            UpdateDraggedObject();
        }
    }

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange, interactableLayers))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
    }

    public void StartDragging(Transform objectToDrag)
    {
        if (objectToDrag != null)
        {
            isDragging = true;
            draggedObject = objectToDrag;
            dragOffset = draggedObject.position - transform.position;
        }
    }

    private void StopDragging()
    {
        isDragging = false;
        draggedObject = null;
    }

    private void UpdateDraggedObject()
    {
        if (draggedObject != null)
        {
            Vector3 targetPosition = transform.position + transform.forward * dragOffset.magnitude;
            draggedObject.position = Vector3.Lerp(draggedObject.position, targetPosition, dragSpeed * Time.deltaTime);
        }
    }

    public void PerformRoleAction()
    {
        GameManager.GameRole role = GameManager.Instance.GetPlayerRole(PhotonNetwork.LocalPlayer.UserId);
        
        switch (role)
        {
            case GameManager.GameRole.Maniac:
                TryKill();
                break;
            case GameManager.GameRole.Detective:
                TryInvestigate();
                break;
        }
    }

    private void TryKill()
    {
        if (GameManager.Instance.GetCurrentPhase() != GameManager.GamePhase.Night)
            return;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange))
        {
            PlayerController targetPlayer = hit.collider.GetComponent<PlayerController>();
            if (targetPlayer != null && GameManager.Instance.IsPlayerAlive(targetPlayer.photonView.Owner.UserId))
            {
                photonView.RPC("KillPlayer", RpcTarget.All, targetPlayer.photonView.Owner.UserId);
            }
        }
    }

    private void TryInvestigate()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange))
        {
            IInvestigateable investigateable = hit.collider.GetComponent<IInvestigateable>();
            if (investigateable != null)
            {
                investigateable.Investigate(this);
            }
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

public interface IInteractable
{
    void Interact(PlayerController player);
}

public interface IInvestigateable
{
    void Investigate(PlayerController player);
} 