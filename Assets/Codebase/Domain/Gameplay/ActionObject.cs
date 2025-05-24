using System;
using System.Collections.Generic;
using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public enum ActionType
    {
        Kill,           // Убийство
        Investigate,    // Исследование улик
        Repair,         // Ремонт оборудования
        Sabotage,       // Саботаж
        Interact,       // Общее взаимодействие
        Hide,           // Спрятать/замаскировать
        Report          // Сообщить о находке
    }

    [RequireComponent(typeof(Collider))]
    public abstract class ActionObject : MonoBehaviour
    {
        [Header("Action Settings")]
        [SerializeField] protected ActionType _actionType;
        [SerializeField] protected List<Role> _allowedRoles = new List<Role>();
        [SerializeField] protected float _interactionRange = 2f;
        [SerializeField] protected float _actionDuration = 1f;
        [SerializeField] protected bool _isOneTimeUse = false;
        [SerializeField] protected bool _isCurrentlyUsable = true;

        protected bool _hasBeenUsed = false;

        // События
        public event Action<CharacterView, ActionObject> OnActionStarted;
        public event Action<CharacterView, ActionObject> OnActionCompleted;
        public event Action<CharacterView, ActionObject> OnActionInterrupted;

        public ActionType ActionType => _actionType;
        public List<Role> AllowedRoles => _allowedRoles;
        public float InteractionRange => _interactionRange;
        public float ActionDuration => _actionDuration;
        public bool IsUsable => _isCurrentlyUsable && (!_isOneTimeUse || !_hasBeenUsed);

        protected virtual void Awake()
        {
            // Настраиваем коллайдер как триггер для обнаружения персонажей
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        public virtual bool CanInteract(CharacterView character)
        {
            if (!IsUsable)
                return false;

            if (_allowedRoles.Count > 0 && !_allowedRoles.Contains(character.Character.Role))
                return false;

            float distance = Vector3.Distance(transform.position, character.Transform.position);
            return distance <= _interactionRange;
        }

        public virtual void StartAction(CharacterView character)
        {
            if (!CanInteract(character))
            {
                Debug.LogWarning($"[ActionObject] {character.Character.Role} cannot interact with {name}");
                return;
            }

            Debug.Log($"[ActionObject] {character.Character.Role} started {_actionType} on {name}");
            OnActionStarted?.Invoke(character, this);
            
            OnActionStart(character);
        }

        public virtual void CompleteAction(CharacterView character)
        {
            Debug.Log($"[ActionObject] {character.Character.Role} completed {_actionType} on {name}");
            
            if (_isOneTimeUse)
            {
                _hasBeenUsed = true;
            }

            OnActionCompleted?.Invoke(character, this);
            OnActionComplete(character);
        }

        public virtual void InterruptAction(CharacterView character)
        {
            Debug.Log($"[ActionObject] {character.Character.Role} interrupted {_actionType} on {name}");
            OnActionInterrupted?.Invoke(character, this);
            OnActionInterrupt(character);
        }

        // Методы для переопределения в наследниках
        protected abstract void OnActionStart(CharacterView character);
        protected abstract void OnActionComplete(CharacterView character);
        protected virtual void OnActionInterrupt(CharacterView character) { }

        // Методы для динамического изменения состояния
        public virtual void SetUsable(bool usable)
        {
            _isCurrentlyUsable = usable;
        }

        public virtual void AddAllowedRole(Role role)
        {
            if (!_allowedRoles.Contains(role))
            {
                _allowedRoles.Add(role);
            }
        }

        public virtual void RemoveAllowedRole(Role role)
        {
            _allowedRoles.Remove(role);
        }

        // Визуальная отладка
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = IsUsable ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }
    }
} 