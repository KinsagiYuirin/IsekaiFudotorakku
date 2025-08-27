using System;
using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Characters.Module
{
    [Serializable]
    public record HealthData
    {
        public float currentHealth;
        public float maxHealth;
        public bool invincible;
    }
    /// <summary>
    /// Module responsible for handling character health.
    /// </summary>
    public class CharacterHealthModule : CharacterModule, IDamageable
    {
        [Title("Health Settings")] 
        [SerializeField] private DamageType receiveDamageType;
        [SerializeField] private HealthData healthData = new HealthData();
        public HealthData pHealthData => healthData;
        [SerializeField] private float bumpThreshold = 10f;
        [SerializeField] private bool useHealthBar = true;
        
        [SerializeField] private GameObject healthScreenUI;
        [SerializeField] private GameObject characterObject;
        
        [SerializeField] private Animator deadAnimator;
        
        [SerializeField] private SpriteRenderer spriteImage;
        [SerializeField] private Color _redColor = Color.red;
        [SerializeField] private Color _whiteColor = Color.white;
        
        [Title("Armor Settings")]
        [SerializeField] private bool haveArmor;
        [SerializeField, ShowIf("haveArmor")] private CharacterArmorModule armorModule;
        
        /*
        [SerializeField, ShowIf(nameof(useMMHealthBar))] 
        private MMHealthBar healthBar;
        */

        [Title("Debug")] 
        [SerializeField, DisplayAsString] private bool iFrame;
        public bool IFrame {get => iFrame; set => iFrame = value; }
        
        [SerializeField] private float testAmount;
        [Button("Test Change Health")] 
        private void TestChangeHealth() => ChangeHealth(testAmount);
        private float _previousChange;

        private void Start()
        {
            healthData.currentHealth = healthData.maxHealth;
            iFrame = false;
        }

        private void OnHealthDataChanged(HealthData previousvalue, HealthData newvalue)
        {
            _previousChange = newvalue.currentHealth - previousvalue.currentHealth;
            //UpdateHealthBar();
        }
        
        public void ReceiveDamage(float amount, DamageData data)
        {
            if (data.type != receiveDamageType)
            {
                ChangeHealth(amount);
            }
        }

        public virtual void ChangeHealth(float amount)
        {
            if (iFrame) return;
            if (!ModulePermitted) return;
            if (healthData.invincible) return;
            //if (characterHub.ConditionState == CharacterConditionState.Armor) return;
            if (haveArmor)
                if (armorModule.HaveArmor)
                    return;
            
            _previousChange = amount;
            healthData.currentHealth += amount;
            healthData.currentHealth = Mathf.Clamp(healthData.currentHealth, 0, healthData.maxHealth);
            if (healthData.currentHealth <= 0)
            {
                Die();
            }
            if (amount < 0) // โดนดาเมจ
            {
                
            }
            UpdateHealthBar();
        }

        public void UpdateHealthBar()
        {
            if (!useHealthBar) return;
        }
        
        
        /// <summary>
        /// Need to fix this method later.
        /// This method is called when the character dies.
        /// </summary>
        protected virtual void Die()
        {
            if (!ModulePermitted) return;
            characterHub.ChangeConditionState(CharacterConditionState.Dead);
            characterObject.layer = LayerMask.NameToLayer("Dead");
            characterObject.GetComponent<Collider2D>().enabled = false;
            characterObject.GetComponent<Rigidbody2D>().simulated = false;
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            if (deadAnimator == null) return;
        }
    }
}
