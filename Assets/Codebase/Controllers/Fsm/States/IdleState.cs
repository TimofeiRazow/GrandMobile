using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Fsm.States
{
    public class IdleState : IState
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jump = Animator.StringToHash("Jump");

        private readonly CharacterView _characterView;
        private readonly float _mouseSensitivity = 2f;
        private readonly float _gravity = -9.81f;

        private IStateSwitcher _stateSwitcher;
        private FsmBase _fsm;
        private Vector3 _velocity;

        public IdleState(CharacterView characterView)
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
            Debug.Log($"[IdleState] Entering for {_characterView.Character.Role}");

            // Устанавливаем параметры аниматора для состояния покоя
            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetFloat(Speed, 0f);
                _characterView.Animator.SetBool(IsRunning, false);
                _characterView.Animator.SetBool(IsMoving, false);
            }
        }

        public void Exit()
        {
            Debug.Log($"[IdleState] Exiting for {_characterView.Character.Role}");
        }

        public void Update(float deltaTime)
        {
            HandleMouseLook(deltaTime);
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

        private void HandleGravity(float deltaTime)
        {
            if (_characterView.CharacterController.isGrounded)
            {
                _velocity.y = 0;

                if (_characterView.InputService.IsJumpPressed())
                    HandleJump();
            }
            else
            {
                _velocity.y += _gravity * deltaTime;
            }

            if (!_characterView.CharacterController.isGrounded || _velocity.y < 0)
            {
                Vector3 moveVector = new Vector3(0, _velocity.y * deltaTime, 0);
                _characterView.CharacterController.Move(moveVector);
            }

            if (_characterView.Animator != null)
                _characterView.Animator.SetBool(IsGrounded, _characterView.CharacterController.isGrounded);
        }

        private void CheckStateTransitions()
        {
            Vector2 inputVector = _characterView.InputService.GetMovementInput();

            if (inputVector.magnitude > 0.1f)
            {
                _stateSwitcher.Switch<MoveState>();
                return;
            }

            if (_characterView.InputService.IsAttackPressed())
                _stateSwitcher.Switch<ActionState>();
        }

        private void HandleJump()
        {
            float jumpHeight = 2f;
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * _gravity);

            Debug.Log($"[IdleState] {_characterView.Character.Role} is jumping!");

            if (_characterView.Animator != null)
                _characterView.Animator.SetTrigger(Jump);
        }
    }
}