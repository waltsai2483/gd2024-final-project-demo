using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class TurretEntity : Entity
{
    private float targetDetectionAge = 0;
    private Entity targetEntity;

    protected override void Update()
    {
        base.Update();
        targetDetectionAge += Time.deltaTime;

        if (targetEntity)
        {
            characterModel.transform.LookAt(targetEntity.transform);
            print(characterModel.transform.eulerAngles.y);
        }

        if (targetDetectionAge < 1) return;
        targetDetectionAge = 0;

        targetEntity = null;
        foreach (var entity in FindObjectsByType<Entity>(FindObjectsSortMode.None))
        {
            if (entity == this) continue;
            
            Physics.Raycast(transform.position, (entity.transform.position - transform.position).normalized, out RaycastHit hit, 10.0f, LayerMask.GetMask("Block"));
            float len = (entity.transform.position - transform.position).magnitude;
            if (!targetEntity)
            {
                if (len < 10f && !hit.collider) targetEntity = entity;
            }
            else if (len < 10f && (targetEntity.transform.position - transform.position).magnitude > len && !hit.collider)
            {
                targetEntity = entity;
            }
        }

        if (targetEntity)
        {
            Attack(targetEntity.transform.position);
        }
    }
}