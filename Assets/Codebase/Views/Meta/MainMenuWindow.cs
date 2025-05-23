using Codebase.Infrastructure.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Codebase.Views.Meta
{
    public class MainMenuWindow : MonoBehaviour
    {
        [SerializeField] private Button _startGame;

        private SceneService _sceneService;

        [Inject]
        private void Construct(SceneService sceneService)
        {
            _sceneService = sceneService;
        }

        private void Start()
        {
            _startGame.onClick.AddListener(OnStartGameButtonClicked);
        }

        private void OnDestroy()
        {
            _startGame.onClick.RemoveAllListeners();
        }

        private void OnStartGameButtonClicked()
        {
            _sceneService.ToGameplay();
        }
    }
}