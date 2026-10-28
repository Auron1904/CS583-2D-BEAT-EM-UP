using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashTime = 0.1f;

    private SpriteRenderer sprite;
    private Color baseColor;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        baseColor = sprite.color;
        GetComponent<Health>().Damaged += OnDamaged;
    }

    private void OnDamaged(Vector2 sourcePosition, float knockback)
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        sprite.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        sprite.color = baseColor;
    }
}