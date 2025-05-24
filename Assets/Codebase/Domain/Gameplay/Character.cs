using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class Character
    {
        public Role Role { get; set; }
        public Transform Transform { get; set; }
        public RoleAction CurrentAction { get; private set; }

        public void SetRoleAction(RoleAction roleAction)
        {
            CurrentAction = roleAction;
        }

        public void ExecuteRoleAction()
        {
            CurrentAction?.Execute();
        }

        public bool HasRoleAction => CurrentAction != null;
    }
}