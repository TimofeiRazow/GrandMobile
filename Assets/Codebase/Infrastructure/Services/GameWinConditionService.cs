using System;
using Codebase.Domain.Gameplay;
using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Infrastructure.Services
{
    public enum WinCondition
    {
        None,
        MafiaWins,          // Мафия убила всех или их количество >= остальных
        CiviliansWin,       // Все задачи выполнены
        PoliceWins,         // Всех мафов нашли и исключили
        TasksCompleted      // Альтернативная победа мирных - все задачи выполнены
    }

    public class GameWinConditionService
    {
        private readonly CharacterLifecycleService _characterLifecycleService;
        private readonly TaskProgressService _taskProgressService;

        private WinCondition _currentWinCondition = WinCondition.None;
        private bool _gameEnded = false;

        // События
        public event Action<WinCondition> GameWon;
        public event Action<WinCondition, string> WinConditionChecked; // для отладки

        public WinCondition CurrentWinCondition => _currentWinCondition;
        public bool IsGameEnded => _gameEnded;

        public GameWinConditionService(CharacterLifecycleService characterLifecycleService, TaskProgressService taskProgressService)
        {
            _characterLifecycleService = characterLifecycleService ?? throw new ArgumentNullException(nameof(characterLifecycleService));
            _taskProgressService = taskProgressService ?? throw new ArgumentNullException(nameof(taskProgressService));

            // Подписываемся на события сервисов
            _characterLifecycleService.CharacterKilled += OnCharacterKilled;
            _characterLifecycleService.RoleCountChanged += OnRoleCountChanged;
            _taskProgressService.AllTasksCompleted += OnAllTasksCompleted;
        }

        public void CheckWinConditions()
        {
            if (_gameEnded)
                return;

            var winCondition = EvaluateWinCondition();
            
            if (winCondition != WinCondition.None)
            {
                EndGame(winCondition);
            }
        }

        private WinCondition EvaluateWinCondition()
        {
            int aliveMafia = _characterLifecycleService.GetAliveCount(Role.Mafia);
            int alivePolice = _characterLifecycleService.GetAliveCount(Role.Police);
            int aliveCivilians = _characterLifecycleService.GetAliveCount(Role.Civilian);
            int aliveTotal = aliveMafia + alivePolice + aliveCivilians;
            int aliveNonMafia = alivePolice + aliveCivilians;

            string status = $"Alive - Mafia: {aliveMafia}, Police: {alivePolice}, Civilians: {aliveCivilians}";
            WinConditionChecked?.Invoke(WinCondition.None, status);

            // 1. Мафия победила: мафов >= остальных игроков или все не-мафы мертвы
            if (aliveMafia > 0 && (aliveMafia >= aliveNonMafia || aliveNonMafia == 0))
            {
                return WinCondition.MafiaWins;
            }

            // 2. Мафия полностью уничтожена
            if (aliveMafia == 0 && aliveNonMafia > 0)
            {
                return WinCondition.PoliceWins;
            }

            // 3. Все задачи выполнены
            if (_taskProgressService.AreAllTasksCompleted())
            {
                return WinCondition.TasksCompleted;
            }

            // 4. Все игроки мертвы (ничья, но пусть будет победа мафии)
            if (aliveTotal == 0)
            {
                return WinCondition.MafiaWins;
            }

            return WinCondition.None;
        }

        private void EndGame(WinCondition winCondition)
        {
            _gameEnded = true;
            _currentWinCondition = winCondition;

            string message = GetWinMessage(winCondition);
            Debug.Log($"[GameWinConditionService] {message}");

            GameWon?.Invoke(winCondition);
        }

        private string GetWinMessage(WinCondition winCondition)
        {
            return winCondition switch
            {
                WinCondition.MafiaWins => "🔴 МАФИЯ ПОБЕДИЛА! Зло восторжествовало!",
                WinCondition.PoliceWins => "🔵 ПОЛИЦИЯ ПОБЕДИЛА! Справедливость восторжествовала!",
                WinCondition.CiviliansWin => "🟢 МИРНЫЕ ПОБЕДИЛИ! Добро восторжествовало!",
                WinCondition.TasksCompleted => "⚙️ ВСЕ ЗАДАЧИ ВЫПОЛНЕНЫ! Мирные жители победили!",
                _ => "🎮 Игра окончена"
            };
        }

        private void OnCharacterKilled(CharacterView character)
        {
            Debug.Log($"[GameWinConditionService] Character killed: {character.Character.Role}");
            CheckWinConditions();
        }

        private void OnRoleCountChanged(Role role, int aliveCount)
        {
            Debug.Log($"[GameWinConditionService] Role count changed: {role} = {aliveCount}");
            CheckWinConditions();
        }

        private void OnAllTasksCompleted()
        {
            Debug.Log("[GameWinConditionService] All tasks completed!");
            CheckWinConditions();
        }

        public void ResetGame()
        {
            _gameEnded = false;
            _currentWinCondition = WinCondition.None;
            Debug.Log("[GameWinConditionService] Game reset");
        }

        // Методы для принудительной победы (для тестирования)
        public void ForceWin(WinCondition winCondition)
        {
            Debug.Log($"[GameWinConditionService] Forced win: {winCondition}");
            EndGame(winCondition);
        }

        // Получение текущего статуса игры
        public string GetGameStatus()
        {
            if (_gameEnded)
            {
                return GetWinMessage(_currentWinCondition);
            }

            int aliveMafia = _characterLifecycleService.GetAliveCount(Role.Mafia);
            int alivePolice = _characterLifecycleService.GetAliveCount(Role.Police);
            int aliveCivilians = _characterLifecycleService.GetAliveCount(Role.Civilian);
            int tasksProgress = _taskProgressService.OverallProgress;

            return $"👥 Мафия: {aliveMafia} | 👮 Полиция: {alivePolice} | 👤 Мирные: {aliveCivilians} | ⚙️ Задачи: {tasksProgress}%";
        }

        public void Dispose()
        {
            // Отписываемся от событий
            if (_characterLifecycleService != null)
            {
                _characterLifecycleService.CharacterKilled -= OnCharacterKilled;
                _characterLifecycleService.RoleCountChanged -= OnRoleCountChanged;
            }

            if (_taskProgressService != null)
            {
                _taskProgressService.AllTasksCompleted -= OnAllTasksCompleted;
            }
        }
    }
} 