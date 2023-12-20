using UnityEngine;
using UnityEngine.Events;

public interface IHittableObject
{
    void TakeDamage(DamageData damage);
}

public abstract class BaseHittableObject : MonoBehaviour, IHittableObject
{
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
        }
    }

    public float StartHealth
    {
        get
        {
            return startHealth;
        }
        set
        {
            startHealth = value;
        }
    }

    public float HealthFraction
    {
        get
        {
            return health / startHealth;
        }
    }

    public bool Invulnerable = false;

    public DamageEvent DamagedEvent = new DamageEvent();
    public DamageEvent DieEvent = new DamageEvent();

    public bool IsAlive
    {
        get
        {
            return isAlive;
        }
    }

    public bool RealIsAlive
    {
        get
        {
            return HealthFraction > 0;
        }
    }

    protected bool isAlive = true;
    [SerializeField]
    float health = 100f;
    float oldHealth = 0f;
    float startHealth;

    void Awake()
    {
        startHealth = health;
        oldHealth = health;
    }

    public virtual void TakeDamage(DamageData damage)
    {
        damage.Receiver = this;

        if(!Invulnerable)
            health -= damage.DamageAmount;

        if (damage.Deadly)
            health = 0;

        if(health <= 0)
        {
            Die(damage);
        }
        else
        {
            if(DamagedEvent != null)
            {
                DamagedEvent.Invoke(damage);
            }
        }
    }        

    protected virtual void SetHealth(float health, float startHealth)
    {
        this.health = health;
        this.startHealth = startHealth;
    }

    public virtual void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, StartHealth);
        if (health > 0)
            isAlive = true;
    }

    public virtual void Die(DamageData damage)
    {
        health = 0;
        if (IsAlive)
        {
            if (DieEvent != null)
            {
                DieEvent.Invoke(damage);
            }
        }
        else
        {
            if (DamagedEvent != null)
            {
                DamagedEvent.Invoke(damage);
            }
        }

        isAlive = false;
    }
}

public class DamageEvent : UnityEvent<DamageData>
{ }

public struct DamageData
{
    public float DamageAmount;    
    public Vector3 HitPosition;
    public Vector3 HitDirection;
    public bool Deadly;
    public IHittableObject Receiver;
    public DamageType HitType;
    public enum DamageType
    {
        BulletImpact,
        KnifeHit,
        Explosion
    }
}