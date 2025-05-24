using System;
using System.Collections.Generic;
using Codebase.Configs;
using Codebase.Controllers.Fsm;
using Codebase.Controllers.Fsm.States;
using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using Codebase.Infrastructure.Services;
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
        private readonly ActionObjectProvider _actionObjectProvider;

        public CharacterViewFactory(DiContainer diContainer, StaticData staticData, ActionObjectProvider actionObjectProvider)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException(nameof(diContainer));
            _staticData = staticData ?? throw new ArgumentNullException(nameof(staticData));
            _actionObjectProvider = actionObjectProvider ?? throw new ArgumentNullException(nameof(actionObjectProvider));
        }

        public CharacterView CreatePlayer(Transform spawnPoint, Role role)
        {
            var viewPrefab = _staticData.CharacterViews[Random.Range(0, _staticData.CharacterViews.Count)];
            var characterView = Object.Instantiate(viewPrefab, spawnPoint.position, spawnPoint.rotation);

            var model = new Character { Role = role };
            SetupRoleAction(model);
            
            var input = _diContainer.Resolve<PlayerInputService>();

            var fsm = new PlayerFsm(CreatePlayerStates(characterView));

            characterView.Initialize(model, input, fsm);

            return characterView;
        }

        public CharacterView CreateBot(Transform spawnPoint, Role role)
        {
            var viewPrefab = _staticData.CharacterViews[Random.Range(0, _staticData.CharacterViews.Count)];
            var characterView = Object.Instantiate(viewPrefab, spawnPoint.position, spawnPoint.rotation);

            var model = new Character { Role = role };
            SetupRoleAction(model);
            
            var input = _diContainer.Resolve<AiInputService>();

            input.Initialize(model, characterView.transform);

            var fsm = new BotFsm(CreateBotStates(characterView));

            characterView.Initialize(model, input, fsm);

            return characterView;
        }

        private void SetupRoleAction(Character character)
        {
            RoleAction roleAction = character.Role switch
            {
                Role.Mafia => new MafiaAction(),
                Role.Police => new PoliceAction(),
                Role.Civilian => new CivilianAction(),
                _ => null
            };

            if (roleAction != null)
            {
                character.SetRoleAction(roleAction);
                Debug.Log($"[CharacterViewFactory] Установлено RoleAction для {character.Role}");
            }
        }

        private List<IState> CreatePlayerStates(CharacterView characterView)
        {
            return new List<IState>()
            {
                new IdleState(characterView),
                new MoveState(characterView),
                new ActionState(characterView),
                new DeathState(characterView),
            };
        }

        private List<IState> CreateBotStates(CharacterView characterView)
        {
            return new List<IState>()
            {
                new AiIdleState(characterView, _actionObjectProvider),
                new AiMoveState(characterView),
                new AiActionState(characterView),
                new DeathState(characterView),
            };
        }
    }
}