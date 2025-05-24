using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using Codebase.Infrastructure.Services;
using Codebase.Views.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Codebase.Controllers.Fsm.States
{
    public class AiIdleState : IState
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        private readonly CharacterView _characterView;
        private readonly ActionObjectProvider _actionObjectProvider;
        private GamePhaseService _gamePhaseService;
        private CharacterLifecycleService _characterLifecycleService;

        private IStateSwitcher _stateSwitcher;
        private AiInputService _aiInputService;
        private NavMeshAgent _navMeshAgent;

        // Поиск целей
        private ActionObject _currentTarget;
        private TaskObject _currentTask;
        private float _taskSearchCooldown;
        private float _taskSearchInterval = 5f;

        // Параметры поведения
        private float _actionTimer;
        private float _actionInterval = 5f;
        private float _lookTimer;
        private readonly float _lookInterval = 2f;

        // Модификаторы поведения в зависимости от фазы
        private bool _isNightPhase = false;
        private bool _isVotingPhase = false;

        public AiIdleState(CharacterView characterView, ActionObjectProvider actionObjectProvider)
        {
            _characterView = characterView;
            _actionObjectProvider = actionObjectProvider;
        }

        public void Initialize(IStateSwitcher stateSwitcher)
        {
            _stateSwitcher = stateSwitcher;
            _aiInputService = _characterView.InputService as AiInputService;
            _navMeshAgent = _characterView.NavMeshAgent;
            
            // Получаем сервисы через DI
            _gamePhaseService = ProjectContext.Instance.Container.Resolve<GamePhaseService>();
            _characterLifecycleService = ProjectContext.Instance.Container.Resolve<CharacterLifecycleService>();
            
            SetActionIntervalByRole();
            SubscribeToEvents();
        }

        public void Enter()
        {
            Debug.Log($"[AiIdleState] Entering for {_characterView.Character.Role}");

            _aiInputService?.ResetInputs();
            
            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetFloat(Speed, 0f);
                _characterView.Animator.SetBool(IsRunning, false);
                _characterView.Animator.SetBool(IsMoving, false);
            }

            // Сбрасываем таймеры
            _taskSearchCooldown = 0f;
            _actionTimer = Random.Range(0f, _actionInterval);
            _lookTimer = Random.Range(0f, _lookInterval);
        }

        public void Exit()
        {
            Debug.Log($"[AiIdleState] Exiting for {_characterView.Character.Role}");
        }

        public void Update(float deltaTime)
        {
            UpdateTaskSearch(deltaTime);
            UpdateLook(deltaTime);
            UpdateActionTimer(deltaTime);
            UpdateAnimator();
            CheckStateTransitions();
        }

        private void SubscribeToEvents()
        {
            if (_gamePhaseService != null)
            {
                _gamePhaseService.PhaseChanged += OnPhaseChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_gamePhaseService != null)
            {
                _gamePhaseService.PhaseChanged -= OnPhaseChanged;
            }
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            _isNightPhase = phase == GamePhase.Night;
            _isVotingPhase = phase == GamePhase.Voting;
            
            Debug.Log($"[AiIdleState-{_characterView.Character.Role}] Phase changed to {phase}");
            
            // Адаптируем поведение в зависимости от фазы
            AdaptBehaviorToPhase(phase);
        }

        private void AdaptBehaviorToPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Day:
                    _taskSearchInterval = 5f;
                    break;
                case GamePhase.Night:
                    // Ночью мафия активнее, остальные осторожнее
                    if (_characterView.Character.Role == Role.Mafia)
                    {
                        _taskSearchInterval = 3f; // Мафия активнее ищет цели
                    }
                    else
                    {
                        _taskSearchInterval = 7f; // Остальные осторожнее
                    }
                    break;
                case GamePhase.Voting:
                    // Во время голосования все менее активны
                    _taskSearchInterval = 8f;
                    break;
            }
        }

        private void UpdateTaskSearch(float deltaTime)
        {
            _taskSearchCooldown -= deltaTime;

            if (_taskSearchCooldown <= 0f)
            {
                _taskSearchCooldown = _taskSearchInterval;
                SearchForNewTarget();
            }
        }

        private void SearchForNewTarget()
        {
            // Учитываем фазу игры при выборе целей
            if (_isVotingPhase)
            {
                // Во время голосования менее активны
                if (Random.value < 0.3f) // 30% шанс найти цель
                {
                    SetRandomDestination();
                }
                return;
            }

            // Сначала ищем задачи (если не мафия ночью)
            if (!(_isNightPhase && _characterView.Character.Role == Role.Mafia))
            {
                var nearestTask = _actionObjectProvider.FindNearestIncompleteTask(
                    _characterView.Transform.position, 
                    _characterView.Character.Role
                );

                if (nearestTask != null && nearestTask != _currentTask)
                {
                    SetTaskTarget(nearestTask);
                    return;
                }
            }

            // Ищем другие объекты по приоритету роли
            var rolePreferences = GetRoleActionPreferences(_characterView.Character.Role);
            
            foreach (var actionType in rolePreferences)
            {
                var actionObject = _actionObjectProvider.FindNearestActionObject(
                    _characterView.Transform.position,
                    _characterView.Character.Role,
                    actionType
                );

                if (actionObject != null)
                {
                    SetActionTarget(actionObject);
                    return;
                }
            }

            // Если ничего не найдено, просто бродим
            SetRandomDestination();
        }

        private ActionType[] GetRoleActionPreferences(Role role)
        {
            // Адаптируем приоритеты в зависимости от фазы
            if (_isNightPhase)
            {
                return role switch
                {
                    Role.Mafia => new[] { ActionType.Kill, ActionType.Sabotage }, // Ночью мафия убивает
                    Role.Police => new[] { ActionType.Investigate, ActionType.Hide }, // Полиция исследует и прячется
                    Role.Civilian => new[] { ActionType.Hide, ActionType.Repair }, // Мирные прячутся
                    _ => new[] { ActionType.Hide }
                };
            }
            
            return role switch
            {
                Role.Mafia => new[] { ActionType.Sabotage, ActionType.Hide, ActionType.Kill },
                Role.Police => new[] { ActionType.Investigate, ActionType.Report },
                Role.Civilian => new[] { ActionType.Repair, ActionType.Interact, ActionType.Report },
                _ => new[] { ActionType.Interact }
            };
        }

        private void SetTaskTarget(TaskObject task)
        {
            _currentTask = task;
            _currentTarget = task;

            Debug.Log($"[AiIdleState-{_characterView.Character.Role}] Найдена задача: {task.TaskName}");

            if (_navMeshAgent != null)
            {
                _navMeshAgent.SetDestination(task.transform.position);
                _stateSwitcher.Switch<AiMoveState>();
            }
        }

        private void SetActionTarget(ActionObject actionObject)
        {
            _currentTarget = actionObject;
            _currentTask = null;

            Debug.Log($"[AiIdleState-{_characterView.Character.Role}] Найден объект для действия: {actionObject.ActionType}");

            if (_navMeshAgent != null)
            {
                _navMeshAgent.SetDestination(actionObject.transform.position);
                _stateSwitcher.Switch<AiMoveState>();
            }
        }

        private void SetRandomDestination()
        {
            // Генерируем случайную точку на NavMesh
            Vector3 randomDirection = Random.insideUnitSphere * 15f;
            randomDirection += _characterView.Transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 15f, NavMesh.AllAreas))
            {
                if (_navMeshAgent != null)
                {
                    _navMeshAgent.SetDestination(hit.position);
                    _stateSwitcher.Switch<AiMoveState>();
                }
            }

            _currentTarget = null;
            _currentTask = null;
        }

        private void UpdateLook(float deltaTime)
        {
            _lookTimer -= deltaTime;

            if (_lookTimer <= 0f)
            {
                _lookTimer = _lookInterval + Random.Range(-0.5f, 0.5f);

                // Случайные повороты головы
                if (Random.value < 0.3f)
                {
                    float lookX = Random.Range(-30f, 30f);
                    _aiInputService?.SetLookInput(new Vector2(lookX, 0f));
                }
                else
                {
                    _aiInputService?.SetLookInput(Vector2.zero);
                }
            }
        }

        private void UpdateActionTimer(float deltaTime)
        {
            _actionTimer -= deltaTime;

            if (_actionTimer <= 0f)
            {
                _actionTimer = _actionInterval + Random.Range(-1f, 1f);
                
                // Выполняем RoleAction если есть
                if (_characterView.Character.HasRoleAction)
                {
                    _characterView.Character.ExecuteRoleAction();
                }
            }
        }

        private void UpdateAnimator()
        {
            if (_characterView.Animator != null)
            {
                float currentSpeed = _characterView.GetMovementSpeed();
                float normalizedSpeed = currentSpeed / 5f;
                
                _characterView.Animator.SetFloat(Speed, normalizedSpeed);
                _characterView.Animator.SetBool(IsMoving, currentSpeed > 0.1f);
                _characterView.Animator.SetBool(IsGrounded, _characterView.IsGrounded());
            }
        }

        private void CheckStateTransitions()
        {
            // Переходим в движение если NavMeshAgent движется
            if (_navMeshAgent != null && _navMeshAgent.hasPath)
            {
                _stateSwitcher.Switch<AiMoveState>();
                return;
            }

            // Переходим к действию если нужно
            if (ShouldPerformAction())
            {
                _stateSwitcher.Switch<AiActionState>();
            }
        }

        private bool ShouldPerformAction()
        {
            // Действие если рядом с целью
            if (_currentTarget != null)
            {
                float distance = Vector3.Distance(_characterView.Transform.position, _currentTarget.transform.position);
                return distance <= _currentTarget.InteractionRange;
            }

            return false;
        }

        private void SetActionIntervalByRole()
        {
            _actionInterval = _characterView.Character.Role switch
            {
                Role.Mafia => 4f,
                Role.Police => 5f,
                Role.Civilian => 6f,
                _ => 5f
            };
        }

        // Методы для получения состояния (для использования другими состояниями)
        public ActionObject GetCurrentTarget() => _currentTarget;
        public TaskObject GetCurrentTask() => _currentTask;
        public bool HasTarget => _currentTarget != null;

        public void SetTarget(ActionObject target)
        {
            _currentTarget = target;
            _currentTask = target as TaskObject;
        }

        // Очистка при уничтожении
        ~AiIdleState()
        {
            UnsubscribeFromEvents();
        }
    }
} 