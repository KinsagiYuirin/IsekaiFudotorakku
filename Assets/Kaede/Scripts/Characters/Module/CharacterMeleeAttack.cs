using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.Utils;
using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Characters.Module
{
    
    [Serializable]
    public struct MeleeAttackPattern
    {
        [BoxGroup("Area"), Required] public DamageArea damageArea;
        [BoxGroup("Damage"), Min(0)] public float damage;
        [BoxGroup("Timing"), Min(0)] public float delay;
        [BoxGroup("Timing"), Min(0)] public float duration;
        [BoxGroup("Timing"), Min(0)] public float interval;
        [BoxGroup("Timing"), Min(0)] public float resetComboTime;
    }
    
    public class CharacterMeleeAttack : DamageDataBase
    {
        [Title("Settings")]
        [SerializeField] protected DamageType damageType;
        [SerializeField] private Transform comboParent;
        [SerializeField] protected List<MeleeAttackPattern> meleeAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] protected int currentPatternIndex;
        [SerializeField, DisplayAsString] protected int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackUsed;
        [SerializeField, DisplayAsString] protected float currentInterval;
        [SerializeField, DisplayAsString] protected float currentComboTime;
        
        public MeleeAttackPattern? CurrentPattern => meleeAttackPatterns[currentPatternIndex];
        protected MeleeAttackPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return meleeAttackPatterns[previousPatternIndex];
            }
        }
        
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            if (PlayerInput.LeftMouseClick.Value.isDown)
            {
                OnAttack();
            }
        }
        
        protected override void UpdateModule()
        {
            if (!ModulePermitted) return;
            base.UpdateModule();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            LateUpdateModule();
        }

        protected override void LateUpdateModule()
        {
            base.LateUpdateModule();
        }
        
        public override void Shutdown()
        {
            base.Shutdown();
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
        }
        
        public override void Initialize(CharacterHub characterHub)
        {
            base.Initialize(characterHub);
            currentPatternIndex = 0;
            currentInterval = 0;
            previousPatternIndex = -1;
            meleeAttackPatterns.ForEach(pattern =>
            {
                pattern.damageArea.SetActive(false);
                pattern.damageArea.OnHitEvent += OnHit;
            });
        }
        
        /// <summary>
        /// Method called when the damage area hits a collider.
        /// </summary>
        /// <param name="collider">Collider that was hit.</param>
        protected override void OnHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            
            DamageData data = new DamageData
            {
                type = damageType,
            };
            
            var armorModule = characterHub.FindModuleOfType<CharacterArmorModule>();
            if (armorModule && CurrentPattern != null)
                //armorModule.ChangeArmor(-CurrentPattern.Value.damage);
                armorModule.ReceiveDamage(-CurrentPattern.Value.damage, data);
            
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (healthModule && CurrentPattern != null) 
                //healthModule.ChangeHealth(-CurrentPattern.Value.damage);
                healthModule.ReceiveDamage(-CurrentPattern.Value.damage, data);
        }
        
        protected override void OnAttack()
        {
            if (!ModulePermitted) return;
            
            _ = AttackAsync();
        }
        
        private async UniTask AttackAsync()
        {
            if (CurrentPattern == null) return;

            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.MeleeAttacking);

            await UniTask.Delay(TimeSpan.FromSeconds(CurrentPattern.Value.delay));
            CurrentPattern.Value.damageArea.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(CurrentPattern.Value.duration));
            CurrentPattern.Value.damageArea.SetActive(false);
            
            characterHub.ChangeActionState(CharacterActionState.None);

            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % meleeAttackPatterns.Count;
        }
    }
}
