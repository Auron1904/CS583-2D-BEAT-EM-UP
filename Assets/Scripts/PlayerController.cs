using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class PlayerController : MonoBehaviour
{
    private enum State { Normal, Charging, Punching, Lunging, Hurt, Dead }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Punch")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 punchBoxSize = new Vector2(0.8f, 0.8f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int punchDamage = 1;
    [SerializeField] private float punchKnockback = 3f;
    [SerializeField] private float punchWindup = 0.08f;
    [SerializeField] private float punchRecovery = 0.2f;

    [Header("Charged Lunge")]
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private float lungeSpeed = 14f;
    [SerializeField] private float lungeDistance = 4f;
    [SerializeField] private int lungeDamage = 3;
    [SerializeField] private float lungeKnockback = 10f;
    [SerializeField] private Color chargedTint = Color.yellow;

    [Header("Getting Hit")]
    [SerializeField] private float hurtStunTime = 0.3f;
    [SerializeField] private float knockbackDecel = 20f;

    [Header("Sounds")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip punchSfx;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioClip lungeSfx;
    [SerializeField] private AudioClip chargeLoopSfx;
    [SerializeField] private AudioClip hurtSfx;
    [SerializeField] private AudioClip deathSfx;

    private Rigidbody2D rb;
    private Health health;
    private SpriteRenderer sprite;
    private Animator anim;
    private AudioSource chargeSource;

    private State state = State.Normal;
    private bool isGrounded;
    private int facing = 1;
    private float chargeTimer;
    private bool isCharged;
    private float lungeTimer;
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

        // Separate looping audio source just for the charge hum.
        chargeSource = gameObject.AddComponent<AudioSource>();
        chargeSource.loop = true;
        chargeSource.playOnAwake = false;
        chargeSource.clip = chargeLoopSfx;

        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void Update()
    {
        if (state == State.Dead) return;
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        switch (state)
        {
            case State.Normal:
                float direction = 0f;
                if (kb.leftArrowKey.isPressed) direction -= 1f;
                if (kb.rightArrowKey.isPressed) direction += 1f;

                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
                if (direction != 0f) SetFacing(direction > 0f ? 1 : -1);

                if (kb.spaceKey.wasPressedThisFrame && isGrounded) Jump();
                if (kb.qKey.wasPressedThisFrame) StartCharging();
                break;

            case State.Charging:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);   // planted
                chargeTimer += Time.deltaTime;
                if (!isCharged && chargeTimer >= chargeTime) BecomeCharged();
                if (!kb.qKey.isPressed) ReleaseCharge();
                break;

            case State.Hurt:
                SlowDown();
                break;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (state != State.Lunging) return;

        rb.linearVelocity = new Vector2(facing * lungeSpeed, rb.linearVelocity.y);
        int hits = Combat.HitInBox(attackPoint.position, punchBoxSize, enemyLayer,
                                   lungeDamage, lungeKnockback, transform.position);
        lungeTimer -= Time.fixedDeltaTime;

        if (hits > 0)
        {
            SFX.Play(hitSfx);
            EndLunge();     // stop on first hit
        }
        else if (lungeTimer <= 0f)
        {
            EndLunge();
        }
    }

    // ---------- Movement ----------

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        SFX.Play(jumpSfx);
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

    // ---------- Punch / Charge / Lunge ----------

    private void StartCharging()
    {
        state = State.Charging;
        chargeTimer = 0f;
        isCharged = false;
    }

    private void BecomeCharged()
    {
        isCharged = true;
        sprite.color = chargedTint;
        if (chargeSource.clip != null) chargeSource.Play();
    }

    private void ReleaseCharge()
    {
        bool wasCharged = isCharged;
        CancelCharge();

        if (wasCharged) StartLunge();
        else StartCoroutine(PunchRoutine());
    }

    private void CancelCharge()
    {
        isCharged = false;
        chargeSource.Stop();
        sprite.color = baseColor;
    }

    private IEnumerator PunchRoutine()
    {
        state = State.Punching;
        Trigger("Punch");
        SFX.Play(punchSfx);

        yield return new WaitForSeconds(punchWindup);

        int hits = Combat.HitInBox(attackPoint.position, punchBoxSize, enemyLayer,
                                   punchDamage, punchKnockback, transform.position);
        if (hits > 0) SFX.Play(hitSfx);

        yield return new WaitForSeconds(punchRecovery);
        state = State.Normal;
    }

    private void StartLunge()
    {
        state = State.Lunging;
        lungeTimer = lungeDistance / lungeSpeed;    // time = distance / speed
        SFX.Play(lungeSfx);
    }

    private void EndLunge()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        state = State.Normal;
    }

    // ---------- Getting hit ----------

    private void OnDamaged(Vector2 sourcePosition, float knockback)
    {
        StopAllCoroutines();    // cancels a punch in progress
        CancelCharge();         // getting hit breaks your charge

        state = State.Hurt;
        float dir = Mathf.Sign(transform.position.x - sourcePosition.x);
        rb.linearVelocity = new Vector2(dir * knockback, 3f);

        Trigger("Hurt");
        SFX.Play(hurtSfx);
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(hurtStunTime);
        state = State.Normal;
    }

    private void OnDied()
    {
        StopAllCoroutines();
        CancelCharge();
        state = State.Dead;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        Trigger("Die");
        SFX.Play(deathSfx);
    }

    // ---------- Animation (used in Phase 6) ----------

    private void UpdateAnimator()
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Charging", state == State.Charging);
        anim.SetBool("Lunging", state == State.Lunging);
    }

    private void Trigger(string name)
    {
        if (anim != null && anim.runtimeAnimatorController != null) anim.SetTrigger(name);
    }

    // Draws the ground check and punch box in the Scene view when Player is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.red;
        if (attackPoint) Gizmos.DrawWireCube(attackPoint.position, punchBoxSize);
    }
}