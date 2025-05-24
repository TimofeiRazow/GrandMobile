using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Domain.Gameplay;
using UnityEngine;

namespace Codebase.Infrastructure.Services
{
    public class ActionObjectProvider
    {
        private readonly List<ActionObject> _actionObjects = new List<ActionObject>();
        private readonly Dictionary<Role, List<ActionObject>> _roleCache = new Dictionary<Role, List<ActionObject>>();
        private readonly Dictionary<ActionType, List<ActionObject>> _typeCache = new Dictionary<ActionType, List<ActionObject>>();
        
        private bool _cacheNeedsUpdate = true;

        public event Action<ActionObject> OnObjectRegistered;
        public event Action<ActionObject> OnObjectUnregistered;

        public void RegisterActionObject(ActionObject actionObject)
        {
            if (actionObject == null || _actionObjects.Contains(actionObject))
                return;

            _actionObjects.Add(actionObject);
            _cacheNeedsUpdate = true;
            
            OnObjectRegistered?.Invoke(actionObject);
            Debug.Log($"[ActionObjectProvider] Registered {actionObject.ActionType} object: {actionObject.name}");
        }

        public void UnregisterActionObject(ActionObject actionObject)
        {
            if (actionObject == null || !_actionObjects.Contains(actionObject))
                return;

            _actionObjects.Remove(actionObject);
            _cacheNeedsUpdate = true;
            
            OnObjectUnregistered?.Invoke(actionObject);
            Debug.Log($"[ActionObjectProvider] Unregistered {actionObject.ActionType} object: {actionObject.name}");
        }

        public ActionObject FindNearestActionObject(Vector3 position, Role role, ActionType? filterType = null)
        {
            UpdateCacheIfNeeded();

            var candidates = GetCandidates(role, filterType);
            
            ActionObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var obj in candidates)
            {
                if (!obj.IsUsable)
                    continue;

                float distance = Vector3.Distance(position, obj.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = obj;
                }
            }

            return nearest;
        }

        public List<ActionObject> FindActionObjectsInRange(Vector3 position, float range, Role role, ActionType? filterType = null)
        {
            UpdateCacheIfNeeded();

            var candidates = GetCandidates(role, filterType);
            var objectsInRange = new List<ActionObject>();

            foreach (var obj in candidates)
            {
                if (!obj.IsUsable)
                    continue;

                float distance = Vector3.Distance(position, obj.transform.position);
                if (distance <= range)
                {
                    objectsInRange.Add(obj);
                }
            }

            return objectsInRange;
        }

        public TaskObject FindNearestIncompleteTask(Vector3 position, Role role)
        {
            UpdateCacheIfNeeded();

            var tasks = _actionObjects.OfType<TaskObject>()
                .Where(t => !t.IsCompleted && 
                           (t.AllowedRoles.Count == 0 || t.AllowedRoles.Contains(role)))
                .ToList();

            TaskObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var task in tasks)
            {
                float distance = Vector3.Distance(position, task.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = task;
                }
            }

            return nearest;
        }

        public List<ActionObject> GetActionObjectsByType(ActionType actionType)
        {
            UpdateCacheIfNeeded();
            
            return _typeCache.TryGetValue(actionType, out var objects) 
                ? new List<ActionObject>(objects) 
                : new List<ActionObject>();
        }

        public List<ActionObject> GetActionObjectsByRole(Role role)
        {
            UpdateCacheIfNeeded();
            
            return _roleCache.TryGetValue(role, out var objects) 
                ? new List<ActionObject>(objects) 
                : new List<ActionObject>();
        }

        public List<ActionObject> GetAllActionObjects()
        {
            return new List<ActionObject>(_actionObjects);
        }

        public int GetActiveObjectsCount() => _actionObjects.Count(obj => obj.IsUsable);

        private IEnumerable<ActionObject> GetCandidates(Role role, ActionType? filterType)
        {
            IEnumerable<ActionObject> candidates = _actionObjects;

            // Фильтруем по роли
            if (_roleCache.TryGetValue(role, out var roleObjects))
            {
                candidates = roleObjects;
            }
            else
            {
                candidates = candidates.Where(obj => obj.AllowedRoles.Count == 0 || obj.AllowedRoles.Contains(role));
            }

            // Фильтруем по типу
            if (filterType.HasValue)
            {
                candidates = candidates.Where(obj => obj.ActionType == filterType.Value);
            }

            return candidates;
        }

        private void UpdateCacheIfNeeded()
        {
            if (!_cacheNeedsUpdate)
                return;

            UpdateCache();
            _cacheNeedsUpdate = false;
        }

        private void UpdateCache()
        {
            _roleCache.Clear();
            _typeCache.Clear();

            foreach (var obj in _actionObjects)
            {
                // Кэш по типам
                if (!_typeCache.TryGetValue(obj.ActionType, out var typeList))
                {
                    typeList = new List<ActionObject>();
                    _typeCache[obj.ActionType] = typeList;
                }
                typeList.Add(obj);

                // Кэш по ролям
                if (obj.AllowedRoles.Count == 0)
                {
                    // Если роли не указаны, добавляем ко всем ролям
                    foreach (Role role in Enum.GetValues(typeof(Role)))
                    {
                        if (!_roleCache.TryGetValue(role, out var roleList))
                        {
                            roleList = new List<ActionObject>();
                            _roleCache[role] = roleList;
                        }
                        roleList.Add(obj);
                    }
                }
                else
                {
                    foreach (var role in obj.AllowedRoles)
                    {
                        if (!_roleCache.TryGetValue(role, out var roleList))
                        {
                            roleList = new List<ActionObject>();
                            _roleCache[role] = roleList;
                        }
                        roleList.Add(obj);
                    }
                }
            }
        }

        public void Clear()
        {
            _actionObjects.Clear();
            _roleCache.Clear();
            _typeCache.Clear();
            _cacheNeedsUpdate = true;
        }

        // Методы для отладки
        public void LogStats()
        {
            UpdateCacheIfNeeded();
            
            Debug.Log($"[ActionObjectProvider] Total objects: {_actionObjects.Count}");
            Debug.Log($"[ActionObjectProvider] Active objects: {GetActiveObjectsCount()}");
            
            foreach (var kvp in _typeCache)
            {
                Debug.Log($"[ActionObjectProvider] {kvp.Key}: {kvp.Value.Count} objects");
            }
        }
    }
} 