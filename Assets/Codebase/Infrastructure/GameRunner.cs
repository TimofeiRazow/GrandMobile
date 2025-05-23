using Codebase.Infrastructure.Services;
using UnityEngine;
using Zenject;

namespace Codebase.Infrastructure
{
    public class GameRunner : MonoBehaviour
    {
        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
        }

        private void Start()
        {
            _sceneService.ToMainMenu();
        }
    }
}