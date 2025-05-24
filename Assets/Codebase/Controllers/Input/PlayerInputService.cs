using UnityEngine;
using UnityEngine.InputSystem;

namespace Codebase.Controllers.Input
{
    public class PlayerInputService : IInputService
    {
        private readonly InputSystem_Actions _inputActions;

        public PlayerInputService()
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.Enable();
        }

        ~PlayerInputService()
        {
            _inputActions?.Disable();
            _inputActions?.Dispose();
        }

        public Vector2 GetMovementInput()
        {
            return _inputActions.Player.Move.ReadValue<Vector2>();
        }

        public Vector2 GetLookInput()
        {
            return _inputActions.Player.Look.ReadValue<Vector2>();
        }

        public bool IsAttackPressed()
        {
            return _inputActions.Player.Attack.WasPressedThisFrame();
        }

        public bool IsInteractPressed()
        {
            return _inputActions.Player.Interact.IsPressed();
        }

        public bool IsSprintPressed()
        {
            return _inputActions.Player.Sprint.IsPressed();
        }

        public bool IsJumpPressed()
        {
            return _inputActions.Player.Jump.WasPressedThisFrame();
        }

        public bool IsCrouchPressed()
        {
            return _inputActions.Player.Crouch.WasPressedThisFrame();
        }
    }
}