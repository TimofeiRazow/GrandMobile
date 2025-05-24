using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Fsm.States
{
    public class ActionState : IState
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        private readonly CharacterView _characterView;
        private readonly float _actionDuration = 1f;
        private readonly float _gravity = -9.81f;

        private IStateSwitcher _stateSwitcher;
        private FsmBase _fsm;
        private float _actionTimer;
        private Vector3 _velocity;

        public ActionState(CharacterView characterView)
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
            Debug.Log($"[ActionState] Entering for {_characterView.Character.Role}");

            _actionTimer = _actionDuration;

            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetTrigger(Attack);
                _characterView.Animator.SetBool(IsAttacking, true);
            }

            PerformRoleAction();
        }

        public void Exit()
        {
            Debug.Log($"[ActionState] Exiting for {_characterView.Character.Role}");

            if (_characterView.Animator != null)
                _characterView.Animator.SetBool(IsAttacking, false);
        }

        public void Update(float deltaTime)
        {
            _actionTimer -= deltaTime;

            HandleGravity(deltaTime);

            if (_actionTimer <= 0f)
            {
                _stateSwitcher.Switch<IdleState>();
                return;
            }

            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetFloat(Speed, 0f);
                _characterView.Animator.SetBool(IsMoving, false);
            }
        }

        private void HandleGravity(float deltaTime)
        {
            if (_characterView.CharacterController.isGrounded)
            {
                _velocity.y = -2f;
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
            {
                _characterView.Animator.SetBool(IsGrounded, _characterView.CharacterController.isGrounded);
            }
        }

        private void PerformRoleAction()
        {
            switch (_characterView.Character.Role)
            {
                case Domain.Gameplay.Role.Mafia:
                    Debug.Log($"[ActionState] Mafia is performing elimination action!");
                    break;

                case Domain.Gameplay.Role.Police:
                    Debug.Log($"[ActionState] Police is performing investigation action!");
                    break;

                case Domain.Gameplay.Role.Civilian:
                    Debug.Log($"[ActionState] Civilian is performing vote action!");
                    break;

                default:
                    Debug.Log($"[ActionState] Unknown role performing action!");
                    break;
            }

            if (_characterView.AudioSource != null)
            {
                // _characterView.AudioSource.PlayOneShot(actionSound);
            }
        }
    }
}