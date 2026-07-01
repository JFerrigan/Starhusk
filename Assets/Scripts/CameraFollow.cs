using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followAcceleration = 55f;
    public float velocityDamping = 5f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        Vector3 offset = targetPosition - transform.position;
        velocity += offset * followAcceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, Vector3.zero, velocityDamping * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;
    }
}
