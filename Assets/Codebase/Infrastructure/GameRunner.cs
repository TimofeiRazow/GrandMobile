using UnityEngine;

namespace Codebase.Infrastructure
{
    public class GameRunner : MonoBehaviour
    {
        private Game _game;
        
        private void Start()
        {
            _game.Initialize();
        }

        private void Update()
        {
            _game.Update(Time.deltaTime);
        }
    }
}