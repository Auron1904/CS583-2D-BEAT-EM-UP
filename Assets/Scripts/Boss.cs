using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Boss : MonoBehaviour
{
    [Header("Facing")]
    [SerializeField] private bool spriteFacesRight = false;   // Wiseman's art faces left

    [Header("Projectile Attack")]
    [SerializeField] private BossProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 6f;
    [SerializeField] private float firstAttackDelay = 2f;
    [SerializeField] private float minTimeBetweenAttacks = 2f;
    [SerializeField] private float maxTimeBetweenAttacks = 3.5f;
    [SerializeField] private float windup = 0.7f;
    [SerializeField] private Color windupTint = new Color(0.6f, 0.6f, 1f);

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.5f;

    [Header("Sounds")]
    [SerializeField] private AudioClip windupSfx;
    [SerializeField] private AudioClip attackSfx;
    [SerializeField] private AudioClip hurtSfx;
    [SerializeField] private AudioClip deathSfx;

    private Health health;
    private SpriteRenderer sprite;
    private Animator anim;
    private Transform player;
    private Health playerHealth;

    private int facing = -1;
    private Color baseColor;
    private Vector3 baseScale;

    void Awake()
    {
        health = GetComponent<Health>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        baseColor = sprite.color;
        baseScale = transform.localScale;

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

        // The boss counts as this level's only enemy.
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterEnemy(health);

        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        if (health.IsDead || player == null) return;
        SetFacing(player.position.x >= transform.position.x ? 1 : -1);   // always face the player
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(firstAttackDelay);

        while (!health.IsDead)
        {
            if (playerHealth != null && playerHealth.IsDead) yield break;

            // Telegraph
            Trigger("Attack");
            SFX.Play(windupSfx);
            sprite.color = windupTint;
            yield return new WaitForSeconds(windup);
            sprite.color = baseColor;

            // Fire
            BossProjectile shot = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            shot.Launch(facing, projectileSpeed);
            SFX.Play(attackSfx);

            // Rest (random so the rhythm can't be memorized)
            yield return new WaitForSeconds(Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks));
        }
    }

    private void OnDamaged(Vector2 sourcePosition, float knockback)
    {
        // Bosses don't get knocked back. Just react.
        Trigger("Hurt");
        SFX.Play(hurtSfx);
    }

    private void OnDied()
    {
        StopAllCoroutines();
        sprite.color = baseColor;
        Trigger("Die");
        SFX.Play(deathSfx);

        foreach (Collider2D col in GetComponents<Collider2D>()) col.enabled = false;
        Destroy(gameObject, destroyDelay);
    }

    private void SetFacing(int newFacing)
    {
        facing = newFacing;
        float artFlip = spriteFacesRight ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * facing * artFlip, baseScale.y, baseScale.z);
    }

    private void Trigger(string name)
    {
        if (anim != null && anim.runtimeAnimatorController != null) anim.SetTrigger(name);
    }
}