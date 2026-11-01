using UnityEngine;

// Run after CameraFollow so we always use this frame's camera position.
[DefaultExecutionOrder(100)]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float speed = 0.5f;   // 0.1 = far away, 1.0 = fixed in the world

    private Transform cam;
    private float startX;
    private float camStartX;

    void Start()
    {
        cam = Camera.main.transform;
        startX = transform.position.x;
        camStartX = cam.position.x;
    }

    void LateUpdate()
    {
        float camMoved = cam.position.x - camStartX;
        transform.position = new Vector3(startX + camMoved * (1f - speed),
                                         transform.position.y, transform.position.z);
    }
}