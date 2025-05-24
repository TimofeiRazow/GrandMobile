using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Domain.Gameplay;
using Codebase.Infrastructure.Factories;
using Codebase.Infrastructure.Services;
using Codebase.Views.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using Random = System.Random;

namespace Codebase.Controllers
{
    public class CoreLoop : IDisposable
    {
        private readonly CharacterViewFactory _characterViewFactory;
        private readonly CameraService _cameraService;
        private readonly GamePhaseService _gamePhaseService;
        private readonly CharacterLifecycleService _characterLifecycleService;
        private readonly TaskProgressService _taskProgressService;
        private readonly GameWinConditionService _gameWinConditionService;

        private readonly List<CharacterView> _botViews = new();

        private GameModeConfig _config;
        private Transform[] _spawnPoints;
        private CharacterView _playerView;
        private bool _isGameStarted;

        public CoreLoop(
            CharacterViewFactory characterViewFactory,
            CameraService cameraService,
            GamePhaseService gamePhaseService,
            CharacterLifecycleService characterLifecycleService,
            TaskProgressService taskProgressService,
            GameWinConditionService gameWinConditionService)
        {
            _characterViewFactory =
                characterViewFactory ?? throw new ArgumentNullException(nameof(characterViewFactory));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
            _gamePhaseService = gamePhaseService ?? throw new ArgumentNullException(nameof(gamePhaseService));
            _characterLifecycleService = characterLifecycleService ??
                                         throw new ArgumentNullException(nameof(characterLifecycleService));
            _taskProgressService = taskProgressService ?? throw new ArgumentNullException(nameof(taskProgressService));
            _gameWinConditionService = gameWinConditionService ??
                                       throw new ArgumentNullException(nameof(gameWinConditionService));
        }

        public bool IsGameStarted => _isGameStarted;
        public CharacterView PlayerView => _playerView;
        public IReadOnlyList<CharacterView> BotViews => _botViews.AsReadOnly();
        public string GetGameStatus() => _gameWinConditionService.GetGameStatus();

        public void Start(GameModeConfig config, Transform[] spawnPoints, CinemachineCamera camera)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            _spawnPoints = spawnPoints ?? throw new ArgumentNullException(nameof(spawnPoints));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            Debug.Log("[CoreLoop] Starting game...");

            // Подготавливаем персонажей
            PrepareCharacters(config);

            // Настраиваем камеру
            _cameraService.BindCamera(camera);
            _cameraService.BindToTarget(_playerView.transform);

            // Обнаруживаем и регистрируем задачи
            _taskProgressService.DiscoverTasks();

            // Регистрируем всех персонажей в системе
            RegisterCharacters();

            SubscribeToServiceEvents();
            
            // Запускаем игровую фазу
            _gamePhaseService.StartDay();

            _isGameStarted = true;
            Debug.Log("[CoreLoop] Game started successfully!");
        }

        public void Update(float deltaTime)
        {
            if (!_isGameStarted)
                return;

            // Обновляем все сервисы
            _gamePhaseService.Update(deltaTime);

            // Проверяем условия победы
            _gameWinConditionService.CheckWinConditions();
        }

        public void RestartGame()
        {
            Debug.Log("[CoreLoop] Restarting game...");

            // Сбрасываем сервисы
            _gameWinConditionService.ResetGame();
            _characterLifecycleService.Clear();
            _taskProgressService.Clear();

            // Перезапускаем игру
            _isGameStarted = false;
            Start(_config, _spawnPoints, _cameraService.GetCurrentCamera());
        }

        public void PauseGame()
        {
            _isGameStarted = false;
            Debug.Log("[CoreLoop] Game paused");
        }

        public void ResumeGame()
        {
            _isGameStarted = true;
            Debug.Log("[CoreLoop] Game resumed");
        }

        public void Dispose()
        {
            Debug.Log("[CoreLoop] Disposing...");

            UnsubscribeFromServiceEvents();
            _cameraService.Unbind();
            _characterLifecycleService.Clear();
            _taskProgressService.Clear();
            _gameWinConditionService.Dispose();

            _isGameStarted = false;
        }

