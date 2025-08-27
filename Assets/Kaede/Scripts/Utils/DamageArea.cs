using Kaede.Scripts.Utils.Others;
using Sirenix.OdinInspector;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class DamageArea : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] protected LayerMask targetLayer;
    protected Collider2D DamageCollider;

    public delegate void OnHit(Collider2D collider);
    public event OnHit OnHitEvent;

    public void Initialize()
    {
        if (!TryGetComponent(out DamageCollider))
        {
            Debug.LogError("No Collider2D found on DamageArea");
            return;
        }
        DamageCollider.isTrigger = true;
    }
    
    protected virtual void Start()
    {
        DamageCollider = GetComponent<Collider2D>();
        DamageCollider.isTrigger = true;
    }

    protected virtual void OnDisable()
    {
        OnHitEvent = null;
    }

    public virtual void SetActive(bool active)
    {
        if (!DamageCollider) DamageCollider = GetComponent<Collider2D>();
        DamageCollider.enabled = active;
    }
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
        {
            OnHitEvent?.Invoke(other);
        }
    }
}
