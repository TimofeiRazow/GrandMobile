using Codebase.Controllers;
using Codebase.Domain.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Codebase.Infrastructure.SceneRoots
{
    public class GameplaySceneRoot : MonoBehaviour
    {
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private GameModeConfig _gameModeConfig;
        [SerializeField] private CinemachineCamera _camera;

        private CoreLoop _coreLoop;

        [Inject]
        private void Construct(CoreLoop coreLoop)
        {
            _coreLoop = coreLoop;
        }

        private void Start()
        {
            Debug.Log("[GameplaySceneRoot] Starting gameplay scene...");
            _coreLoop.Start(_gameModeConfig, _spawnPoints, _camera);
        }

        private void Update()
        {
            _coreLoop.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Debug.Log("[GameplaySceneRoot] Destroying gameplay scene...");
            _coreLoop.Dispose();
        }
    }
}