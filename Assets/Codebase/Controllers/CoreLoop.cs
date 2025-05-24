using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Domain.Gameplay;
using Codebase.Infrastructure.Factories;
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

        private readonly List<CharacterView> _botViews = new();

        private GameModeConfig _config;
        private Transform[] _spawnPoints;

        private CharacterView _playerView;

        public CoreLoop(CharacterViewFactory characterViewFactory, CameraService cameraService)
        {
            _characterViewFactory =
                characterViewFactory ?? throw new ArgumentNullException(nameof(characterViewFactory));
            _cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
        }

        public void Start(GameModeConfig config, Transform[] spawnPoints, CinemachineCamera camera)
        {
            _spawnPoints = spawnPoints ?? throw new ArgumentNullException(nameof(spawnPoints));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            PrepareCharacters(config);

            _cameraService.BindCamera(camera);
            _cameraService.BindToTarget(_playerView.transform);
        }

        private void PrepareCharacters(GameModeConfig config)
        {
            (var playerRole, var botRoles) = DistributeRoles(config);

            var shuffledPoints = _spawnPoints
                .OrderBy(x => new Random().Next())
                .ToList();

            _playerView = _characterViewFactory.CreatePlayer(shuffledPoints.First(), playerRole);

            _botViews.Clear();

            for (int i = 0; i < botRoles.Count; i++)
            {
                var bot = _characterViewFactory.CreateBot(shuffledPoints[i + 1], botRoles[i]);
                _botViews.Add(bot);
            }
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

        public void Dispose()
        {
            _cameraService.Unbind();
        }

        public void Update(float deltaTime)
        {
        }
    }
}