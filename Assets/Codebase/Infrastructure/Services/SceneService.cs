using Codebase.Configs;
using UnityEngine.SceneManagement;

namespace Codebase.Infrastructure.Services
{
    public class SceneService
    {
        private readonly StaticData _staticData;

        public SceneService(StaticData staticData)
        {
            _staticData = staticData;
        }

        public void ToMainMenu()
        {
            SceneManager.LoadSceneAsync(_staticData.MenuSceneName, LoadSceneMode.Single);
        }
        
        public void ToGameplay()
        {
            SceneManager.LoadSceneAsync(_staticData.GameplaySceneName, LoadSceneMode.Single);
        }
    }
}