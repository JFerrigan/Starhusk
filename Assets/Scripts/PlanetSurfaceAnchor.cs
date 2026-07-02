using UnityEngine;

public class PlanetSurfaceAnchor : MonoBehaviour
{
    [SerializeField]
    private Transform planet;

    [SerializeField]
    private Vector2 surfaceNormal = Vector2.up;

    [Header("Surface Fit")]
    [SerializeField]
    private float surfaceInset = 4.4f;

    [SerializeField]
    private float rotationOffsetDegrees = -90f;

    public Transform Planet => planet;
    public Vector2 SurfaceNormal => surfaceNormal;

    public void Bind(Transform targetPlanet, Vector2 normal)
    {
        planet = targetPlanet;
        surfaceNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector2.up;
    }

    public void SnapToSurface()
    {
        if (planet == null)
        {
            return;
        }

        ApplySurfacePose(transform, planet, surfaceNormal, transform.position.z, surfaceInset, rotationOffsetDegrees);
    }

    public static void ApplySurfacePose(
        Transform objectTransform,
        Transform targetPlanet,
        Vector2 normal,
        float z,
        float surfaceInset = 0.55f,
        float rotationOffsetDegrees = -90f)
    {
        if (objectTransform == null || targetPlanet == null)
        {
            return;
        }

        Vector2 normalizedNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector2.up;
        objectTransform.position = SurfacePosition(objectTransform, targetPlanet, normalizedNormal, z, surfaceInset);
        objectTransform.rotation = SurfaceRotation(normalizedNormal, rotationOffsetDegrees);
    }

    public static Vector3 SurfacePosition(
        Transform objectTransform,
        Transform targetPlanet,
        Vector2 normal,
        float z,
        float surfaceInset = 0.55f)
    {
        Vector2 planetCenter = targetPlanet.position;
        float planetRadius = SurfaceRadiusFor(targetPlanet);
        float objectRadius = SurfaceRadiusFor(objectTransform);

        // The inset sinks the mine slightly into the planet so it looks planted,
        // not floating above the edge.
        float distanceFromPlanetCenter = planetRadius + Mathf.Max(0f, objectRadius - Mathf.Max(0f, surfaceInset));
        Vector2 offset = normal.normalized * distanceFromPlanetCenter;

        return new Vector3(planetCenter.x + offset.x, planetCenter.y + offset.y, z);
    }

    public static Quaternion SurfaceRotation(Vector2 normal, float rotationOffsetDegrees = -90f)
    {
        Vector2 normalizedNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector2.up;

        // This assumes the mine sprite's visual "top" points up in the sprite.
        // If the mine points sideways, change rotationOffsetDegrees.
        float angle = Mathf.Atan2(normalizedNormal.y, normalizedNormal.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
    }

    public static float SurfaceRadiusFor(Transform target)
    {
        if (target == null)
        {
            return 0f;
        }

        CircleCollider2D circleCollider = target.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            return circleCollider.radius * Mathf.Max(target.lossyScale.x, target.lossyScale.y);
        }

        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        return Mathf.Max(target.lossyScale.x, target.lossyScale.y) * 0.5f;
    }
}