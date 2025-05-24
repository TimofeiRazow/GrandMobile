using Codebase.Domain.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Input
{
    public class AiInputService : IInputService
    {
        private Character _character;
        
        // Простые данные ввода, которые будут устанавливаться состояниями
        private Vector2 _currentMovementInput;
        private Vector2 _currentLookInput;
        private bool _isAttackPressed;
        private bool _isInteractPressed;
        private bool _isSprintPressed;
        private bool _isJumpPressed;
        private bool _isCrouchPressed;
        
        public void Initialize(Character character, Transform transform)
        {
            _character = character;
            Debug.Log($"[AiInputService] Initialized simple AI input for {_character.Role}");
        }

        // Методы для получения данных (используются FSM)
        public Vector2 GetMovementInput() => _currentMovementInput;
        public Vector2 GetLookInput() => _currentLookInput;
        public bool IsAttackPressed() => _isAttackPressed;
        public bool IsInteractPressed() => _isInteractPressed;
        public bool IsSprintPressed() => _isSprintPressed;
        public bool IsJumpPressed() => _isJumpPressed;
        public bool IsCrouchPressed() => _isCrouchPressed;

        // Методы для установки данных (используются AI состояниями)
        public void SetMovementInput(Vector2 input) => _currentMovementInput = input;
        public void SetLookInput(Vector2 input) => _currentLookInput = input;
        public void SetAttackPressed(bool pressed) => _isAttackPressed = pressed;
        public void SetInteractPressed(bool pressed) => _isInteractPressed = pressed;
        public void SetSprintPressed(bool pressed) => _isSprintPressed = pressed;
        public void SetJumpPressed(bool pressed) => _isJumpPressed = pressed;
        public void SetCrouchPressed(bool pressed) => _isCrouchPressed = pressed;

        // Сброс всех входных данных
        public void ResetInputs()
        {
            _currentMovementInput = Vector2.zero;
            _currentLookInput = Vector2.zero;
            _isAttackPressed = false;
            _isInteractPressed = false;
            _isSprintPressed = false;
            _isJumpPressed = false;
            _isCrouchPressed = false;
        }

        public void Disable()
        {
            ResetInputs();
        }
    }
}