using UnityEngine;
using UnityEngine.InputSystem;

public class DamageTester : MonoBehaviour
{
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        health.HealthChanged += (current, max) => Debug.Log($"HP: {current}/{max}");
        health.Died += () => Debug.Log("DIED");
    }

    void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
            health.TakeDamage(1, transform.position + Vector3.left, 0f);
    }
}