using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class MafiaAction : RoleAction
    {
        public override void Execute()
        {
            string[] actions = { 
                "планирует злодейство", 
                "высматривает жертву", 
                "готовит саботаж", 
                "ищет укрытие",
                "изучает территорию"
            };
            
            string action = actions[Random.Range(0, actions.Length)];
            Debug.Log($"[MafiaAction] Мафия {action}");
        }
    }
} 