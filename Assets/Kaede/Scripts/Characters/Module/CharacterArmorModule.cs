using System;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Characters.Module
{
    [Serializable]
    public record ArmorData
    {
        public float currentArmor;
        public float maxArmor;
        public bool invincible;
    }
    /// <summary>
    /// Module responsible for handling character health.
    /// </summary>
    public class CharacterArmorModule : CharacterModule, IDamageable
    {
        [Title("Health Settings")] 
        [SerializeField] private DamageType receiveDamageType;
        [SerializeField] private DamageType receiveHaftDamageType;
        [SerializeField] private ArmorData armorData = new ArmorData();
        public ArmorData PArmorData => armorData;
        [SerializeField] private float bumpThreshold = 10f;
        [SerializeField] private bool useHealthBar = true;
        
        [SerializeField] private GameObject armorScreenUI;
        [SerializeField] private GameObject characterObject;
        
        [SerializeField] private bool haveArmor;
        public bool HaveArmor => haveArmor;
        
        [SerializeField] private Animator animation;
        
        /*
        [SerializeField, ShowIf(nameof(useMMHealthBar))] 
        private MMHealthBar healthBar;
        */

        [Title("Debug")] 
        [SerializeField] 
        private float testAmount;
        [Button("Test Change Armor")] 
        private void TestChangeArmor() => ChangeArmor(testAmount);
        private float _previousChange;

        private void Start()
        {
            GetArmor();
        }

        public void GetArmor()
        {
            armorData.currentArmor = armorData.maxArmor;
            haveArmor = true;
            //characterHub.ChangeConditionState(CharacterConditionState.Armor);
        }
        
        private void OnHealthDataChanged(HealthData previousvalue, HealthData newvalue)
        {
            _previousChange = newvalue.currentHealth - previousvalue.currentHealth;
            //UpdateHealthBar();
        }

        public void ReceiveDamage(float amount, DamageData data)
        {
            if (data.type == receiveDamageType)
            {
                if (!haveArmor) return;
                ChangeArmor(amount);
            }
            else if (data.type == receiveHaftDamageType)
            {
                if (!haveArmor) return;
                ChangeArmor(amount / 2);
            }
        }

        public virtual void ChangeArmor(float amount)
        {
            if (!ModulePermitted) return;
            if (armorData.invincible) return;
            //if (characterHub.ConditionState != CharacterConditionState.Armor) return;
            
            _previousChange = amount;
            armorData.currentArmor += amount;
            armorData.currentArmor = Mathf.Clamp(armorData.currentArmor, 0, armorData.maxArmor);
            if (armorData.currentArmor <= 0)
            {
                IsArmorBroken();
            }
            UpdateHealthBar();
        }

        public void UpdateHealthBar()
        {
            if (!useHealthBar) return;
        }
        
        protected virtual void IsArmorBroken()
        {
            if (!ModulePermitted) return;
            //characterHub.ChangeConditionState(CharacterConditionState.Normal);
            haveArmor = false;
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            if (animation == null) return;
            //animation.SetBool("IsDead", characterHub.ConditionState == CharacterConditionState.Dead);
        }
    }
}
