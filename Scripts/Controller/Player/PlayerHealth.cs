using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : BaseHittableObject
{
    
    public float PlayerGrenadeDamageMul = 2f;

    public override void TakeDamage(DamageData damage)
    {
        if(damage.HitType == DamageData.DamageType.Explosion)
        {
            damage.DamageAmount *= PlayerGrenadeDamageMul;
        }
        base.TakeDamage(damage);
    }

    public override void Die(DamageData damage)
    {        
        base.Die(damage);

        GameManager.Instance.EndGameEvent.Invoke();
    }
}
