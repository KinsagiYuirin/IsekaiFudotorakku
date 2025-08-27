using System;
using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Characters.Module
{
    [Flags]
    public enum FacingDirection
    {
        Left = 1 << 0,
        Right = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3
    }
    
    public class CharacterMovementModule : CharacterModule
    {
        #region Inspector
        [Title("References")] 
        [SerializeField] private Rigidbody2D rb2d;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Title("Movement Setting")] 
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float movementThreshold = 0.1f;

        [Title("Movement Debug")] [SerializeField, ReadOnly]
        Vector2 moveDirection;
        #endregion

        public Vector2 MoveDirection
        {
            get => moveDirection;
            set => moveDirection = value;
        }

        #region Life Cycles

        public override void Shutdown()
        {
            base.Shutdown();
            SetDirection(Vector2.zero);
            SetVelocity(Vector2.zero);
        }

        #endregion

        #region Events
        protected override void Subscribe()
        {
            base.Subscribe();
        }
        
        protected override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        protected override void OnPermissionChanged(bool value)
        {
            if (value) return;
            SetDirection(Vector2.zero);
            SetVelocity(Vector2.zero);
        }
        #endregion
        
        protected override void HandleInput()
        {
            if (characterHub.CharacterType is not CharacterType.Player) return;
            base.HandleInput();
            SetDirection(PlayerInput.MovementInput);
        }
        
        protected override void UpdateModule()
        {
            if (!ModulePermitted)
            {
                SetDirection(Vector3.zero);
                return;
            }
            base.UpdateModule();
            SetVelocity(moveDirection * movementSpeed);
        }

        protected void LateUpdate()
        {
            base.LateUpdate();
            LateUpdateModule();
        }

        protected override void LateUpdateModule()
        {
            if (!ModulePermitted) return;
            if (moveDirection.magnitude <= 0)
            {
                SetVelocity(Vector2.zero);
            }
            UpdateDirection();
            base.LateUpdateModule();
        }

        private void UpdateDirection()
        {
            
        }
        
        public void SetDirection(Vector2 direction)
        {
            moveDirection = direction;
            moveDirection.Normalize();
            var state = moveDirection.magnitude > 0
                ? CharacterMovementState.Walking
                : CharacterMovementState.Idle;
            characterHub.ChangeMovementState(state);
        }

        public void SetPosition(Vector2 position)
        {
            rb2d.position = position;
        }

        public void SetVelocity(Vector2 velocity)
        {
            rb2d.linearVelocity = velocity;
        }
    }
}
