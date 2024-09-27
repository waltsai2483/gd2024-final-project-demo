using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileControl : AttackControl
{
    public float projectDuration = 1;
    public float projectionMaxHeight = 1;

    private Rigidbody _rigidbody;

    private Vector3 _origin;
    private Vector3 _targetPosition;
    private Vector2 _direction;
    private float _lifetime;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _lifetime += Time.deltaTime;
        float normalized = _lifetime / projectDuration; // 0 -> 1

        if (normalized >= 1)
        {
            onAttackHit(transform.position);
            Destroy(gameObject);
            return;
        }
        
        float yDist = _targetPosition.y - _origin.y;
        float xDist = (1 + Mathf.Sqrt(1 + Mathf.Abs(yDist / projectionMaxHeight))) / 2;
        
        float yOffset = Mathf.Max(yDist, 0);

        float xOffset = yDist > 0 ? 1 - normalized : normalized;
        float operand = 2 * xOffset * xDist - 1;
            
        float y = -projectionMaxHeight * operand * operand + projectionMaxHeight + yOffset;
        
        transform.position = _origin + new Vector3(_direction.x * normalized, y, _direction.y * normalized);
    }

    public virtual void onAttackHit(Vector3 hitPosition)
    {
        
    }

    public override void Attack(Entity attacker, Vector3 targetPosition)
    {
        base.Attack(attacker, targetPosition);
        _origin = transform.position;
        _targetPosition = targetPosition;
        _direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.z - transform.position.z);
    }
}
