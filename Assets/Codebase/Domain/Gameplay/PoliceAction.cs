using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class PoliceAction : RoleAction
    {
        public override void Execute()
        {
            string[] actions = { 
                "патрулирует территорию", 
                "ищет подозрительную активность", 
                "анализирует обстановку", 
                "проверяет улики",
                "наблюдает за подозреваемыми"
            };
            
            string action = actions[Random.Range(0, actions.Length)];
            Debug.Log($"[PoliceAction] Детектив {action}");
        }
    }
} 