using Codebase.Controllers.Input;
using Codebase.Domain.Gameplay;
using Codebase.Views.Gameplay;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Codebase.Controllers.Fsm.States
{
    public class AiActionState : IState
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        private readonly CharacterView _characterView;

        private IStateSwitcher _stateSwitcher;
        private AiInputService _aiInputService;
        private ActionObject _currentActionObject;
        private CancellationTokenSource _actionCancellationToken;

        public AiActionState(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Initialize(IStateSwitcher stateSwitcher)
        {
            _stateSwitcher = stateSwitcher;
            _aiInputService = _characterView.InputService as AiInputService;
        }

        public void Enter()
        {
            Debug.Log($"[AiActionState] Entering for {_characterView.Character.Role}");

            _aiInputService?.ResetInputs();
            _aiInputService?.SetAttackPressed(true);

            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetTrigger(Attack);
                _characterView.Animator.SetBool(IsAttacking, true);
            }

            // Получаем текущую цель
            if (_stateSwitcher is FsmBase fsm)
            {
                var moveState = fsm.GetState<AiMoveState>();
                if (moveState != null)
                {
                    _currentActionObject = moveState.GetCurrentTarget();
                }
                
                // Если цель не найдена в MoveState, пробуем IdleState
                if (_currentActionObject == null)
                {
                    var idleState = fsm.GetState<AiIdleState>();
                    if (idleState != null)
                    {
                        _currentActionObject = idleState.GetCurrentTarget();
                    }
                }
            }

            StartAction();
        }

        public void Exit()
        {
            Debug.Log($"[AiActionState] Exiting for {_characterView.Character.Role}");

            if (_characterView.Animator != null)
                _characterView.Animator.SetBool(IsAttacking, false);

            _aiInputService?.SetAttackPressed(false);

            // Прерываем текущее действие если оно выполняется
            InterruptCurrentAction();
        }

        public void Update(float deltaTime)
        {
            // Обновляем аниматор
            if (_characterView.Animator != null)
            {
                _characterView.Animator.SetFloat(Speed, 0f);
                _characterView.Animator.SetBool(IsMoving, false);
                _characterView.Animator.SetBool(IsGrounded, _characterView.IsGrounded());
            }
        }

        private void StartAction()
        {
            if (_currentActionObject != null && _currentActionObject.CanInteract(_characterView))
            {
                StartActionWithObject(_currentActionObject);
            }
            else
            {
                // Выполняем RoleAction если нет подходящего объекта
                PerformRoleAction();
            }
        }

        private void StartActionWithObject(ActionObject actionObject)
        {
            Debug.Log($"[AiActionState] Начинаем действие с объектом: {actionObject.name}");

            _actionCancellationToken = new CancellationTokenSource();
            actionObject.StartAction(_characterView);

            // Запускаем асинхронное действие
            PerformAsyncAction(actionObject, _actionCancellationToken.Token).Forget();
        }

        private async UniTaskVoid PerformAsyncAction(ActionObject actionObject, CancellationToken cancellationToken)
        {
            try
            {
                // Ждем указанное время действия
                await UniTask.Delay(
                    Mathf.RoundToInt(actionObject.ActionDuration * 1000),
                    cancellationToken: cancellationToken
                );

                // Если не был отменен, завершаем действие
                if (!cancellationToken.IsCancellationRequested)
                {
                    actionObject.CompleteAction(_characterView);
                    OnActionCompleted();
                }
            }
            catch (System.OperationCanceledException)
            {
                // Действие было прервано
                actionObject.InterruptAction(_characterView);
                Debug.Log($"[AiActionState] Действие с {actionObject.name} было прервано");
            }
        }

        private void PerformRoleAction()
        {
            // Выполняем специальное действие роли
            if (_characterView.Character.HasRoleAction)
            {
                Debug.Log($"[AiActionState] Выполняем RoleAction для {_characterView.Character.Role}");
                _characterView.Character.ExecuteRoleAction();
            }
            else
            {
                Debug.Log($"[AiActionState] Общее действие для {_characterView.Character.Role}");
            }

            // Простое ожидание для действий без объектов
            float defaultDuration = GetDefaultActionDuration();
            _actionCancellationToken = new CancellationTokenSource();
            WaitAndComplete(defaultDuration, _actionCancellationToken.Token).Forget();
        }

        private float GetDefaultActionDuration()
        {
            return _characterView.Character.Role switch
            {
                Role.Mafia => 2f,
                Role.Police => 1.5f,
                Role.Civilian => 1f,
                _ => 1f
            };
        }

        private async UniTaskVoid WaitAndComplete(float duration, CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    Mathf.RoundToInt(duration * 1000),
                    cancellationToken: cancellationToken
                );

                if (!cancellationToken.IsCancellationRequested)
                {
                    OnActionCompleted();
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log($"[AiActionState] RoleAction было прервано");
            }
        }

        private void OnActionCompleted()
        {
            Debug.Log($"[AiActionState] Действие завершено для {_characterView.Character.Role}");
            
            // Очищаем цель после выполнения действия
            ClearCurrentTarget();
            
            _stateSwitcher.Switch<AiIdleState>();
        }

        private void InterruptCurrentAction()
        {
            if (_actionCancellationToken != null && !_actionCancellationToken.IsCancellationRequested)
            {
                _actionCancellationToken.Cancel();
                _actionCancellationToken.Dispose();
                _actionCancellationToken = null;
            }

            _currentActionObject = null;
        }

        private void ClearCurrentTarget()
        {
            // Сообщаем другим состояниям, что цель больше не актуальна
            if (_stateSwitcher is FsmBase fsm)
            {
                var idleState = fsm.GetState<AiIdleState>();
                if (idleState != null)
                {
                    idleState.SetTarget(null);
                }

                var moveState = fsm.GetState<AiMoveState>();
                if (moveState != null)
                {
                    moveState.SetTarget(null);
                }
            }
        }

        // Метод для принудительного прерывания действия
        public void ForceInterrupt()
        {
            InterruptCurrentAction();
            _stateSwitcher.Switch<AiIdleState>();
        }

        public ActionObject GetCurrentActionObject() => _currentActionObject;
    }
} 