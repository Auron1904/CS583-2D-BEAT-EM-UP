using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 16f;
    [SerializeField] private float smoothTime = 0.15f;

    private float velocityX;

    void LateUpdate()
    {
        if (target == null) return;
        float goalX = Mathf.Clamp(target.position.x, minX, maxX);
        float x = Mathf.SmoothDamp(transform.position.x, goalX, ref velocityX, smoothTime);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }
}