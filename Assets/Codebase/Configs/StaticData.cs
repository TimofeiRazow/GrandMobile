using System.Collections.Generic;
using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Configs
{
    [CreateAssetMenu(menuName = "Data/Create StaticData", fileName = "StaticData", order = 0)]
    public class StaticData : ScriptableObject
    {
        [field: SerializeField] public string BootstrapSceneName { get; set; }
        
        [field: SerializeField] public string MenuSceneName { get; set; }
        
        [field: SerializeField] public string GameplaySceneName { get; set; }
        [field: SerializeField] public List<CharacterView> CharacterViews { get; set; }
    }
}