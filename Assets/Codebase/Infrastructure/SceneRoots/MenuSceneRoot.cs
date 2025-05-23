using UnityEngine;
using Zenject;

namespace Codebase.Infrastructure.SceneRoots
{
    public class MenuSceneRoot : MonoBehaviour
    {
        [Inject]
        private void Construct()
        {
        }
    }
}