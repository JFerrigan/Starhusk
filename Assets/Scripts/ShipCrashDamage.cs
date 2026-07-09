using UnityEngine;

public class ShipCrashDamage : MonoBehaviour
{
    public float safeImpactSpeed = 8f;
    public float damagePerSpeedOverThreshold = 4f;
    public float maxCrashDamage = 65f;
    public float impactCooldownSeconds = 0.35f;

    private ShipHealth health;
    private float nextImpactDamageTime;

    private void Awake()
    {
        health = GetComponent<ShipHealth>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryApplyCrashDamage(collision.collider, collision.relativeVelocity.magnitude, Time.time);
    }

    public static float CalculateDamage(float relativeSpeed, float safeImpactSpeed, float damagePerSpeedOverThreshold, float maxCrashDamage)
    {
        float speedOverThreshold = Mathf.Max(0f, relativeSpeed - safeImpactSpeed);
        return Mathf.Min(maxCrashDamage, speedOverThreshold * damagePerSpeedOverThreshold);
    }

    public bool TryApplyCrashDamage(Collider2D other, float relativeSpeed, float impactTime)
    {
        if (other == null || other.isTrigger || impactTime < nextImpactDamageTime)
        {
            return false;
        }

        if (IsUpgradeUnlocked(UpgradeId.ImpactShield))
        {
            return false;
        }

        if (IsUpgradeUnlocked(UpgradeId.AsteroidCarverHull) && other.GetComponentInParent<CircularDestructibleAsteroid>() != null)
        {
            return false;
        }

        if (!CanDamageFromCollision(other))
        {
            return false;
        }

        if (health == null)
        {
            health = GetComponent<ShipHealth>();
        }

        if (health == null)
        {
            return false;
        }

        float damage = CalculateDamage(relativeSpeed, safeImpactSpeed, damagePerSpeedOverThreshold, maxCrashDamage);
        if (!health.ApplyDamage(damage, ShipFaction.Neutral))
        {
            return false;
        }

        nextImpactDamageTime = impactTime + impactCooldownSeconds;
        return true;
    }

    private static bool CanDamageFromCollision(Collider2D other)
    {
        if (other.GetComponentInParent<ShipHealth>() != null)
        {
            return true;
        }

        if (other.GetComponentInParent<CircularDestructibleAsteroid>() != null)
        {
            return true;
        }

        MapMarker marker = other.GetComponentInParent<MapMarker>();
        return marker != null && (marker.markerType == MapMarkerType.Planet || marker.markerType == MapMarkerType.Star);
    }

    private bool IsUpgradeUnlocked(UpgradeId upgradeId)
    {
        PlayerUpgradeState state = GetComponent<PlayerUpgradeState>();
        if (state == null)
        {
            state = PlayerUpgradeState.Current;
        }

        return state != null && state.IsUnlocked(upgradeId);
    }
}
