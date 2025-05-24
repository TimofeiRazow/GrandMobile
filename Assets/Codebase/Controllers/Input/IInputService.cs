using UnityEngine;

namespace Codebase.Controllers.Input
{
    public interface IInputService
    {
        Vector2 GetMovementInput();
        Vector2 GetLookInput();
        bool IsAttackPressed();
        bool IsInteractPressed();
        bool IsSprintPressed();
        bool IsJumpPressed();
        bool IsCrouchPressed();
        void Disable();
    }
}