using System;
using System.Collections.Generic;
using Codebase.Configs;
using Codebase.Controllers.Fsm;
using Codebase.Controllers.Fsm.States;
using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using Codebase.Views.Gameplay;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Codebase.Infrastructure.Factories
{
    public class CharacterViewFactory
    {
        private readonly DiContainer _diContainer;
        private readonly StaticData _staticData;

        public CharacterViewFactory(DiContainer diContainer, StaticData staticData)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException(nameof(diContainer));
            _staticData = staticData ?? throw new ArgumentNullException(nameof(staticData));
        }

        public CharacterView CreatePlayer(Transform spawnPoint, Role role)
        {
            var viewPrefab = _staticData.CharacterViews[Random.Range(0, _staticData.CharacterViews.Count)];
            var characterView = Object.Instantiate(viewPrefab, spawnPoint.position, spawnPoint.rotation);

            var model = new Character { Role = role };
            var input = _diContainer.Resolve<PlayerInputService>();

            var fsm = new PlayerFsm(CreateStates(characterView));

            characterView.Initialize(model, input, fsm);

            return characterView;
        }

        public CharacterView CreateBot(Transform spawnPoint, Role role)
        {
            var viewPrefab = _staticData.CharacterViews[Random.Range(0, _staticData.CharacterViews.Count)];
            var characterView = Object.Instantiate(viewPrefab, spawnPoint.position, spawnPoint.rotation);

            var model = new Character { Role = role };
            var input = _diContainer.Resolve<AiInputService>();

            input.Initialize(model, characterView.transform);

            var fsm = new BotFsm(CreateStates(characterView));

            characterView.Initialize(model, input, fsm);

            return characterView;
        }

        private List<IState> CreateStates(CharacterView characterView)
        {
            return new List<IState>()
            {
                new IdleState(characterView),
                new MoveState(characterView),
                new ActionState(characterView),
                new DeathState(characterView),
            };
        }
    }
}