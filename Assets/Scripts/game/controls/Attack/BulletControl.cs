using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletControl : AttackControl
{
    public float attackDamage;
    public float moveSpeed;
    public float distance;

    private Rigidbody _rigidbody;
    private Vector3 _originalPosition;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if ((transform.position - _originalPosition).magnitude > distance)
        {
            BulletOutOfRange();
        }
    }

    public override void Attack(Entity attacker, Vector3 targetPosition)
    {
        base.Attack(attacker, targetPosition);

        Vector3 direction = (targetPosition - transform.position).normalized;
        _rigidbody.velocity = direction * moveSpeed;
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

    private void OnCollisionEnter(Collision other)
    {
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