        private void PrepareCharacters(GameModeConfig config)
        {
            (var playerRole, var botRoles) = DistributeRoles(config);

            var shuffledPoints = _spawnPoints
                .OrderBy(x => new Random().Next())
                .ToList();

            // Создаем игрока
            _playerView = _characterViewFactory.CreatePlayer(shuffledPoints.First(), playerRole);
            Debug.Log($"[CoreLoop] Created player: {playerRole}");

            // Создаем ботов
            _botViews.Clear();
            for (int i = 0; i < botRoles.Count; i++)
            {
                var bot = _characterViewFactory.CreateBot(shuffledPoints[i + 1], botRoles[i]);
                _botViews.Add(bot);
                Debug.Log($"[CoreLoop] Created bot {i + 1}: {botRoles[i]}");
            }
        }

        private void RegisterCharacters()
        {
            // Регистрируем игрока
            _characterLifecycleService.RegisterCharacter(_playerView);

            // Регистрируем ботов
            foreach (var bot in _botViews) 
                _characterLifecycleService.RegisterCharacter(bot);

            Debug.Log($"[CoreLoop] Registered {_botViews.Count + 1} characters");
            _characterLifecycleService.LogStatistics();
        }

        private (Role, List<Role>) DistributeRoles(GameModeConfig config)
        {
            var allRoles = new List<Role>();

            allRoles.AddRange(Enumerable.Repeat(Role.Mafia, config.MafiaCount));
            allRoles.AddRange(Enumerable.Repeat(Role.Police, config.PoliceCount));
            allRoles.AddRange(Enumerable.Repeat(Role.Civilian, config.CivilianCount));

            allRoles = allRoles.OrderBy(x => new Random().Next()).ToList();

            var player = allRoles.Take(1).First();
            var bots = allRoles.Skip(1).ToList();

            return (player, bots);
        }

        private void SubscribeToServiceEvents()
        {
            // События фаз игры
            _gamePhaseService.PhaseChanged += OnPhaseChanged;
            _gamePhaseService.GameMessage += OnGameMessage;

            // События персонажей
            _characterLifecycleService.CharacterKilled += OnCharacterKilled;
            _characterLifecycleService.RoleCountChanged += OnRoleCountChanged;

            // События задач
            _taskProgressService.TaskCompleted += OnTaskCompleted;
            _taskProgressService.OverallProgressChanged += OnTaskProgressChanged;

            // События условий победы
            _gameWinConditionService.GameWon += OnGameWon;
        }

        private void UnsubscribeFromServiceEvents()
        {
            // Отписываемся от событий фаз игры
            _gamePhaseService.PhaseChanged -= OnPhaseChanged;
            _gamePhaseService.GameMessage -= OnGameMessage;

            // Отписываемся от событий персонажей
            _characterLifecycleService.CharacterKilled -= OnCharacterKilled;
            _characterLifecycleService.RoleCountChanged -= OnRoleCountChanged;

            // Отписываемся от событий задач
            _taskProgressService.TaskCompleted -= OnTaskCompleted;
            _taskProgressService.OverallProgressChanged -= OnTaskProgressChanged;

            // Отписываемся от событий условий победы
            _gameWinConditionService.GameWon -= OnGameWon;
        }

        // Обработчики событий
        private void OnPhaseChanged(GamePhase phase)
        {
            Debug.Log($"[CoreLoop] Phase changed to: {phase}");
            // Здесь можно добавить логику реакции на смену фазы
        }

        private void OnGameMessage(string message)
        {
            Debug.Log($"[CoreLoop] Game message: {message}");
            // Здесь можно отправить сообщение в UI
        }

        private void OnCharacterKilled(CharacterView character)
        {
            Debug.Log($"[CoreLoop] Character killed: {character.Character.Role}");
            // Здесь можно добавить эффекты смерти
        }

        private void OnRoleCountChanged(Role role, int aliveCount)
        {
            Debug.Log($"[CoreLoop] Role count changed: {role} = {aliveCount}");
        }

        private void OnTaskCompleted(TaskObject task)
        {
            Debug.Log($"[CoreLoop] Task completed: {task.TaskName}");
        }

        private void OnTaskProgressChanged(int overallProgress)
        {
            Debug.Log($"[CoreLoop] Overall task progress: {overallProgress}%");
        }

        private void OnGameWon(WinCondition winCondition)
        {
            Debug.Log($"[CoreLoop] Game won: {winCondition}");
            _isGameStarted = false;

            // Здесь можно запустить экран победы
            string status = _gameWinConditionService.GetGameStatus();
            Debug.Log($"[CoreLoop] Final status: {status}");
        }
    }
}