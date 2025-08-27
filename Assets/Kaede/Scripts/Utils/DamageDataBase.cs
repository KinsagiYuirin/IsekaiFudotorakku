using System;
using Kaede.Scripts.Characters.Module;
using MadDuck.Scripts.Characters;
using MadDuck.Scripts.Characters.Modules;
using UnityEngine;

[Flags]
public enum DamageType
{
    None,
    Melee,
    Range
}

public struct DamageData
{
    public DamageType type;
}

public interface IDamageable
{
    void ReceiveDamage(float amount, DamageData data);
}

public abstract class DamageDataBase : CharacterModule
{
    
    protected override void HandleInput()
    {
        if (characterHub.CharacterType is not CharacterType.Player) return;
        base.HandleInput();
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
    
    public override void Initialize(CharacterHub characterHub)
    {
        base.Initialize(characterHub);
    }
    
    public override void Shutdown()
    {
        base.Shutdown();
    }
    
    /// <summary>
    /// Method called when the damage area hits a collider.
    /// </summary>
    /// <param name="collider">Collider that was hit.</param>
    protected virtual void OnHit(Collider2D collider)
    {
        if (!collider.TryGetComponent(out CharacterHub characterHub)) return;
    }
    
    protected virtual void OnAttack()
    {
        if (!ModulePermitted) return;
    }
}
