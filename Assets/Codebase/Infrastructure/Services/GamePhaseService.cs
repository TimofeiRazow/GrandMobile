using System;
using Codebase.Controllers;
using UnityEngine;

namespace Codebase.Infrastructure.Services
{
    public class GamePhaseService
    {
        private GamePhase _currentPhase = GamePhase.Day;
        private float _phaseTimeRemaining;
        
        // Настройки времени фаз
        private readonly float _dayDuration = 120f;
        private readonly float _nightDuration = 60f;
        private readonly float _votingDuration = 30f;

        // События
        public event Action<GamePhase> PhaseChanged;
        public event Action<GamePhase, float> PhaseTimeUpdated;
        public event Action<string> GameMessage;

        public GamePhase CurrentPhase => _currentPhase;
        public float PhaseTimeRemaining => _phaseTimeRemaining;

        public void StartDay()
        {
            SetPhase(GamePhase.Day, _dayDuration);
            GameMessage?.Invoke("🌞 ДЕНЬ НАЧАЛСЯ! Граждане работают, маньяк саботирует!");
            Debug.Log("[GamePhaseService] Day phase started");
        }

        public void StartNight()
        {
            SetPhase(GamePhase.Night, _nightDuration);
            GameMessage?.Invoke("🌙 НОЧЬ НАЧАЛАСЬ! Маньяк охотится, остальные выживают!");
            Debug.Log("[GamePhaseService] Night phase started");
        }

        public void StartVoting()
        {
            SetPhase(GamePhase.Voting, _votingDuration);
            GameMessage?.Invoke("🗳️ ГОЛОСОВАНИЕ! Кто подозрительный?");
            Debug.Log("[GamePhaseService] Voting phase started");
        }

        public void StartGameOver()
        {
            SetPhase(GamePhase.GameOver, 0f);
            Debug.Log("[GamePhaseService] Game over");
        }

        public void Update(float deltaTime)
        {
            if (_currentPhase == GamePhase.GameOver)
                return;

            _phaseTimeRemaining -= deltaTime;
            PhaseTimeUpdated?.Invoke(_currentPhase, _phaseTimeRemaining);

            if (_phaseTimeRemaining <= 0f)
            {
                SwitchToNextPhase();
            }
        }

        private void SetPhase(GamePhase phase, float duration)
        {
            _currentPhase = phase;
            _phaseTimeRemaining = duration;
            PhaseChanged?.Invoke(phase);
        }

        private void SwitchToNextPhase()
        {
            switch (_currentPhase)
            {
                case GamePhase.Day:
                    StartNight();
                    break;
                case GamePhase.Night:
                    StartVoting();
                    break;
                case GamePhase.Voting:
                    StartDay();
                    break;
            }
        }

        public void ForcePhaseChange()
        {
            _phaseTimeRemaining = 0f;
        }

        public void SetPhaseDuration(GamePhase phase, float duration)
        {
            // Для настройки длительности фаз извне
            // Можно добавить конфигурацию
        }
    }
} 