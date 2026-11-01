using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private Vector2 hitBoxSize = new Vector2(0.8f, 0.8f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float spinSpeed = 360f;   // degrees per second, just for looks

    private int direction = -1;
    private float speed = 6f;

    // Called by the boss right after Instantiate.
    public void Launch(int dir, float spd)
    {
        direction = dir;
        speed = spd;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * dir, s.y, s.z);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);   // cleanup if it never hits anything
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
        transform.Rotate(0f, 0f, -spinSpeed * direction * Time.deltaTime);

        Collider2D hit = Physics2D.OverlapBox(transform.position, hitBoxSize, 0f, playerLayer);
        if (hit != null)
        {
            Health target = hit.GetComponentInParent<Health>();
            if (target != null) target.Kill();   // instant kill, ignores invincibility
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, hitBoxSize);
    }
}