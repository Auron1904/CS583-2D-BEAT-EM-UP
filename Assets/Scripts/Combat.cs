using UnityEngine;

public static class Combat
{
    // Damages every Health inside a box. Returns how many were hit.
    public static int HitInBox(Vector2 center, Vector2 size, LayerMask targets,
                               int damage, float knockback, Vector2 sourcePosition)
    {
        int hits = 0;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, targets);

        foreach (Collider2D col in colliders)
        {
            Health target = col.GetComponentInParent<Health>();
            if (target != null && target.TakeDamage(damage, sourcePosition, knockback))
                hits++;
        }
        return hits;
    }
}