using System;
using System.Collections.Generic;
using System.Linq;
using MadDuck.Scripts.Characters.Modules;
using MadDuck.Scripts.Inputs;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace MadDuck.Scripts.Characters
{
    [Flags] public enum CharacterType
    {
        Player = 1 << 0,
        NPC = 1 << 1
    }
    
    [Flags] public enum FacingDirection
    {
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3
    }
    
    /// <summary>
    ///  Character hub is the main class that manages the character and its modules.
    /// </summary>
    [TypeInfoBox("Character Hub is the main class that manages the character and its modules.")]
    public class CharacterHub : MonoBehaviour
    {
        #region Inspector
        [Title("Character Settings")] 
        [field: SerializeField, 
                PropertyTooltip("Character type, used to determine if this character is a player or an NPC.")]
        public CharacterType CharacterType { get; private set; }
        
        [Title("Debug")]
        [field: SerializeField, ReadOnly, PropertyTooltip("List of active modules in this character.")]
        [Inject] private List<CharacterModule> modules = new();
        [field: SerializeField, DisplayAsString, PropertyTooltip("Current movement state of the character.")] 
        public CharacterMovementState MovementState { get; private set; } = CharacterMovementState.Idle;
        [field: SerializeField, DisplayAsString, PropertyTooltip("Current action state of the character.")]
        public CharacterActionState ActionState { get; private set; } = CharacterActionState.None;
        [field: SerializeField, DisplayAsString, PropertyTooltip("Current condition state of the character.")] 
        public CharacterConditionState ConditionState { get; private set; } = CharacterConditionState.Normal;
        [ShowInInspector, DisplayAsString, PropertyTooltip("Current facing direction of the character.")] 
        public FacingDirection FacingDirection => facingDirection.Value;
        #endregion
        
        #region Properties
        public PlayerInputHandler PlayerInput { get; private set; }
        public bool Initialized { get; private set; }
        public readonly ReactiveProperty<FacingDirection> facingDirection = new(FacingDirection.Right);
        private IPublisher<MovementStateEvent> _movementStatePublisher;
        private IPublisher<ActionStateEvent> _actionStatePublisher;
        private IPublisher<ConditionStateEvent> _conditionStatePublisher;
        #endregion

        #region Life Cycle
        private void OnEnable()
        {
            if (Initialized) return;
            Initialized = true;
            Initialize();
            Subscribe();
        }

        /// <summary>
        /// Initializes the character hub and its modules.
        /// </summary>
        protected virtual void Initialize()
        {
            PlayerInput = GetComponent<PlayerInputHandler>();
            if (!PlayerInput)
            {
                Debug.LogError("PlayerInput is not found on CharacterHub!");
                return;
            }
            _movementStatePublisher = GlobalMessagePipe.GetPublisher<MovementStateEvent>();
            _actionStatePublisher = GlobalMessagePipe.GetPublisher<ActionStateEvent>();
            _conditionStatePublisher = GlobalMessagePipe.GetPublisher<ConditionStateEvent>();
            if (!PlayerInput && CharacterType == CharacterType.Player)
            {
                Debug.LogError($"{nameof(PlayerInput)} component not found in player object.");
            }
            foreach (var module in modules)
            {
                module.Initialize(this);
            }
        }

        /// <summary>
        /// Subscribes to the events that the character hub needs to listen to.
        /// </summary>
        protected virtual void Subscribe()
        {
            
        }


        /// <summary>
        /// Shuts down the character hub and its modules.
        /// </summary>
        protected virtual void Shutdown()
        {
            foreach (var module in modules)
            {
                module.Shutdown();
            }
            modules.Clear();
        }

        /// <summary>
        /// Unsubscribes from the events that the character hub was listening to.
        /// </summary>
        protected virtual void Unsubscribe()
        {
            
        }

        private void OnDisable()
        {
            if (!Initialized) return;
            Shutdown();
            Unsubscribe();
            Initialized = false;
        }
        #endregion

        #region State Change
        public void ChangeMovementState(CharacterMovementState newState)
        {
            if (newState == MovementState) return;
            _movementStatePublisher.Publish(new MovementStateEvent(this, MovementState, newState));
            MovementState = newState;
        }
        
        public void ChangeActionState(CharacterActionState newState)
        {
            if (newState == ActionState) return;
            _actionStatePublisher.Publish(new ActionStateEvent(this, ActionState, newState));
            ActionState = newState;
        }
        
        public void ChangeConditionState(CharacterConditionState newState)
        {
            if (newState == ConditionState) return;
            _conditionStatePublisher.Publish(new ConditionStateEvent(this, ConditionState, newState));
            ConditionState = newState;
        }

        public void ChangeFacingDirection(FacingDirection direction, bool forceOnNext = false)
        {
            facingDirection.Value = direction;
            if (forceOnNext)
            {
                facingDirection.OnNext(direction);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Finds a module of the specified type in the character hub.
        /// </summary>
        /// <typeparam name="T">Type of the module to find.</typeparam>
        /// <returns>The module of the specified type if found, null otherwise.</returns>
        public T FindModuleOfType<T>() where T : CharacterModule
        {
            return modules.Where(module => module is T).Cast<T>().FirstOrDefault();
        }
        #endregion
    }
}
