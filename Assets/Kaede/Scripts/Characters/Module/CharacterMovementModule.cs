using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterMovementModule : CharacterModule
{
    [Title("References")]
    [SerializeField] private Rigidbody2D rb2d;
    public Rigidbody2D Rb2d => rb2d;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    
    [Title("Movement Setting")]
    [SerializeField] private float movementSpeed;

    [Title("Movement Debug")]
    [SerializeField, ReadOnly] protected Vector2 moveDirection;
    public Vector2 MoveDirection { get => moveDirection; set => moveDirection = value; }

    
    protected override void UpdateModule()
    {
        base.UpdateModule();
        rb2d.linearVelocity = moveDirection * movementSpeed;
    }
    
    protected void LateUpdate()
    {
        LateUpdateModule();
    }
    
    protected override void LateUpdateModule()
    {
        /*if (characterHub.ActionState == CharacterActionState.None)
        {
            moveDirection = Vector2.zero;
        }
            
        if (moveDirection.magnitude <= 0)
        {
            rb2d.linearVelocity = Vector2.zero;
        }*/
        base.LateUpdateModule();
    }
    
    public void SetDirection(Vector2 direction, bool forceSet = false)
    {
        if (!ModulePermitted && !forceSet) return;
        moveDirection = direction;
        moveDirection.Normalize();
        if (characterHub.MovementState == CharacterMovementState.Dashing) return;
        var state = moveDirection.magnitude > 0
            ? CharacterMovementState.Walking
            : CharacterMovementState.Idle;
        characterHub.ChangeMovementState(state);
    }
        
    public void SetPosition(Vector2 position)
    {
        if (!ModulePermitted) return;
        rb2d.position = position;
    }
        
    protected override void HandleInput()
    {
        if (characterHub.CharacterType is not CharacterType.Player) return;
        base.HandleInput();
        SetDirection(PlayerInput.MovementInput);
    }
}
