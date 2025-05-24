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
        [SerializeField] private CharacterController _characterController;

        private Character _character;
        private IInputService _inputService;
        private FsmBase _fsm;

        public Transform Transform => transform;
        public Animator Animator => _animator;
        public AudioSource AudioSource => _audioSource;
        public TMP_Text UiBar => _uiBar;
        public Renderer Body => _body;
        public Character Character => _character;
        public IInputService InputService => _inputService;
        public CharacterController CharacterController => _characterController;

        public void Initialize(Character character, IInputService inputService, FsmBase fsm)
        {
            _character = character;
            _inputService = inputService;
            _fsm = fsm;

            _fsm.Reset();
        }

        private void Update()
        {
            if (_inputService is AiInputService aiService) 
                aiService.Update(Time.deltaTime);
            
            _fsm.Update(Time.deltaTime);
        }
    }
}