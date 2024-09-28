using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

public class Entity : MonoBehaviour
{
    [SerializeField] public EntityStats stats;
    public Animator animator;

    public GameObject characterModel;
    public ParticleSystem walkingParticles;

    public AttackControl attackPrefab;
    
    public bool canMove = true;
    public bool forceRotate = true;

    protected CharacterController entityController;
    protected Rigidbody rigidBody;
    protected int walkParticleEmissionRate = 3;
    
    public Vector2 attackDirection { get; protected set; } = Vector2.zero;
    
    private bool _isAttacking;
    private float _endAttackTime = 0;

    private Vector2 _movementDirection = Vector2.zero;

    private float _yVelocity;
    
    public virtual bool isAttacking
    {
        get => _isAttacking;
    }

    protected virtual void Start()
    {
        entityController = GetComponent<CharacterController>();
        rigidBody = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR
        stats.UpdateStats();
#endif
        // movement update
        float multiplier = GameManager.instance.normalMovingSpeed * stats.movingSpeedRatio * Time.deltaTime;
        _yVelocity = entityController.isGrounded
            ? 0.0f
            : _yVelocity - GameManager.instance.baseGravity * Time.deltaTime;
        entityController.Move(new Vector3(_movementDirection.x * multiplier, _yVelocity,
            _movementDirection.y * multiplier));
        
        float? destYRot = null;
        if (attackDirection != Vector2.zero)
        {
            destYRot = -Mathf.Atan2(attackDirection.y, attackDirection.x) + Mathf.PI / 2.5f;
        }
        else if (canMove && _movementDirection != Vector2.zero)
        {
            destYRot = -Mathf.Atan2(_movementDirection.y, _movementDirection.x) + Mathf.PI / 2;
        }

        if (characterModel && destYRot != null && forceRotate)
        {
            Quaternion destRot = Quaternion.Euler(0, destYRot.Value * Mathf.Rad2Deg, 0);
            float m = attackDirection != Vector2.zero || Math.Abs(Time.time - _endAttackTime) <= 0.5f
                ? 750 // need to look at attack position asap & smoothly, therefore faster
                : 300 * stats.rotateWeight; // Normal walking therefore slower & depends on rotation weight
            characterModel.transform.rotation = Quaternion.RotateTowards(characterModel.transform.rotation, destRot, m * Time.deltaTime);
        }
        // walking particle update

        if (walkingParticles)
        {
            ParticleSystem.EmissionModule psEmission = walkingParticles.emission;
            psEmission.rateOverTime = _movementDirection != Vector2.zero ? walkParticleEmissionRate * entityController.velocity.magnitude : 0;
        }
    }

    protected void LateUpdate()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator)
        {
            animator.SetFloat("Speed", _movementDirection.magnitude);
            animator.speed = _movementDirection.magnitude > 0.1 ? _movementDirection.magnitude * 1.6f : 1f;
            
            if (_isAttacking)
            {
                if (Time.time > _endAttackTime)
                {
                    _isAttacking = false;
                    OnAttackEnd();
            
                    attackDirection = Vector2.zero;
                    animator.ResetTrigger("Attack");
                    animator.SetTrigger("Attack End"); // Transited back to idle state
                }
                else
                {
                    animator.ResetTrigger("Attack End");
                }
            }
        }
    }

    protected void HandleMovement(Vector2 direction)
    {
        if (!canMove)
        {
            _movementDirection = Vector2.zero;
        }

        float turn = stats.rotateWeight * 0.5f;
        _movementDirection = (direction * turn + _movementDirection) / Math.Max(1, turn + 1f);
    }

    public void Attack(Vector3 targetPosition)
    {
        if (Time.time <= _endAttackTime) return;

        bool temp = _isAttacking;
        
        _isAttacking = true;
        _endAttackTime = Time.time + stats.attackDuration / 1000;
        attackDirection = new Vector2(targetPosition.x - transform.position.x, targetPosition.z - transform.position.z)
            .normalized;

        if (animator)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack"); // Replay attack animation again
        }

        GameObject attack = Instantiate(attackPrefab.gameObject, transform.position, Quaternion.identity);
        attack.GetComponent<AttackControl>().Attack(this, targetPosition);
        
        if (!temp)
        {
            OnAttackBegin();
        }
        OnAttackHolding();
    }

    public void Damage(Entity attacker, float damageValue)
    {
        
    }

    protected virtual void OnAttackBegin()
    {
    }

    protected virtual void OnAttackHolding()
    {
    }
    
    protected virtual void OnAttackEnd()
    {
    }
}