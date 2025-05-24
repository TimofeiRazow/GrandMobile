using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class RepairStationObject : TaskObject
    {
        [Header("Repair Station")]
        [SerializeField] private string _stationType = "Engine";
        [SerializeField] private bool _canBeSabotaged = true;
        [SerializeField] private int _sabotageAmount = 20;

        // protected override void Awake()
        // {
        //     base.Awake();
        //     
        //     // Настройки для ремонтной станции
        //     _actionType = ActionType.Repair;
        //     _allowedRoles.Clear();
        //     _allowedRoles.Add(Role.Civilian); // Только мирные могут чинить
        //     
        //     _actionDuration = 3f; // Ремонт занимает 3 секунды
        //     _maxProgress = 100;
        //     _progressPerAction = 15; // 15% за одно действие
        //     _taskName = $"Repair {_stationType}";
        //     _taskDescription = $"Fix the {_stationType} to help the team escape";
        // }

        protected override void OnActionStart(CharacterView character)
        {
            base.OnActionStart(character);
            
            if (character.Character.Role == Role.Civilian)
            {
                Debug.Log($"[RepairStation] {character.Character.Role} начал ремонт {_stationType}");
                
                // Можно добавить звук работы, партиклы искр и т.д.
                ShowRepairEffect();
            }
        }

        protected override void OnActionComplete(CharacterView character)
        {
            if (character.Character.Role == Role.Civilian)
            {
                base.OnActionComplete(character);
                Debug.Log($"[RepairStation] {character.Character.Role} продвинул ремонт {_stationType} на {_progressPerAction}%");
            }
            else if (character.Character.Role == Role.Mafia && _canBeSabotaged)
            {
                // Мафия может саботировать
                PerformSabotage(character);
            }
        }

        private void PerformSabotage(CharacterView saboteur)
        {
            if (!_canBeSabotaged)
                return;

            Sabotage(_sabotageAmount);
            Debug.Log($"[RepairStation] {saboteur.Character.Role} саботировал {_stationType}! Потеряно {_sabotageAmount}% прогресса.");
            
            ShowSabotageEffect();
        }

        private void ShowRepairEffect()
        {
            // Здесь можно добавить:
            // - Частицы искр
            // - Звук дрели/молотка
            // - Изменение освещения
            Debug.Log($"[RepairStation] Показываем эффект ремонта для {_stationType}");
        }

        private void ShowSabotageEffect()
        {
            // Здесь можно добавить:
            // - Частицы дыма
            // - Звук поломки
            // - Красное освещение
            Debug.Log($"[RepairStation] Показываем эффект саботажа для {_stationType}");
        }

        // Переопределяем CanInteract для поддержки саботажа
        public override bool CanInteract(CharacterView character)
        {
            // Мирные могут чинить незавершенные задачи
            if (character.Character.Role == Role.Civilian && !IsCompleted)
            {
                return base.CanInteract(character);
            }
            
            // Мафия может саботировать если есть прогресс
            if (character.Character.Role == Role.Mafia && _canBeSabotaged && CurrentProgress > 0)
            {
                float distance = Vector3.Distance(transform.position, character.Transform.position);
                return distance <= InteractionRange;
            }
            
            return false;
        }

        // Метод для внешнего отключения саботажа (например, при определенных условиях)
        public void SetSabotageEnabled(bool enabled)
        {
            _canBeSabotaged = enabled;
        }

        // Получить информацию о станции
        public string GetStationInfo()
        {
            return $"{_stationType} Station - Progress: {CurrentProgress}/{MaxProgress} ({ProgressPercentage:P0})";
        }
    }
} 