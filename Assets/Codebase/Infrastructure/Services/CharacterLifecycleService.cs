using System;
using System.Collections.Generic;
using System.Linq;
using Codebase.Domain.Gameplay;
using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Infrastructure.Services
{
    public class CharacterLifecycleService
    {
        private readonly List<CharacterView> _allCharacters = new List<CharacterView>();
        private readonly List<CharacterView> _deadCharacters = new List<CharacterView>();

        // События
        public event Action<CharacterView> CharacterRegistered;
        public event Action<CharacterView> CharacterKilled;
        public event Action<CharacterView, CharacterView> CharacterAttacked; // (attacker, victim)
        public event Action<Role, int> RoleCountChanged;

        // Геттеры
        public IReadOnlyList<CharacterView> AllCharacters => _allCharacters.AsReadOnly();
        public IReadOnlyList<CharacterView> AliveCharacters => _allCharacters.Where(c => !_deadCharacters.Contains(c)).ToList().AsReadOnly();
        public IReadOnlyList<CharacterView> DeadCharacters => _deadCharacters.AsReadOnly();

        public void RegisterCharacter(CharacterView character)
        {
            if (character == null || _allCharacters.Contains(character))
                return;

            _allCharacters.Add(character);
            CharacterRegistered?.Invoke(character);
            UpdateRoleCount(character.Character.Role);

            Debug.Log($"[CharacterLifecycleService] Registered {character.Character.Role}: {character.name}");
        }

        public void UnregisterCharacter(CharacterView character)
        {
            if (character == null)
                return;

            _allCharacters.Remove(character);
            _deadCharacters.Remove(character);
            UpdateRoleCount(character.Character.Role);

            Debug.Log($"[CharacterLifecycleService] Unregistered {character.Character.Role}: {character.name}");
        }

        public void KillCharacter(CharacterView victim, CharacterView killer = null)
        {
            if (victim == null || _deadCharacters.Contains(victim))
                return;

            _deadCharacters.Add(victim);
            CharacterKilled?.Invoke(victim);
            UpdateRoleCount(victim.Character.Role);

            string killerInfo = killer != null ? $"by {killer.Character.Role}" : "by unknown";
            Debug.Log($"[CharacterLifecycleService] {victim.Character.Role} was killed {killerInfo}");

            if (killer != null)
            {
                CharacterAttacked?.Invoke(killer, victim);
            }
        }

        public void ReviveCharacter(CharacterView character)
        {
            if (character == null || !_deadCharacters.Contains(character))
                return;

            _deadCharacters.Remove(character);
            UpdateRoleCount(character.Character.Role);

            Debug.Log($"[CharacterLifecycleService] {character.Character.Role} was revived");
        }

        public List<CharacterView> GetCharactersByRole(Role role, bool aliveOnly = true)
        {
            var characters = aliveOnly ? AliveCharacters : AllCharacters;
            return characters.Where(c => c.Character.Role == role).ToList();
        }

        public int GetAliveCount(Role role)
        {
            return AliveCharacters.Count(c => c.Character.Role == role);
        }

        public int GetTotalCount(Role role)
        {
            return AllCharacters.Count(c => c.Character.Role == role);
        }

        public bool IsCharacterAlive(CharacterView character)
        {
            return character != null && !_deadCharacters.Contains(character);
        }

        public CharacterView FindNearestCharacter(Vector3 position, Role? roleFilter = null, bool aliveOnly = true, CharacterView excludeCharacter = null)
        {
            var candidates = aliveOnly ? AliveCharacters : AllCharacters;
            
            if (roleFilter.HasValue)
            {
                candidates = candidates.Where(c => c.Character.Role == roleFilter.Value).ToList();
            }

            if (excludeCharacter != null)
            {
                candidates = candidates.Where(c => c != excludeCharacter).ToList();
            }

            CharacterView nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var character in candidates)
            {
                float distance = Vector3.Distance(position, character.Transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = character;
                }
            }

            return nearest;
        }

        public List<CharacterView> GetCharactersInRange(Vector3 position, float range, Role? roleFilter = null, bool aliveOnly = true)
        {
            var candidates = aliveOnly ? AliveCharacters : AllCharacters;
            
            if (roleFilter.HasValue)
            {
                candidates = candidates.Where(c => c.Character.Role == roleFilter.Value).ToList();
            }

            return candidates.Where(c => Vector3.Distance(position, c.Transform.position) <= range).ToList();
        }

        private void UpdateRoleCount(Role role)
        {
            int aliveCount = GetAliveCount(role);
            RoleCountChanged?.Invoke(role, aliveCount);
        }

        public void Clear()
        {
            _allCharacters.Clear();
            _deadCharacters.Clear();
        }

        // Статистика
        public void LogStatistics()
        {
            Debug.Log($"[CharacterLifecycleService] Total characters: {_allCharacters.Count}");
            Debug.Log($"[CharacterLifecycleService] Alive characters: {AliveCharacters.Count}");
            Debug.Log($"[CharacterLifecycleService] Dead characters: {_deadCharacters.Count}");
            
            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                int alive = GetAliveCount(role);
                int total = GetTotalCount(role);
                Debug.Log($"[CharacterLifecycleService] {role}: {alive}/{total} alive");
            }
        }
    }
} 