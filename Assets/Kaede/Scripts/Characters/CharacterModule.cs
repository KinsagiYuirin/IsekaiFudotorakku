using System;
using System.Collections.Generic;
using MadDuck.Scripts.Frameworks;
using MadDuck.Scripts.Frameworks.MessagePipe;
using MadDuck.Scripts.Inputs;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using DisposableBag = MessagePipe.DisposableBag;

namespace MadDuck.Scripts.Characters.Modules
{
    /// <summary>
    /// Inheritable character module class
    /// </summary>
    public abstract class CharacterModule : MonoBehaviour
    {
        #region Inspector
        [Title("Base Settings")]
        [SerializeField, PropertyOrder(-10),
        PropertyTooltip("Is this module enabled?")] 
        protected SerializableReactiveProperty<bool> moduleEnabled = new(true);
        [SerializeField, PropertyOrder(-10),
        PropertyTooltip("List of movement states that block this module")] 
        protected List<CharacterMovementState> blockedMovementStates = new();
        [SerializeField, PropertyOrder(-10),
        PropertyTooltip("List of action states that block this module")] 
        protected List<CharacterActionState> blockedActionStates = new();
        [SerializeField, PropertyOrder(-10),
        PropertyTooltip("List of condition states that block this module")] 
        protected List<CharacterConditionState> blockedConditionStates = new();

        [Title("Base Debug")]
        [ShowInInspector, DisplayAsString, PropertyOrder(-10),
         PropertyTooltip(
             "Is this module permitted? (Module is permitted if it is enabled and the character is not in a blocked state)")]
        protected bool ModulePermitted {get => modulePermitted.Value; set => modulePermitted.Value = value;}
        
        protected readonly ReactiveProperty<bool> modulePermitted = new();

        #endregion

        #region Properties
        protected CharacterHub characterHub;
        protected bool initialized;
        public bool ModuleEnabled { get => moduleEnabled.Value; set => moduleEnabled.Value = value; }
        public CharacterHub CharacterHub => characterHub;
        public PlayerInputHandler PlayerInput => characterHub.PlayerInput;
        private bool _movementStateBlocked;
        private bool _actionStateBlocked;
        private bool _conditionStateBlocked;
        private IDisposable _characterStateListener;    
        private IDisposable _moduleEnabledListener;
        private IDisposable _modulePermissionListener;
        #endregion

        #region Unity Methods
        /// <summary>
        /// Unity update method. Note: It is not recommended to override this method. Use UpdateModule instead.
        /// </summary>
        protected virtual void Update()
        {
            if (!ModulePermitted) return;
            HandleInput();
            UpdateModule();
        }

        /// <summary>
        /// Unity fixed update method. Note: It is not recommended to override this method. Use FixedUpdateModule instead.
        /// </summary>
        protected void FixedUpdate()
        {
            if (!ModulePermitted) return;
            FixedUpdateModule();
        }

        /// <summary>
        /// Unity late update method. Note: It is not recommended to override this method. Use LateUpdateModule instead.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!ModulePermitted) return;
            LateUpdateModule();
        }
        #endregion

        #region Life Cycle
        /// <summary>
        /// Initialize module
        /// </summary>
        public virtual void Initialize(CharacterHub characterHub)
        {
            this.characterHub = characterHub;
            Subscribe();
            _movementStateBlocked = blockedMovementStates.Contains(CharacterHub.MovementState);
            _actionStateBlocked = blockedActionStates.Contains(CharacterHub.ActionState);
            _conditionStateBlocked = blockedConditionStates.Contains(CharacterHub.ConditionState);
            
            UpdatePermission();
        }

        /// <summary>
        /// Shutdown module
        /// </summary>
        public virtual void Shutdown()
        {
            Unsubscribe();
        }
        #endregion

        #region Event Subscriptions
        protected virtual void Subscribe()
        {
            var movementSubscriber = GlobalMessagePipe.GetSubscriber<MovementStateEvent>();
            var movementFilter = new ObjectIdentifierFilter<MovementStateEvent, CharacterHub>(characterHub);
            var actionSubscriber = GlobalMessagePipe.GetSubscriber<ActionStateEvent>();
            var actionFilter = new ObjectIdentifierFilter<ActionStateEvent, CharacterHub>(characterHub);
            var conditionSubscriber = GlobalMessagePipe.GetSubscriber<ConditionStateEvent>();
            var conditionFilter = new ObjectIdentifierFilter<ConditionStateEvent, CharacterHub>(characterHub);
            var bag = DisposableBag.CreateBuilder();
            bag.Add(movementSubscriber.Subscribe(OnMovementStateEvent, movementFilter));
            bag.Add(actionSubscriber.Subscribe(OnActionStateEvent, actionFilter));
            bag.Add(conditionSubscriber.Subscribe(OnConditionStateEvent, conditionFilter));
            _characterStateListener = bag.Build();
            _modulePermissionListener = modulePermitted.Subscribe(OnPermissionChanged);
            _moduleEnabledListener = moduleEnabled.Subscribe(OnModuleEnabledChanged);
        }
        
        protected virtual void Unsubscribe()
        {
            _characterStateListener?.Dispose();
            _modulePermissionListener?.Dispose();
            _moduleEnabledListener?.Dispose();
        }
        
        protected virtual void OnPermissionChanged(bool value)
        {
            
        }
        
        private void OnModuleEnabledChanged(bool value)
        {
            UpdatePermission();
        }
        
        private void OnMovementStateEvent(MovementStateEvent eventData)
        {
            _movementStateBlocked = blockedMovementStates.Contains(eventData.newState);
            UpdatePermission();
        }
        
        private void OnActionStateEvent(ActionStateEvent eventData)
        {
            _actionStateBlocked = blockedActionStates.Contains(eventData.newState);
            UpdatePermission();
        }
        
        private void OnConditionStateEvent(ConditionStateEvent eventData)
        {
            _conditionStateBlocked = blockedConditionStates.Contains(eventData.newState);
            UpdatePermission();
        }
        #endregion
        
        #region Input
        /// <summary>
        /// Handle player input
        /// </summary>
        protected virtual void HandleInput()
        {
        
        }
        #endregion

        #region Update Module
        /// <summary>
        /// Update module using standard update rate
        /// </summary>
        protected virtual void UpdateModule()
        {
            
        }
        
        /// <summary>
        /// Update module using fixed update rate
        /// </summary>
        protected virtual void FixedUpdateModule()
        {
            
        }
        
        /// <summary>
        /// Update module using late update rate
        /// </summary>
        protected virtual void LateUpdateModule()
        {
            UpdateAnimator();
        }

        /// <summary>
        /// Update animator
        /// </summary>
        protected virtual void UpdateAnimator()
        {
            
        }
        #endregion

        protected virtual void UpdatePermission()
        {
            ModulePermitted = characterHub &&
                              characterHub.Initialized &&
                              !_movementStateBlocked &&
                              !_actionStateBlocked &&
                              !_conditionStateBlocked &&
                              moduleEnabled.Value;
        }
    }
}
