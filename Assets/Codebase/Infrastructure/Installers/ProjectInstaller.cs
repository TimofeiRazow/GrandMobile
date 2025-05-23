using Codebase.Configs;
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
        }
    }
}