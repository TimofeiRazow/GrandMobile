using Codebase.Configs;
using Codebase.Controllers;
using Codebase.Controllers.Fsm.States;
using Codebase.Controllers.Input;
using Codebase.Infrastructure.Factories;
using Codebase.Infrastructure.Services;
using UnityEngine;
using Zenject;

namespace Codebase.Infrastructure.Installers
{
    [CreateAssetMenu(menuName = "Data/Create ProjectInstaller", fileName = "ProjectInstaller", order = 0)]
    public class ProjectInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private StaticData _staticData;

        public override void InstallBindings()
        {
            Container.Bind<StaticData>().FromInstance(_staticData).AsSingle();
            Container.Bind<SceneService>().AsSingle();

            Container.Bind<CoreLoop>().AsTransient();
            Container.Bind<CharacterViewFactory>().AsSingle();
            Container.Bind<CameraService>().AsSingle();
            
            Container.Bind<PlayerInputService>().AsTransient();
            Container.Bind<AiInputService>().AsTransient();
            
            Container.Bind<IdleState>().AsTransient();
            Container.Bind<MoveState>().AsTransient();
            Container.Bind<ActionState>().AsTransient();
            Container.Bind<DeathState>().AsTransient();
        }
    }
}