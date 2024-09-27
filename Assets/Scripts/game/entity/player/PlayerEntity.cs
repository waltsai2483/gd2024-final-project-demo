using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerEntity : Entity
{
    public AttackIndicator attackIndicator;
    
    private Vector2 _inputDirection;

    private bool _isHoldingAttack = false;

    public override bool isAttacking
    {
        get => _isHoldingAttack;
    }

    public Vector3 mouseDirection { get; private set; }

    private readonly StatModifier _attackSlownessModifier = new(StatOperationType.AddPercent, -0.25f);

    protected override void Update()
    {
        base.Update();
        HandleMovement(_inputDirection);

        Vector3 mousePos = CameraControl.instance.GetMousePos();
        if (_isHoldingAttack)
        {
            Vector2 xzVect = new Vector2(mousePos.x - transform.position.x, mousePos.z - transform.position.z);
            if (xzVect.magnitude > attackIndicator.viewDistance)
            {
                mousePos = new Vector3(transform.position.x + xzVect.normalized.x * attackIndicator.viewDistance, mousePos.y, transform.position.z + xzVect.normalized.y * attackIndicator.viewDistance);
            }
            Attack(mousePos);
        }

        attackIndicator.origin = transform.position;
        attackIndicator.targetPosition = mousePos;
    }

    public void HandleInputs(InputAction.CallbackContext context)
    {
        _inputDirection = context.ReadValue<Vector2>();
    }

    public void HandleAttack(InputAction.CallbackContext context)
    {
        _isHoldingAttack = context.performed;
    }

    protected override void OnAttackBegin()
    {
        stats.AddModifier(StatType.MovingSpeed, _attackSlownessModifier); // -25% Speed while attacking
    }

    protected override void OnAttackEnd()
    {
        stats.RemoveModifier(StatType.MovingSpeed, _attackSlownessModifier);
    }
}