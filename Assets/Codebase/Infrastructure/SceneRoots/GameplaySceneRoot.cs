using UnityEngine;
using Zenject;

namespace Codebase.Infrastructure.SceneRoots
{
    public class GameplaySceneRoot : MonoBehaviour
    {
        [Inject]
        private void Construct()
        {
        }
    }
}