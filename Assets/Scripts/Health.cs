using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invincibleTime = 0.5f;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public bool IsDead { get; private set; }

    // Broadcasts. Other scripts subscribe with +=
    public event Action<int, int> HealthChanged;    // current, max
    public event Action<Vector2, float> Damaged;    // attacker position, knockback force
    public event Action Died;

    private float invincibleTimer;

    void Awake()
    {
        Current = maxHealth;
    }

    void Update()
    {
        if (invincibleTimer > 0f) invincibleTimer -= Time.deltaTime;
    }

    // Returns true if the hit actually landed.
    public bool TakeDamage(int amount, Vector2 sourcePosition, float knockback)
    {
        if (IsDead || invincibleTimer > 0f) return false;

        Current = Mathf.Max(Current - amount, 0);
        invincibleTimer = invincibleTime;

        HealthChanged?.Invoke(Current, maxHealth);
        Damaged?.Invoke(sourcePosition, knockback);

        if (Current == 0) Die();
        return true;
    }

    // Instant kill (boss projectile). Ignores invincibility.
    public void Kill()
    {
        if (IsDead) return;
        Current = 0;
        HealthChanged?.Invoke(Current, maxHealth);
        Die();
    }

    private void Die()
    {
        IsDead = true;
        Died?.Invoke();
    }
}