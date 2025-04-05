using UnityEngine;

public class ArcadeCamera : MonoBehaviour
{
    Vector3 offset = new Vector3(0f, 0f, -3f);
    float smoothTime = 0.25f;
    Vector3 velocity = Vector3.zero;
    [SerializeField] Transform target;
    Transform tf;
    void Start() {
        tf = GetComponent<Transform>();
    }

    void Update() {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(tf.position, targetPosition, ref velocity, smoothTime);
    }
}