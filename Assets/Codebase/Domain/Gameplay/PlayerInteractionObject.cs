using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class PlayerInteractionObject : ActionObject
    {
        [Header("Player Interaction")]
        [SerializeField] private CharacterView _targetCharacter;
        [SerializeField] private bool _isTargetDead = false;

        public CharacterView TargetCharacter => _targetCharacter;
        public bool IsTargetDead => _isTargetDead;

        protected override void Awake()
        {
            base.Awake();
            
            // Динамически настраиваем в зависимости от цели
            _actionDuration = 1.5f;
            _interactionRange = 1.5f;
            _isOneTimeUse = false; // Можно взаимодействовать многократно
        }

        public void Initialize(CharacterView targetCharacter)
        {
            _targetCharacter = targetCharacter;
            
            // Следуем за целевым персонажем
            transform.SetParent(targetCharacter.Transform);
            transform.localPosition = Vector3.zero;
        }

        public override bool CanInteract(CharacterView character)
        {
            if (!base.CanInteract(character))
                return false;

            // Нельзя взаимодействовать с самим собой
            if (character == _targetCharacter)
                return false;

            // Определяем доступные действия по роли
            switch (character.Character.Role)
            {
                case Role.Mafia:
                    // Мафия может убивать живых или прятать трупы
                    _actionType = _isTargetDead ? ActionType.Hide : ActionType.Kill;
                    return true;
                
                case Role.Police:
                    // Детектив может исследовать трупы или проверять живых
                    _actionType = _isTargetDead ? ActionType.Investigate : ActionType.Interact;
                    return true;
                
                case Role.Civilian:
                    // Гражданские могут только докладывать о трупах
                    _actionType = _isTargetDead ? ActionType.Report : ActionType.Interact;
                    return _isTargetDead; // Только с трупами
                
                default:
                    return false;
            }
        }

        protected override void OnActionStart(CharacterView character)
        {
            switch (_actionType)
            {
                case ActionType.Kill:
                    Debug.Log($"[PlayerInteraction] {character.Character.Role} начал убийство {_targetCharacter.Character.Role}");
                    break;
                
                case ActionType.Hide:
                    Debug.Log($"[PlayerInteraction] {character.Character.Role} начал прятать тело {_targetCharacter.Character.Role}");
                    break;
                
                case ActionType.Investigate:
                    Debug.Log($"[PlayerInteraction] {character.Character.Role} начал исследование {_targetCharacter.Character.Role}");
                    break;
                
                case ActionType.Report:
                    Debug.Log($"[PlayerInteraction] {character.Character.Role} начал докладывать о теле {_targetCharacter.Character.Role}");
                    break;
                
                case ActionType.Interact:
                    Debug.Log($"[PlayerInteraction] {character.Character.Role} начал проверку {_targetCharacter.Character.Role}");
                    break;
            }
        }

        protected override void OnActionComplete(CharacterView character)
        {
            switch (_actionType)
            {
                case ActionType.Kill:
                    PerformKill(character);
                    break;
                
                case ActionType.Hide:
                    PerformHideBody(character);
                    break;
                
                case ActionType.Investigate:
                    PerformInvestigation(character);
                    break;
                
                case ActionType.Report:
                    PerformReport(character);
                    break;
                
                case ActionType.Interact:
                    PerformCheck(character);
                    break;
            }
        }

        private void PerformKill(CharacterView killer)
        {
            if (_isTargetDead)
                return;

            Debug.Log($"[PlayerInteraction] {killer.Character.Role} убил {_targetCharacter.Character.Role}!");
            
            _isTargetDead = true;
            
            // Переводим цель в состояние смерти
            _targetCharacter.InputService.Disable();
            
            // Создаем улики на месте убийства
            CreateMurderEvidence();
            
            // Уведомляем игровой менеджер

        }

        private void PerformHideBody(CharacterView character)
        {
            Debug.Log($"[PlayerInteraction] {character.Character.Role} спрятал тело {_targetCharacter.Character.Role}");
            
            // Убираем объект с уликами поблизости
            var evidences = FindObjectsOfType<EvidenceObject>();
            foreach (var evidence in evidences)
            {
                float distance = Vector3.Distance(evidence.transform.position, transform.position);
                if (distance < 3f)
                {
                    evidence.HideEvidence();
                }
            }
            
            // Делаем тело менее заметным
            if (_targetCharacter.Body != null)
            {
                Color color = _targetCharacter.Body.material.color;
                color.a = 0.3f; // Полупрозрачность
                _targetCharacter.Body.material.color = color;
            }
        }

        private void PerformInvestigation(CharacterView detective)
        {
            Debug.Log($"[PlayerInteraction] {detective.Character.Role} исследовал {(_isTargetDead ? "тело" : "персонажа")} {_targetCharacter.Character.Role}");
            
            if (_isTargetDead)
            {
                // Детектив получает информацию о причине смерти
                Debug.Log($"[PlayerInteraction] Причина смерти: убийство. Тело {_targetCharacter.Character.Role}");
            }
            else
            {
                // Проверка живого персонажа (может определить роль или алиби)
                Debug.Log($"[PlayerInteraction] {_targetCharacter.Character.Role} проверен детективом");
            }
        }

        private void PerformReport(CharacterView reporter)
        {
            Debug.Log($"[PlayerInteraction] {reporter.Character.Role} сообщил о теле {_targetCharacter.Character.Role}!");
            
            // Уведомляем всех игроков о находке

        }

        private void PerformCheck(CharacterView checker)
        {
            Debug.Log($"[PlayerInteraction] {checker.Character.Role} проверил {_targetCharacter.Character.Role}");
            // Обычное взаимодействие между живыми персонажами
        }

        private void CreateMurderEvidence()
        {
            // Создаем улику на месте убийства
            var evidence = EvidenceObject.CreateBodyEvidence(
                transform.position + Random.insideUnitSphere * 2f,
                $"Следы борьбы возле тела {_targetCharacter.Character.Role}"
            );
            
            Debug.Log($"[PlayerInteraction] Создана улика об убийстве");
        }

        private void Update()
        {
            // Обновляем доступность действий в зависимости от состояния цели
            if (_targetCharacter != null)
            {
                // Проверяем, не изменилось ли состояние персонажа
                // Это можно сделать через FSM или другие механики
            }
        }
    }
} 