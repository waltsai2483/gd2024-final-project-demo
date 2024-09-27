using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : AttackControl
{
    public float attackDamage;
    public float moveSpeed;
    public float distance;

    private float _travelDistance;

    private void Start()
    {
    }

    private void Update()
    {
        if (_travelDistance > distance)
        {
            BulletOutOfRange();
        }
        _travelDistance += moveSpeed * Time.deltaTime;
    }

    public override void Attack(Entity attacker, Vector3 targetPosition)
    {
        base.Attack(attacker, targetPosition);
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        GetComponent<Rigidbody>().velocity = direction * moveSpeed;
    }

    protected virtual void BulletOutOfRange()
    {
        Destroy(gameObject);
    }

    protected virtual void BulletHitBlock(GameObject hitObject)
    {
        Destroy(gameObject);
    }

    protected virtual void BulletHitEntity(Entity hitEntity)
    {
        hitEntity.Damage(attacker, attackDamage * attacker.stats.attackDamageMultiplier);
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.layer);
        if (other.gameObject.layer == LayerMask.NameToLayer("Entity"))
        {
            BulletHitEntity(other.gameObject.GetComponent<Entity>());
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            BulletHitBlock(other.gameObject);
        }
    }
}