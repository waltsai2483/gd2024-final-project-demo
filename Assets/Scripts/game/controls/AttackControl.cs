using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackControl : MonoBehaviour
{
    public Entity attacker { get; protected set; }
    
    public virtual void Attack(Entity attacker, Vector3 targetPosition)
    {
        this.attacker = attacker;
    }
}
