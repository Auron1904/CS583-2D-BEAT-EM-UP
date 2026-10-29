using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyGrunt : MonoBehaviour
{
    private enum State { Approach, Attacking, Hurt, Dead }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float speedVariance = 0.5f;
    [SerializeField] private float attackRange = 1.1f;
    [SerializeField] private float rangeVariance = 0.25f;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(0.8f, 0.8f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockback = 5f;
    [SerializeField] private float windup = 0.45f;
    [SerializeField] private float recovery = 0.5f;
    [SerializeField] private float cooldown = 1.2f;
    [SerializeField] private Color windupTint = new Color(1f, 0.5f, 0.5f);

    [Header("Getting Hit")]
    [SerializeField] private float hurtStunTime = 0.4f;
    [SerializeField] private float knockbackDecel = 15f;
    [SerializeField] private float destroyDelay = 0.6f;

    [Header("Sounds")]
    [SerializeField] private AudioClip attackSfx;
    [SerializeField] private AudioClip hurtSfx;
    [SerializeField] private AudioClip deathSfx;

    private Rigidbody2D rb;
    private Health health;
    private SpriteRenderer sprite;
    private Animator anim;
    private Transform player;
    private Health playerHealth;

    private State state = State.Approach;
    private float cooldownTimer;
    private int facing = 1;
    private Color baseColor;
    private Vector3 baseScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        baseColor = sprite.color;
        baseScale = transform.localScale;

        // Small random differences so grunts don't move as one blob.
        moveSpeed += Random.Range(-speedVariance, speedVariance);
        attackRange += Random.Range(-rangeVariance, rangeVariance);
        cooldownTimer = Random.Range(0.2f, 0.8f);

        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            player = pc.transform;
            playerHealth = pc.GetComponent<Health>();
        }
    }

    void Update()
    {
        if (state == State.Dead) return;

        if (state == State.Hurt) SlowDown();
        else if (state == State.Approach) Approach();

        UpdateAnimator();
    }

    private void Approach()
    {
        cooldownTimer -= Time.deltaTime;

        // No player, or player is dead: stand still.
        if (player == null || playerHealth.IsDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dx = player.position.x - transform.position.x;
        SetFacing(dx >= 0f ? 1 : -1);

        if (Mathf.Abs(dx) > attackRange)
        {
            rb.linearVelocity = new Vector2(facing * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (cooldownTimer <= 0f) StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        state = State.Attacking;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        Trigger("Attack");

        sprite.color = windupTint;              // telegraph
        yield return new WaitForSeconds(windup);
        sprite.color = baseColor;

        SFX.Play(attackSfx);
        Combat.HitInBox(attackPoint.position, attackBoxSize, playerLayer,
                        damage, knockback, transform.position);

        yield return new WaitForSeconds(recovery);
        cooldownTimer = cooldown;
        state = State.Approach;
    }

    private void OnDamaged(Vector2 sourcePosition, float force)
    {
        if (state == State.Dead) return;

        StopAllCoroutines();        // getting hit interrupts their attack
        sprite.color = baseColor;
        state = State.Hurt;

        float dir = Mathf.Sign(transform.position.x - sourcePosition.x);
        rb.linearVelocity = new Vector2(dir * force, 2f);

        Trigger("Hurt");
        SFX.Play(hurtSfx);
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtStunTime);
        cooldownTimer = Mathf.Max(cooldownTimer, 0.4f);   // don't instantly counterattack
        state = State.Approach;
    }

    private void OnDied()
    {
        StopAllCoroutines();
        state = State.Dead;
        sprite.color = baseColor;
        Trigger("Die");
        SFX.Play(deathSfx);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;    // stop falling once collider is off
        foreach (Collider2D col in GetComponents<Collider2D>()) col.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

    private void SetFacing(int newFacing)
    {
        facing = newFacing;
        transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * facing, baseScale.y, baseScale.z);
    }

    private void SlowDown()
    {
        float x = Mathf.MoveTowards(rb.linearVelocity.x, 0f, knockbackDecel * Time.deltaTime);
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    private void UpdateAnimator()
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void Trigger(string name)
    {
        if (anim != null && anim.runtimeAnimatorController != null) anim.SetTrigger(name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint) Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
}