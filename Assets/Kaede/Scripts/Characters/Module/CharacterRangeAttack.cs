using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Characters.Module
{

    [Serializable]
    public struct RangeAttackPattern 
    {
        [TabGroup("FirePoint"), Required] public Transform firePoint;
        [TabGroup("RangeArea"), Required] public ProjectileDamageArea projectileDamageAreaPrefab;
        [TabGroup("Damage"), Min(0)] public float damage;
        [TabGroup("Damage"), Min(0)] public LayerMask passThroughLayer;
        [TabGroup("Speed"), Min(0)] public float projectileSpeed;
        [TabGroup("Timing"), Min(0)] public float delay;
        [TabGroup("Timing"), Min(0)] public bool hasDuration;
        [TabGroup("Timing"), Min(0), ShowIf(nameof(hasDuration))] public float duration;
        [TabGroup("Timing"), Min(0)] public float interval;
        [TabGroup("Timing"), Min(0)] public float resetComboTime;
        [TabGroup("Energy"), Min(0)] public float getEnergy;
    }
    
    public class CharacterRangeAttack : DamageDataBase
    {
        [Title("Settings")]
        [SerializeField] protected DamageType damageType;
        [SerializeField] private Transform comboParent;
        [SerializeField] protected List<RangeAttackPattern> rangeAttackPatterns;
        
        [Title("Debug")]
        [SerializeField, DisplayAsString] protected int currentPatternIndex;
        [SerializeField, DisplayAsString] protected int previousPatternIndex = -1;
        [SerializeField, DisplayAsString] private bool attackReady;
        [SerializeField, DisplayAsString] private bool attackUsed;
        [SerializeField, DisplayAsString] protected float currentInterval;
        [SerializeField, DisplayAsString] protected float currentComboTime;
        
        private RangeAttackPattern? CurrentPattern => rangeAttackPatterns[currentPatternIndex];
        private RangeAttackPattern? PreviousPattern
        {
            get
            {
                if (previousPatternIndex == -1) return null;
                return rangeAttackPatterns[previousPatternIndex];
            }
        }
        
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            if (PlayerInput.RightMouseClick.Value.isDown)
            {
                OnAttack();
            }
        }
        
        protected override void UpdateModule()
        {
            UpdateCooldown();
            if (!ModulePermitted)
            {
                UpdateBasicAttackActionIcon(false);
                return;
            }
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
        }
        
        protected virtual void OnRangeHit(Collider2D collider)
        {
            if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
            var healthModule = characterHub.FindModuleOfType<CharacterHealthModule>();
            if (!healthModule || CurrentPattern == null) return;
            var damage = -CurrentPattern.Value.damage;
            /*if (criticalModule)
            {
                criticalModule.CalculateCritical(ref damage);
            }*/
            healthModule.ChangeHealth(damage);
            //GetEnergy(CurrentPattern.Value.getEnergy);
        }
        
        private void UpdateCooldown()
        {
            if (PreviousPattern != null)
            {
                if (attackReady && currentComboTime < PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime += Time.deltaTime;
                }
                if (currentComboTime >= PreviousPattern.Value.resetComboTime)
                {
                    currentComboTime = 0;
                    currentPatternIndex = 0;
                    previousPatternIndex = -1;
                }
            }
            var pattern = PreviousPattern ?? CurrentPattern;
            if (pattern == null) return;
            if (!attackReady && currentInterval < pattern.Value.interval)
            {
                currentInterval += Time.deltaTime;
            }
            else
            {
                attackReady = true;
            }
            bool available = !attackUsed;
            UpdateBasicAttackActionIcon(available, pattern);
        }
        
        private void UpdateBasicAttackActionIcon(bool available, RangeAttackPattern? pattern = null)
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            if (pattern != null)
            {
                float max = pattern.Value.interval;
                //PlayerCanvasManager.Instance.UpdateBasicAttackIcon(currentInterval, max);
            }
            //PlayerCanvasManager.Instance.SetAvailableBasicAttack(available);
        }
        
        protected override void OnAttack()
        {
            if (!ModulePermitted) return;
            if (!attackReady) return;
            _ = AttackAsync();
        }
        
        public virtual void SetAttackDirection(Vector2 direction)
        {
            if (!ModulePermitted) return;
            direction.Normalize();
            comboParent.right = direction;
        }
        
        private void SpawnProjectile()
        {
            ProjectileDamageArea projectileDamageArea = 
                Instantiate(CurrentPattern.Value.projectileDamageAreaPrefab, CurrentPattern.Value.firePoint.transform.position, Quaternion.identity);
            projectileDamageArea.SetPassThroughLayer(CurrentPattern.Value.passThroughLayer);
            projectileDamageArea.SetDirection(CurrentPattern.Value.firePoint.right, CurrentPattern.Value.projectileSpeed);
        }
        
        protected async UniTask AttackAsync()
        {
            if (CurrentPattern == null) return;

            attackUsed = true;
            currentComboTime = 0;
            characterHub.ChangeActionState(CharacterActionState.RangeAttacking);

            await UniTask.Delay(TimeSpan.FromSeconds(CurrentPattern.Value.delay));

            SpawnProjectile();

            var projectileDamageArea = GameObject.Instantiate(
                CurrentPattern.Value.projectileDamageAreaPrefab,
                CurrentPattern.Value.firePoint.transform.position,
                Quaternion.identity
            );

            projectileDamageArea.SetPassThroughLayer(CurrentPattern.Value.passThroughLayer);
            projectileDamageArea.SetDirection(CurrentPattern.Value.firePoint.right, CurrentPattern.Value.projectileSpeed);
            projectileDamageArea.OnHitEvent += OnRangeHit;

            if (CurrentPattern.Value.hasDuration)
            {
                projectileDamageArea.Initialize();
                projectileDamageArea.SetActive(true);
                await UniTask.Delay(TimeSpan.FromSeconds(CurrentPattern.Value.duration));
                projectileDamageArea.SetActive(false);
            }

            characterHub.ChangeActionState(CharacterActionState.None);
            previousPatternIndex = currentPatternIndex;
            currentPatternIndex = (currentPatternIndex + 1) % rangeAttackPatterns.Count;
            currentInterval = 0;
            attackReady = false;
            attackUsed = false;
        }
    }
}
