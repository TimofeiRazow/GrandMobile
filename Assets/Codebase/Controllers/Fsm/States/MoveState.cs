using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Fsm.States
{
    public class MoveState : IState
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jump = Animator.StringToHash("Jump");

        private readonly CharacterView _characterView;
        private readonly float _moveSpeed = 5f;
        private readonly float _mouseSensitivity = 2f;
        private readonly float _gravity = -9.81f;

        private IStateSwitcher _stateSwitcher;
        private FsmBase _fsm;
        private Vector3 _velocity;

        public MoveState(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Initialize(IStateSwitcher stateSwitcher)
        {
            _stateSwitcher = stateSwitcher;
            _fsm = stateSwitcher as FsmBase;
        }

        public void Enter()
        {
            Debug.Log($"[MoveState] Entering for {_characterView.Character.Role}");
        }

        public void Exit()
        {
            Debug.Log($"[MoveState] Exiting for {_characterView.Character.Role}");
        }

        public void Update(float deltaTime)
        {
            HandleMouseLook(deltaTime);
            HandleMovement(deltaTime);
            
            // Гравитация только для CharacterController
            if (_characterView.CharacterController != null && _characterView.CharacterController.enabled)
            {
                HandleGravity(deltaTime);
            }

            CheckStateTransitions();
        }

        private void HandleMouseLook(float deltaTime)
        {
            Vector2 lookInput = _characterView.InputService.GetLookInput();

            if (lookInput.magnitude > 0.1f)
            {
                float yRotation = lookInput.x * _mouseSensitivity * deltaTime;
                _characterView.Transform.Rotate(0, yRotation, 0);
            }
        }

        private void HandleMovement(float deltaTime)
        {
            Vector2 inputVector = _characterView.InputService.GetMovementInput();

            if (inputVector.magnitude < 0.1f)
            {
                _stateSwitcher.Switch<IdleState>();
                return;
            }

            // Для NavMeshAgent движение уже обрабатывается в AiInputService
            if (_characterView.NavMeshAgent != null && _characterView.NavMeshAgent.enabled)
            {
                HandleNavMeshMovement(inputVector);
            }
            else if (_characterView.CharacterController != null && _characterView.CharacterController.enabled)
            {
                HandleCharacterControllerMovement(inputVector, deltaTime);
            }

            // Обновляем аниматор независимо от типа движения
            UpdateAnimator(inputVector.magnitude, _characterView.InputService.IsSprintPressed());
        }

        private void HandleNavMeshMovement(Vector2 inputVector)
        {
            // Для NavMeshAgent просто обновляем аниматор
            // Само движение обрабатывается в AiInputService
            bool isSprinting = _characterView.InputService.IsSprintPressed();
            
            if (isSprinting && _characterView.NavMeshAgent != null)
            {
                _characterView.NavMeshAgent.speed = 5.25f; // 1.5x базовой скорости
            }
            else if (_characterView.NavMeshAgent != null)
            {
                _characterView.NavMeshAgent.speed = 3.5f; // Базовая скорость
            }
        }

        private void HandleCharacterControllerMovement(Vector2 inputVector, float deltaTime)
        {
            Vector3 forward = _characterView.Transform.forward;
            Vector3 right = _characterView.Transform.right;

            Vector3 movement = (forward * inputVector.y + right * inputVector.x).normalized;

            float currentSpeed = _moveSpeed;
            if (_characterView.InputService.IsSprintPressed())
            {
                currentSpeed *= 1.5f;
            }

            Vector3 moveVector = movement * (currentSpeed * deltaTime);
            moveVector.y = _velocity.y * deltaTime;

            _characterView.CharacterController.Move(moveVector);
        }

        private void HandleGravity(float deltaTime)
        {
            if (_characterView.CharacterController.isGrounded)
            {
                _velocity.y = 0f;

                if (_characterView.InputService.IsJumpPressed())
                    HandleJump();
            }
            else
            {
                _velocity.y += _gravity * deltaTime;
            }
        }

        private void UpdateAnimator(float movementMagnitude, bool isSprinting)
        {
            if (_characterView.Animator != null)
            {
                // Для NavMeshAgent используем фактическую скорость движения
                float actualSpeed = _characterView.GetMovementSpeed();
                float normalizedSpeed = actualSpeed / 5f; // Нормализуем к максимальной скорости
                
                _characterView.Animator.SetFloat(Speed, normalizedSpeed);
                _characterView.Animator.SetBool(IsRunning, isSprinting);
                _characterView.Animator.SetBool(IsMoving, movementMagnitude > 0.1f || actualSpeed > 0.1f);
                _characterView.Animator.SetBool(IsGrounded, _characterView.IsGrounded());
            }
        }

        private void CheckStateTransitions()
        {
            if (_characterView.InputService.IsAttackPressed())
                _stateSwitcher.Switch<ActionState>();
        }

        private void HandleJump()
        {
            float jumpHeight = 2f;
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * _gravity);

            Debug.Log($"[MoveState] {_characterView.Character.Role} is jumping!");

            if (_characterView.Animator != null)
                _characterView.Animator.SetTrigger(Jump);
        }
    }
}