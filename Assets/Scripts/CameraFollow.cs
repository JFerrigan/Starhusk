using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followAcceleration = 55f;
    public float velocityDamping = 5f;
    public float baseOrthographicSize = 16f;
    public float maxOrthographicSize = 34f;
    public float speedForMaxZoom = 90f;
    public float zoomResponsiveness = 3f;

    private Vector3 velocity;
    private Camera attachedCamera;
    private PlayerMovement targetMovement;

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
        RefreshTargetMovement();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        RefreshTargetMovement();

        Vector3 targetPosition = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        Vector3 offset = targetPosition - transform.position;
        velocity += offset * followAcceleration * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, Vector3.zero, velocityDamping * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;

        UpdateZoom();
    }

    public static float CalculateTargetOrthographicSize(float speed, float baseSize, float maxSize, float speedForMaxZoom)
    {
        float minimumSize = Mathf.Max(0.01f, baseSize);
        float maximumSize = Mathf.Max(minimumSize, maxSize);
        float normalizedSpeed = speedForMaxZoom <= 0f ? 1f : Mathf.Clamp01(Mathf.Max(0f, speed) / speedForMaxZoom);
        return Mathf.Lerp(minimumSize, maximumSize, normalizedSpeed);
    }

    private void RefreshTargetMovement()
    {
        if (target == null)
        {
            targetMovement = null;
            return;
        }

        if (targetMovement == null || targetMovement.transform != target)
        {
            targetMovement = target.GetComponent<PlayerMovement>();
        }
    }

    private void UpdateZoom()
    {
        if (attachedCamera == null)
        {
            attachedCamera = GetComponent<Camera>();
        }

        if (attachedCamera == null || !attachedCamera.orthographic)
        {
            return;
        }

        float speed = targetMovement == null ? 0f : targetMovement.Speed;
        float targetSize = CalculateTargetOrthographicSize(speed, baseOrthographicSize, maxOrthographicSize, speedForMaxZoom);
        float t = 1f - Mathf.Exp(-Mathf.Max(0f, zoomResponsiveness) * Time.deltaTime);
        attachedCamera.orthographicSize = Mathf.Lerp(attachedCamera.orthographicSize, targetSize, t);
    }
}
