using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health target;          // leave empty to auto-find the player
    [SerializeField] private RectTransform fill;

    void Start()
    {
        if (target == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) target = pc.GetComponent<Health>();
        }

        if (target == null)
        {
            gameObject.SetActive(false);   // nothing to show
            return;
        }

        target.HealthChanged += Refresh;
        Refresh(target.Current, target.Max);   // draw the starting value
    }

    void OnDestroy()
    {
        if (target != null) target.HealthChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        fill.localScale = new Vector3((float)current / max, 1f, 1f);
    }
}