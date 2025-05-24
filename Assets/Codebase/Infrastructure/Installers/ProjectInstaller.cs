using Codebase.Configs;
using Codebase.Controllers;
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
            
            // Основные игровые сервисы
            Container.Bind<ActionObjectProvider>().AsSingle();
            Container.Bind<GamePhaseService>().AsSingle();
            Container.Bind<CharacterLifecycleService>().AsSingle();
            Container.Bind<TaskProgressService>().AsSingle();
            Container.Bind<GameWinConditionService>().AsSingle();

            // Основной игровой цикл и фабрики
            Container.Bind<CoreLoop>().AsTransient();
            Container.Bind<CharacterViewFactory>().AsSingle();
            Container.Bind<CameraService>().AsSingle();
            
            // Сервисы ввода
            Container.Bind<PlayerInputService>().AsTransient();
            Container.Bind<AiInputService>().AsTransient();
        }
    }
}