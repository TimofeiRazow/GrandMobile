using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class CivilianAction : RoleAction
    {
        public override void Execute()
        {
            string[] actions = { 
                "ищет работу", 
                "проверяет безопасность", 
                "ищет инструменты", 
                "помогает команде",
                "выполняет задания"
            };
            
            string action = actions[Random.Range(0, actions.Length)];
            Debug.Log($"[CivilianAction] Гражданский {action}");
        }
    }
} 