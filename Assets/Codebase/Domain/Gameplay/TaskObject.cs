using System;
using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class TaskObject : ActionObject
    {
        [Header("Task Settings")]
        [SerializeField]
        protected int _maxProgress = 100;
        [SerializeField] protected int _progressPerAction = 10;
        [SerializeField] protected string _taskName = "Repair Task";
        [SerializeField] protected string _taskDescription = "Repair this object to help the team";
        
        private int _currentProgress = 0;
        private bool _isCompleted = false;

        // События для прогресса
        public event Action<TaskObject, int, int> OnProgressChanged; // (task, current, max)
        public event Action<TaskObject> OnTaskCompleted;

        public int CurrentProgress => _currentProgress;
        public int MaxProgress => _maxProgress;
        public float ProgressPercentage => (float)_currentProgress / _maxProgress;
        public bool IsCompleted => _isCompleted;
        public string TaskName => _taskName;
        public string TaskDescription => _taskDescription;

        protected override void Awake()
        {
            base.Awake();
            
            // TaskObject всегда можно использовать пока не завершен
            _isOneTimeUse = false;
        }

        public override bool CanInteract(CharacterView character)
        {
            // Нельзя взаимодействовать с завершенной задачей
            if (_isCompleted)
                return false;

            return base.CanInteract(character);
        }

        protected override void OnActionStart(CharacterView character)
        {
            // Начинаем работу над задачей
            Debug.Log($"[TaskObject] {character.Character.Role} started working on {_taskName}");
        }

        protected override void OnActionComplete(CharacterView character)
        {
            // Добавляем прогресс
            AddProgress(_progressPerAction);
            
            Debug.Log($"[TaskObject] {character.Character.Role} added {_progressPerAction} progress to {_taskName}. Progress: {_currentProgress}/{_maxProgress}");
        }

        protected override void OnActionInterrupt(CharacterView character)
        {
            // При прерывании добавляем частичный прогресс
            int partialProgress = Mathf.RoundToInt(_progressPerAction * 0.5f);
            AddProgress(partialProgress);
            
            Debug.Log($"[TaskObject] {character.Character.Role} was interrupted. Added partial progress: {partialProgress}");
        }

        public void AddProgress(int amount)
        {
            if (_isCompleted)
                return;

            int oldProgress = _currentProgress;
            _currentProgress = Mathf.Clamp(_currentProgress + amount, 0, _maxProgress);

            // Уведомляем об изменении прогресса
            if (_currentProgress != oldProgress)
            {
                OnProgressChanged?.Invoke(this, _currentProgress, _maxProgress);
                
                // Проверяем завершение
                if (_currentProgress >= _maxProgress && !_isCompleted)
                {
                    CompleteTask();
                }
            }
        }

        public void RemoveProgress(int amount)
        {
            if (_isCompleted)
                return;

            int oldProgress = _currentProgress;
            _currentProgress = Mathf.Clamp(_currentProgress - amount, 0, _maxProgress);

            if (_currentProgress != oldProgress)
            {
                OnProgressChanged?.Invoke(this, _currentProgress, _maxProgress);
            }
        }

        private void CompleteTask()
        {
            _isCompleted = true;
            _isCurrentlyUsable = false;
            
            Debug.Log($"[TaskObject] Task {_taskName} completed!");
            OnTaskCompleted?.Invoke(this);
        }

        public void ResetTask()
        {
            _currentProgress = 0;
            _isCompleted = false;
            _isCurrentlyUsable = true;
            
            OnProgressChanged?.Invoke(this, _currentProgress, _maxProgress);
        }

        // Методы для саботажа (мафия может уменьшать прогресс)
        public void Sabotage(int damage)
        {
            if (_isCompleted)
                return;

            RemoveProgress(damage);
            Debug.Log($"[TaskObject] {_taskName} was sabotaged! Lost {damage} progress.");
        }

        // Визуализация прогресса
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // Рисуем прогресс-бар над объектом
            Vector3 barPosition = transform.position + Vector3.up * 3f;
            Vector3 barSize = new Vector3(2f, 0.2f, 0.1f);
            
            // Фон прогресс-бара
            Gizmos.color = Color.gray;
            Gizmos.DrawCube(barPosition, barSize);
            
            // Заполненная часть
            if (_maxProgress > 0)
            {
                float fillPercentage = (float)_currentProgress / _maxProgress;
                Vector3 fillSize = new Vector3(barSize.x * fillPercentage, barSize.y, barSize.z);
                Vector3 fillPosition = barPosition + Vector3.left * (barSize.x * (1f - fillPercentage) * 0.5f);
                
                Gizmos.color = _isCompleted ? Color.green : Color.blue;
                Gizmos.DrawCube(fillPosition, fillSize);
            }
        }

        // Статические методы для общей статистики
        public static int GetTotalTaskProgress()
        {
            var tasks = FindObjectsOfType<TaskObject>();
            int totalProgress = 0;
            int totalMaxProgress = 0;

            foreach (var task in tasks)
            {
                totalProgress += task._currentProgress;
                totalMaxProgress += task._maxProgress;
            }

            return totalMaxProgress > 0 ? (totalProgress * 100) / totalMaxProgress : 0;
        }

        public static int GetCompletedTasksCount()
        {
            var tasks = FindObjectsOfType<TaskObject>();
            int completed = 0;

            foreach (var task in tasks)
            {
                if (task.IsCompleted)
                    completed++;
            }

            return completed;
        }
    }
} 