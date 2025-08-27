using System;
using Kaede.Scripts.Utils.Others;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileDamageArea : DamageArea
{
    private float speed;
    private Vector3 direction;
    [SerializeField] private LayerMask passThroughLayer;

    private void Update()
    {
        transform.position += direction * (speed * Time.deltaTime); 
        //rotate to the direction
        if (direction == Vector3.zero) return;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    public override void SetActive(bool active)
    {
        if (!DamageCollider) DamageCollider = GetComponent<Collider2D>();
    }
    
    public void SetPassThroughLayer(LayerMask layer)
    {
        passThroughLayer = layer;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        
        
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, passThroughLayer)) return;
        if (LayerMaskUtils.IsInLayerMask(other.gameObject.layer, targetLayer))
            Destroy(gameObject);
        
    }
    
    public void SetDirection(Vector3 dir, float projectileSpeed)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
    }
}
