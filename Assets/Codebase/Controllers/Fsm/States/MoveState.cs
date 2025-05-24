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
            HandleGravity(deltaTime);

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

            Vector3 forward = _characterView.Transform.forward;
            Vector3 right = _characterView.Transform.right;

            Vector3 movement = (forward * inputVector.y + right * inputVector.x).normalized;

            float currentSpeed = _moveSpeed;
            if (_characterView.InputService.IsSprintPressed())
            {
                currentSpeed *= 1.5f; // Увеличиваем скорость в 1.5 раза при беге
            }

            Vector3 moveVector = movement * (currentSpeed * deltaTime);

            moveVector.y = _velocity.y * deltaTime;

            _characterView.CharacterController.Move(moveVector);

            UpdateAnimator(inputVector.magnitude, _characterView.InputService.IsSprintPressed());
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
                _characterView.Animator.SetFloat(Speed, movementMagnitude);
                _characterView.Animator.SetBool(IsRunning, isSprinting);
                _characterView.Animator.SetBool(IsMoving, movementMagnitude > 0.1f);
                _characterView.Animator.SetBool(IsGrounded, _characterView.CharacterController.isGrounded);
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