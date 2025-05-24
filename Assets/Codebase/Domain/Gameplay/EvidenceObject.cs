using Codebase.Views.Gameplay;
using UnityEngine;

namespace Codebase.Domain.Gameplay
{
    public class EvidenceObject : ActionObject
    {
        [Header("Evidence Settings")]
        [SerializeField] private string _evidenceDescription = "Suspicious item";
        [SerializeField] private bool _isHidden = false;
        [SerializeField] private float _discoveryRange = 1f;

        public string EvidenceDescription => _evidenceDescription;
        public bool IsHidden => _isHidden;

        protected override void Awake()
        {
            base.Awake();
            
            // Улики могут исследовать только детективы
            _actionType = ActionType.Investigate;
            _allowedRoles.Clear();
            _allowedRoles.Add(Role.Police);
            _actionDuration = 2f;
            _isOneTimeUse = true;
            
            // Скрытые улики имеют меньший радиус обнаружения
            if (_isHidden)
            {
                _interactionRange = _discoveryRange;
            }
        }

        protected override void OnActionStart(CharacterView character)
        {
            Debug.Log($"[EvidenceObject] {character.Character.Role} начал исследование улики: {_evidenceDescription}");
        }

        protected override void OnActionComplete(CharacterView character)
        {
            Debug.Log($"[EvidenceObject] {character.Character.Role} обнаружил улику: {_evidenceDescription}");
            
            // Уведомляем игровой менеджер об обнаружении улики
            
            // Делаем улику видимой для всех после обнаружения
            _isHidden = false;
            
            // Можно добавить визуальные эффекты
            ShowDiscoveryEffect();
        }

        private void ShowDiscoveryEffect()
        {
            // Здесь можно добавить партиклы, звук, изменение материала и т.д.
            Debug.Log($"[EvidenceObject] Показываем эффект обнаружения улики");
        }

        public void HideEvidence()
        {
            _isHidden = true;
            _hasBeenUsed = false;
            _interactionRange = _discoveryRange;
        }

        // Статический метод для создания улик от трупов
        public static EvidenceObject CreateBodyEvidence(Vector3 position, string description)
        {
            var evidenceObject = new GameObject("Body Evidence").AddComponent<EvidenceObject>();
            evidenceObject.transform.position = position;
            evidenceObject._evidenceDescription = description;
            evidenceObject._isHidden = false;
            evidenceObject._interactionRange = 2f;
            
            return evidenceObject;
        }
    }
} 