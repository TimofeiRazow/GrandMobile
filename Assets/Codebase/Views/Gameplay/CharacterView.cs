using Codebase.Controllers.Fsm;
using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Codebase.Views.Gameplay
{
    public class CharacterView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private TMP_Text _uiBar;
        [SerializeField] private Renderer _body;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private NavMeshAgent _navMeshAgent;

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
        public NavMeshAgent NavMeshAgent => _navMeshAgent;

        private void Awake()
        {
            // Если CharacterController не назначен в инспекторе, добавляем его
            if (_characterController == null)
            {
                _characterController = GetComponent<CharacterController>();
                if (_characterController == null)
                {
                    _characterController = gameObject.AddComponent<CharacterController>();
                    // Настройки по умолчанию для CharacterController
                    _characterController.radius = 0.5f;
                    _characterController.height = 2f;
                    _characterController.center = new Vector3(0, 1f, 0);
                }
            }

            // Проверяем NavMeshAgent
            if (_navMeshAgent == null)
            {
                _navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        public void Initialize(Character character, IInputService inputService, FsmBase fsm)
        {
            _character = character;
            _inputService = inputService;
            _fsm = fsm;

            // Настраиваем компоненты в зависимости от типа управления
            ConfigureMovementComponents();

            _fsm.Initialize();
            _fsm.Reset();
        }

        private void ConfigureMovementComponents()
        {
            bool isBot = _inputService is AiInputService;

            if (isBot)
            {
                // Для ботов используем NavMeshAgent
                if (_characterController != null)
                {
                    _characterController.enabled = false;
                }

                if (_navMeshAgent == null)
                {
                    _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
                }

                if (_navMeshAgent != null)
                {
                    _navMeshAgent.enabled = true;
                    
                    // Настройки NavMeshAgent
                    _navMeshAgent.height = 2f;
                    _navMeshAgent.radius = 0.5f;
                    _navMeshAgent.speed = 3.5f;
                    _navMeshAgent.acceleration = 8f;
                    _navMeshAgent.angularSpeed = 120f;
                    _navMeshAgent.stoppingDistance = 0.1f;
                    _navMeshAgent.autoBraking = true;
                    
                    Debug.Log($"[CharacterView] Configured NavMeshAgent for bot {_character.Role}");
                }
            }
            else
            {
                // Для игрока используем CharacterController
                if (_navMeshAgent != null)
                {
                    _navMeshAgent.enabled = false;
                }

                if (_characterController != null)
                {
                    _characterController.enabled = true;
                    Debug.Log($"[CharacterView] Configured CharacterController for player {_character.Role}");
                }
            }
        }

        private void Update()
        {
            _fsm.Update(Time.deltaTime);
        }

        // Методы для переключения между режимами движения
        public void SwitchToCharacterController()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                // Сохраняем позицию
                Vector3 position = transform.position;
                
                _navMeshAgent.enabled = false;
                
                if (_characterController != null)
                {
                    _characterController.enabled = true;
                    // Восстанавливаем позицию (NavMeshAgent может немного сдвинуть)
                    transform.position = position;
                }
            }
        }

        public void SwitchToNavMeshAgent()
        {
            if (_characterController != null && _characterController.enabled)
            {
                _characterController.enabled = false;
                
                if (_navMeshAgent != null)
                {
                    _navMeshAgent.enabled = true;
                }
            }
        }

        // Метод для получения текущей позиции независимо от типа движения
        public Vector3 GetGroundPosition()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                return _navMeshAgent.transform.position;
            }

            if (_characterController != null && _characterController.enabled)
            {
                return _characterController.transform.position;
            }

            return transform.position;
        }

        // Проверяем, на земле ли персонаж (для любого типа движения)
        public bool IsGrounded()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                return _navMeshAgent.isOnNavMesh;
            }

            if (_characterController != null && _characterController.enabled)
            {
                return _characterController.isGrounded;
            }

            return true; // По умолчанию считаем что на земле
        }

        // Метод для получения скорости движения
        public float GetMovementSpeed()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                return _navMeshAgent.velocity.magnitude;
            }

            if (_characterController != null && _characterController.enabled)
            {
                return _characterController.velocity.magnitude;
            }

            return 0f;
        }
    }
}