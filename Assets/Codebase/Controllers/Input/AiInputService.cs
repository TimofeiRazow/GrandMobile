using Codebase.Domain.Gameplay;
using UnityEngine;

namespace Codebase.Controllers.Input
{
    public class AiInputService : IInputService
    {
        private Character _character;
        private Transform _transform;

        // Параметры движения
        private Vector2 _currentMovementInput;
        private float _movementChangeTimer;
        private readonly float _movementChangeInterval = 2f; // Меняем направление каждые 2 секунды
        private readonly float _moveChance = 0.7f; // 70% шанс двигаться

        // Параметры действий
        private float _actionTimer;
        private readonly float _baseActionInterval = 5f; // Базовый интервал действий
        private float _currentActionInterval;

        // Параметры обзора (для имитации осмотра)
        private Vector2 _currentLookInput;
        private float _lookChangeTimer;
        private readonly float _lookChangeInterval = 1.5f;

        public void Initialize(Character character, Transform transform)
        {
            _character = character;
            _transform = transform;

            _movementChangeTimer = Random.Range(0f, _movementChangeInterval);
            _lookChangeTimer = Random.Range(0f, _lookChangeInterval);

            SetActionIntervalByRole();
            _actionTimer = Random.Range(0f, _currentActionInterval);

            Debug.Log($"[AiInputService] Initialized AI for {_character.Role}");
        }

        public void Update(float deltaTime)
        {
            UpdateMovement(deltaTime);
            UpdateLook(deltaTime);
            UpdateActionTimer(deltaTime);
        }

        public Vector2 GetMovementInput()
        {
            return _currentMovementInput;
        }

        public Vector2 GetLookInput()
        {
            return _currentLookInput;
        }

        public bool IsAttackPressed()
        {
            // Возвращаем true когда таймер действия истек
            return _actionTimer <= 0f;
        }

        public bool IsInteractPressed()
        {
            // ИИ редко взаимодействует, в основном через действия
            return false;
        }

        public bool IsSprintPressed()
        {
            // ИИ бегает случайно, чтобы выглядеть более естественно
            return Random.value < 0.3f; // 30% шанс бега
        }

        public bool IsJumpPressed()
        {
            // ИИ редко прыгает
            return Random.value < 0.05f; // 5% шанс прыжка
        }

        public bool IsCrouchPressed()
        {
            // ИИ не приседает в базовой версии
            return false;
        }

        private void SetActionIntervalByRole()
        {
            switch (_character.Role)
            {
                case Role.Mafia:
                    // Маньяк действует чаще - более агрессивен
                    _currentActionInterval = _baseActionInterval * 0.8f;
                    break;

                case Role.Police:
                    // Детектив действует средне - ищет улики
                    _currentActionInterval = _baseActionInterval * 1.0f;
                    break;

                case Role.Civilian:
                    // Гражданский действует реже - работает спокойно
                    _currentActionInterval = _baseActionInterval * 1.2f;
                    break;

                default:
                    _currentActionInterval = _baseActionInterval;
                    break;
            }
        }

        private void UpdateMovement(float deltaTime)
        {
            _movementChangeTimer -= deltaTime;

            if (_movementChangeTimer <= 0f)
            {
                _movementChangeTimer = _movementChangeInterval + Random.Range(-0.5f, 0.5f);

                if (Random.value < _moveChance)
                {
                    GenerateNewMovementDirection();
                }
                else
                {
                    _currentMovementInput = Vector2.zero;
                }
            }
        }

        private void GenerateNewMovementDirection()
        {
            Vector2 direction = Vector2.zero;

            switch (_character.Role)
            {
                case Role.Mafia:
                    direction = Random.insideUnitCircle.normalized;
                    break;

                case Role.Police:
                    float angle = Random.Range(0f, 360f);
                    direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    break;

                case Role.Civilian:
                    int randomDir = Random.Range(0, 8);
                    switch (randomDir)
                    {
                        case 0:
                            direction = Vector2.up;
                            break;
                        case 1:
                            direction = Vector2.down;
                            break;
                        case 2:
                            direction = Vector2.left;
                            break;
                        case 3:
                            direction = Vector2.right;
                            break;
                        case 4:
                            direction = new Vector2(1, 1).normalized;
                            break;
                        case 5:
                            direction = new Vector2(-1, 1).normalized;
                            break;
                        case 6:
                            direction = new Vector2(1, -1).normalized;
                            break;
                        case 7:
                            direction = new Vector2(-1, -1).normalized;
                            break;
                    }

                    break;
            }

            _currentMovementInput = direction;
        }

        private void UpdateLook(float deltaTime)
        {
            _lookChangeTimer -= deltaTime;

            if (_lookChangeTimer <= 0f)
            {
                _lookChangeTimer = _lookChangeInterval + Random.Range(-0.3f, 0.3f);

                if (Random.value < 0.4f) // 40% шанс посмотреть по сторонам
                {
                    float lookX = Random.Range(-50f, 50f);
                    _currentLookInput = new Vector2(lookX, 0f);
                }
                else
                {
                    _currentLookInput = Vector2.zero;
                }
            }
        }

        private void UpdateActionTimer(float deltaTime)
        {
            _actionTimer -= deltaTime;

            if (_actionTimer <= 0f)
            {
                _actionTimer = _currentActionInterval + Random.Range(-1f, 1f);

                LogRoleAction();
            }
        }

        private void LogRoleAction()
        {
            switch (_character.Role)
            {
                case Role.Mafia:
                    string[] mafiaActions =
                        { "саботирует оборудование", "ищет жертву", "прячет улики", "изучает местность" };
                    string mafiaAction = mafiaActions[Random.Range(0, mafiaActions.Length)];
                    Debug.Log($"[AI-Mafia] {mafiaAction}");
                    break;

                case Role.Police:
                    string[] policeActions =
                    {
                        "ищет улики", "патрулирует территорию", "проверяет подозрительные места",
                        "анализирует обстановку"
                    };
                    string policeAction = policeActions[Random.Range(0, policeActions.Length)];
                    Debug.Log($"[AI-Police] {policeAction}");
                    break;

                case Role.Civilian:
                    string[] civilianActions =
                        { "чинит машину", "выполняет задания", "ищет запчасти", "проверяет оборудование" };
                    string civilianAction = civilianActions[Random.Range(0, civilianActions.Length)];
                    Debug.Log($"[AI-Civilian] {civilianAction}");
                    break;
            }
        }

        public void SetMovementActive(bool active)
        {
            if (active)
                return;

            _currentMovementInput = Vector2.zero;
        }

        public void ForceAction()
        {
            _actionTimer = 0f;
        }

        public void SetActionInterval(float interval)
        {
            _currentActionInterval = interval;
        }

        public void SetAggressiveness(float aggressiveness)
        {
            // Изменяем частоту действий в зависимости от агрессивности (0-2)
            _currentActionInterval = _baseActionInterval / Mathf.Clamp(aggressiveness, 0.5f, 2f);
        }
    }
}