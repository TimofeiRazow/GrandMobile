using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Fsm.States
{
    public class DeathState : IState
    {
        private static readonly int Death = Animator.StringToHash("Death");
        private static readonly int IsDead = Animator.StringToHash("IsDead");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

        private readonly CharacterView _characterView;

        private IStateSwitcher _stateSwitcher;

        public DeathState(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Initialize(IStateSwitcher stateSwitcher)
        {
            _stateSwitcher = stateSwitcher;
        }

        public void Enter()
        {
            Debug.Log($"[DeathState] {_characterView.Character.Role} has died!");

            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetTrigger(Death);
                _characterView.Animator.SetBool(IsDead, true);
                _characterView.Animator.SetFloat(Speed, 0f);
                _characterView.Animator.SetBool(IsMoving, false);
            }

            DisableCharacterInteraction();
        }

        public void Exit()
        {
            Debug.Log($"[DeathState] {_characterView.Character.Role} is no longer in death state");
        }

        public void Update(float deltaTime)
        {
            // В состоянии смерти персонаж не может ничего делать
        }

        private void DisableCharacterInteraction()
        {
            if (_characterView.Body != null)
            {
                Color currentColor = _characterView.Body.material.color;
                currentColor.a = 0.5f;
                _characterView.Body.material.color = currentColor;
            }

            if (_characterView.UiBar != null)
            {
                _characterView.UiBar.text = $"{_characterView.Character.Role} - МЕРТВ";
                _characterView.UiBar.color = Color.red;
            }

            Debug.Log($"[DeathState] Character interaction disabled for {_characterView.Character.Role}");
        }
    }
}