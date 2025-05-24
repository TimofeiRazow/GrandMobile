using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Domain.Gameplay;
using UnityEngine;

namespace Codebase.Infrastructure.Services
{
    public class TaskProgressService
    {
        private readonly List<TaskObject> _allTasks = new List<TaskObject>();
        private readonly ActionObjectProvider _actionObjectProvider;

        // События
        public event Action<TaskObject> TaskRegistered;
        public event Action<TaskObject> TaskCompleted;
        public event Action<TaskObject, int, int> TaskProgressChanged; // (task, current, max)
        public event Action<int> OverallProgressChanged; // percentage
        public event Action AllTasksCompleted;

        // Статистика
        public int TotalTasks => _allTasks.Count;
        public int CompletedTasks => _allTasks.Count(t => t.IsCompleted);
        public int IncompleteTasks => _allTasks.Count(t => !t.IsCompleted);
        public int OverallProgress => CalculateOverallProgress();

        public TaskProgressService(ActionObjectProvider actionObjectProvider)
        {
            _actionObjectProvider = actionObjectProvider ?? throw new ArgumentNullException(nameof(actionObjectProvider));
        }

        public void RegisterTask(TaskObject task)
        {
            if (task == null || _allTasks.Contains(task))
                return;

            _allTasks.Add(task);
            _actionObjectProvider.RegisterActionObject(task);

            // Подписываемся на события задачи
            task.OnProgressChanged += OnTaskProgress;
            task.OnTaskCompleted += OnTaskCompleted;
            
            TaskRegistered?.Invoke(task);
            UpdateOverallProgress();

            Debug.Log($"[TaskProgressService] Registered task: {task.TaskName}");
        }

        public void UnregisterTask(TaskObject task)
        {
            if (task == null || !_allTasks.Contains(task))
                return;

            // Отписываемся от событий
            task.OnProgressChanged -= OnTaskProgress;
            task.OnTaskCompleted -= OnTaskCompleted;
            
            _allTasks.Remove(task);
            UpdateOverallProgress();

            Debug.Log($"[TaskProgressService] Unregistered task: {task.TaskName}");
        }

        private void OnTaskProgress(TaskObject task, int current, int max)
        {
            TaskProgressChanged?.Invoke(task, current, max);
            UpdateOverallProgress();
            
            Debug.Log($"[TaskProgressService] Task '{task.TaskName}' progress: {current}/{max} ({task.ProgressPercentage:P1})");
        }

        private void OnTaskCompleted(TaskObject task)
        {
            TaskCompleted?.Invoke(task);
            UpdateOverallProgress();
            
            Debug.Log($"[TaskProgressService] Task completed: {task.TaskName}");

            // Проверяем, все ли задачи выполнены
            if (AreAllTasksCompleted())
            {
                AllTasksCompleted?.Invoke();
                Debug.Log("[TaskProgressService] All tasks completed!");
            }
        }

        private int CalculateOverallProgress()
        {
            if (_allTasks.Count == 0)
                return 100;

            int totalCurrentProgress = _allTasks.Sum(t => t.CurrentProgress);
            int totalMaxProgress = _allTasks.Sum(t => t.MaxProgress);

            return totalMaxProgress > 0 ? (totalCurrentProgress * 100) / totalMaxProgress : 0;
        }

        private void UpdateOverallProgress()
        {
            int progress = CalculateOverallProgress();
            OverallProgressChanged?.Invoke(progress);
        }

        public bool AreAllTasksCompleted()
        {
            return _allTasks.Count > 0 && _allTasks.All(t => t.IsCompleted);
        }

        public List<TaskObject> GetIncompleteTasks()
        {
            return _allTasks.Where(t => !t.IsCompleted).ToList();
        }

        public List<TaskObject> GetCompletedTasks()
        {
            return _allTasks.Where(t => t.IsCompleted).ToList();
        }

        public List<TaskObject> GetTasksForRole(Role role)
        {
            return _allTasks.Where(t => t.AllowedRoles.Count == 0 || t.AllowedRoles.Contains(role)).ToList();
        }

        public TaskObject GetNearestIncompleteTask(Vector3 position, Role role)
        {
            var availableTasks = GetIncompleteTasks()
                .Where(t => t.AllowedRoles.Count == 0 || t.AllowedRoles.Contains(role))
                .ToList();

            TaskObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var task in availableTasks)
            {
                float distance = Vector3.Distance(position, task.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = task;
                }
            }

            return nearest;
        }

        public TaskObject GetMostProgressedTask()
        {
            return _allTasks
                .Where(t => !t.IsCompleted)
                .OrderByDescending(t => t.ProgressPercentage)
                .FirstOrDefault();
        }

        public TaskObject GetLeastProgressedTask()
        {
            return _allTasks
                .Where(t => !t.IsCompleted)
                .OrderBy(t => t.ProgressPercentage)
                .FirstOrDefault();
        }

        // Автообнаружение задач на сцене
        public void DiscoverTasks()
        {
            var foundTasks = UnityEngine.Object.FindObjectsOfType<TaskObject>();
            
            foreach (var task in foundTasks)
            {
                RegisterTask(task);
            }

            Debug.Log($"[TaskProgressService] Discovered {foundTasks.Length} tasks on scene");
        }

        public void Clear()
        {
            // Отписываемся от всех событий
            foreach (var task in _allTasks)
            {
                task.OnProgressChanged -= OnTaskProgress;
                task.OnTaskCompleted -= OnTaskCompleted;
            }
            
            _allTasks.Clear();
        }

        // Статистика и отладка
        public void LogStatistics()
        {
            Debug.Log($"[TaskProgressService] === TASK STATISTICS ===");
            Debug.Log($"[TaskProgressService] Total tasks: {TotalTasks}");
            Debug.Log($"[TaskProgressService] Completed: {CompletedTasks}");
            Debug.Log($"[TaskProgressService] Incomplete: {IncompleteTasks}");
            Debug.Log($"[TaskProgressService] Overall progress: {OverallProgress}%");
            
            foreach (var task in _allTasks)
            {
                string status = task.IsCompleted ? "✓" : "○";
                Debug.Log($"[TaskProgressService] {status} {task.TaskName}: {task.CurrentProgress}/{task.MaxProgress} ({task.ProgressPercentage:P1})");
            }
        }
    }
} 