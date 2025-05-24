using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using Codebase.Views.Gameplay;
using UnityEngine;
using UnityEngine.AI;

namespace Codebase.Controllers.Fsm.States
{
    public class AiMoveState : IState
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        private readonly CharacterView _characterView;

        private IStateSwitcher _stateSwitcher;
        private AiInputService _aiInputService;
        private NavMeshAgent _navMeshAgent;

        // Навигация
        private ActionObject _currentTarget;
        private float _destinationReachedThreshold = 1f;
        private float _lookUpdateTimer;
        private readonly float _lookUpdateInterval = 0.5f;

        public AiMoveState(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Initialize(IStateSwitcher stateSwitcher)
        {
            _stateSwitcher = stateSwitcher;
            _aiInputService = _characterView.InputService as AiInputService;
            _navMeshAgent = _characterView.NavMeshAgent;
        }

        public void Enter()
        {
            Debug.Log($"[AiMoveState] Entering for {_characterView.Character.Role}");

            // Получаем текущую цель от AiIdleState
            if (_stateSwitcher is FsmBase fsm)
            {
                var idleState = fsm.GetState<AiIdleState>();
                if (idleState != null)
                {
                    _currentTarget = idleState.GetCurrentTarget();
                }
            }

            _lookUpdateTimer = 0f;
        }

        public void Exit()
        {
            Debug.Log($"[AiMoveState] Exiting for {_characterView.Character.Role}");
            _aiInputService?.ResetInputs();
        }

        public void Update(float deltaTime)
        {
            UpdateNavigation(deltaTime);
            UpdateMovementInput();
            UpdateLook(deltaTime);
            UpdateAnimator();
            CheckStateTransitions();
        }

        private void UpdateNavigation(float deltaTime)
        {
            if (_navMeshAgent == null)
                return;

            // Проверяем, достигли ли цели
            if (_currentTarget != null)
            {
                float distanceToTarget = Vector3.Distance(_characterView.Transform.position, _currentTarget.transform.position);

                if (distanceToTarget <= _currentTarget.InteractionRange)
                {
                    // Достигли объекта для взаимодействия
                    Debug.Log($"[AiMoveState-{_characterView.Character.Role}] Достиг цели: {_currentTarget.name}");

                    // Останавливаемся
                    _navMeshAgent.ResetPath();
                    _stateSwitcher.Switch<AiActionState>();
                    return;
                }

                // Настраиваем скорость в зависимости от расстояния
                bool shouldSprint = distanceToTarget > 5f;
                _aiInputService?.SetSprintPressed(shouldSprint);
                
                if (shouldSprint)
                {
                    _navMeshAgent.speed = 5.25f; // 1.5x базовой скорости
                }
                else
                {
                    _navMeshAgent.speed = 3.5f; // Базовая скорость
                }
            }
            else if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance < _destinationReachedThreshold)
            {
                // Достигли случайной точки, возвращаемся к поиску
                Debug.Log($"[AiMoveState-{_characterView.Character.Role}] Достиг случайной точки");
                _stateSwitcher.Switch<AiIdleState>();
                return;
            }
        }

        private void UpdateMovementInput()
        {
            if (_navMeshAgent == null || !_navMeshAgent.hasPath)
            {
                _aiInputService?.SetMovementInput(Vector2.zero);
                return;
            }

            // Преобразуем направление NavMeshAgent в input
            Vector3 desiredVelocity = _navMeshAgent.desiredVelocity.normalized;

            // Преобразуем в локальные координаты персонажа
            Vector3 localDesiredVelocity = _characterView.Transform.InverseTransformDirection(desiredVelocity);

            Vector2 movementInput = new Vector2(localDesiredVelocity.x, localDesiredVelocity.z);

            // Ограничиваем величину
            if (movementInput.magnitude > 1f)
            {
                movementInput = movementInput.normalized;
            }

            _aiInputService?.SetMovementInput(movementInput);
        }

        private void UpdateLook(float deltaTime)
        {
            _lookUpdateTimer -= deltaTime;

            if (_lookUpdateTimer <= 0f)
            {
                _lookUpdateTimer = _lookUpdateInterval;

                // Смотрим в сторону цели или движения
                Vector3 lookDirection = Vector3.zero;

                if (_currentTarget != null)
                {
                    // Смотрим в сторону цели
                    lookDirection = (_currentTarget.transform.position - _characterView.Transform.position).normalized;
                }
                else if (_navMeshAgent != null && _navMeshAgent.hasPath)
                {
                    // Смотрим в сторону движения
                    lookDirection = _navMeshAgent.desiredVelocity.normalized;
                }

                if (lookDirection != Vector3.zero)
                {
                    float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
                    float currentAngle = _characterView.Transform.eulerAngles.y;
                    float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

                    // Плавный поворот
                    Vector2 lookInput = new Vector2(angleDifference * 0.1f, 0f);
                    _aiInputService?.SetLookInput(lookInput);
                }
            }
        }

        private void UpdateAnimator()
        {
            if (_characterView.Animator != null)
            {
                float currentSpeed = _characterView.GetMovementSpeed();
                float normalizedSpeed = currentSpeed / 5f; // Нормализуем к максимальной скорости
                
                bool isSprinting = _aiInputService?.IsSprintPressed() ?? false;
                
                _characterView.Animator.SetFloat(Speed, normalizedSpeed);
                _characterView.Animator.SetBool(IsRunning, isSprinting);
                _characterView.Animator.SetBool(IsMoving, currentSpeed > 0.1f);
                _characterView.Animator.SetBool(IsGrounded, _characterView.IsGrounded());
            }
        }

        private void CheckStateTransitions()
        {
            // Останавливаемся если NavMeshAgent не движется
            if (_navMeshAgent == null || !_navMeshAgent.hasPath)
            {
                _stateSwitcher.Switch<AiIdleState>();
                return;
            }

            // Переходим к действию если рядом с целью
            if (_currentTarget != null)
            {
                float distance = Vector3.Distance(_characterView.Transform.position, _currentTarget.transform.position);
                if (distance <= _currentTarget.InteractionRange)
                {
                    _stateSwitcher.Switch<AiActionState>();
                }
            }
        }

        // Методы для получения состояния
        public ActionObject GetCurrentTarget() => _currentTarget;
        public bool HasTarget => _currentTarget != null;

        public void SetTarget(ActionObject target)
        {
            _currentTarget = target;
        }
    }
} 