using Codebase.Controllers.Fsm;
using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using TMPro;
using UnityEngine;

namespace Codebase.Views.Gameplay
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private TMP_Text _uiBar;
        [SerializeField] private Renderer _body;

        private Character _character;
        private IInputService _inputService;
        private FsmBase _fsm;

        public void Initialize(Character character, IInputService inputService, FsmBase fsm)
        {
            _character = character;
            _inputService = inputService;
            _fsm = fsm;

            _fsm.Reset();
        }

        private void Update()
        {
            _fsm.Update(Time.deltaTime);
        }
    }
}