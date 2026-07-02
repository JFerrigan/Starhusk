using UnityEngine;

public class DysonSatellite : MonoBehaviour
{
    public DysonSatelliteMode mode;
    public Vector2 sunPosition = Vector2.zero;
    public float orbitRadius = 90f;
    public float startAngleDegrees;
    public float orbitSpeedDegrees;

    private float spawnedAt;

    public bool IsDynamic => mode == DysonSatelliteMode.Dynamic;

    private void Awake()
    {
        spawnedAt = Time.time;
    }

    private void Update()
    {
        if (!IsDynamic)
        {
            return;
        }

        transform.position = OrbitPositionAtTime(
            sunPosition,
            orbitRadius,
            startAngleDegrees,
            orbitSpeedDegrees,
            Time.time - spawnedAt
        );
    }

    public static Vector2 OrbitPositionAtTime(Vector2 center, float radius, float startAngleDegrees, float orbitSpeedDegrees, float elapsedSeconds)
    {
        float angle = (startAngleDegrees + (orbitSpeedDegrees * elapsedSeconds)) * Mathf.Deg2Rad;
        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    public static float AngleFromSun(Vector2 sunPosition, Vector2 satellitePosition)
    {
        Vector2 offset = satellitePosition - sunPosition;
        return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }

    public void SetStationaryPosition(Vector2 position)
    {
        transform.position = position;

        if (IsDynamic)
        {
            return;
        }

        Vector2 offset = position - sunPosition;
        orbitRadius = offset.magnitude;
        startAngleDegrees = AngleFromSun(sunPosition, position);
        orbitSpeedDegrees = 0f;
    }
}
