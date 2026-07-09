using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    public float fireCooldown = 0.16f;
    public float muzzleOffset = 1.05f;
    public float projectileSpeed = 58f;
    public float projectileLifetime = 2.4f;
    public float projectileCutRadius = 1.65f;
    public float tripleShotSpreadDegrees = 12f;

    private Rigidbody2D rb;
    private float nextFireTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (IsBuildPlacementActive())
        {
            return;
        }

        bool firePressed = false;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            firePressed = true;
        }

        if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed)
        {
            firePressed = true;
        }

        if (firePressed && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void Fire()
    {
        Vector2 forward = transform.up;
        bool tripleShot = IsUpgradeUnlocked(UpgradeId.TripleShotProjectiles);

        if (tripleShot)
        {
            SpawnProjectile(Quaternion.Euler(0f, 0f, -tripleShotSpreadDegrees) * forward);
            SpawnProjectile(forward);
            SpawnProjectile(Quaternion.Euler(0f, 0f, tripleShotSpreadDegrees) * forward);
            return;
        }

        SpawnProjectile(forward);
    }

    private void SpawnProjectile(Vector2 forward)
    {
        forward = forward.sqrMagnitude > 0.001f ? forward.normalized : (Vector2)transform.up;
        Vector2 spawnPosition = (Vector2)transform.position + (forward * muzzleOffset);

        GameObject projectileObject = new GameObject("Mining Projectile");
        projectileObject.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.speed = projectileSpeed;
        projectile.lifetime = projectileLifetime;
        projectile.cutRadius = projectileCutRadius;
        projectile.faction = ShipFaction.Player;
        projectile.homingEnabled = IsUpgradeUnlocked(UpgradeId.HomingProjectiles);
        projectile.annihilateAsteroids = IsUpgradeUnlocked(UpgradeId.AsteroidAnnihilator);
        projectile.Launch(forward, rb == null ? Vector2.zero : rb.linearVelocity, transform, ShipFaction.Player);
    }

    private static bool IsUpgradeUnlocked(UpgradeId upgradeId)
    {
        PlayerUpgradeState state = PlayerUpgradeState.Current;
        return state != null && state.IsUnlocked(upgradeId);
    }

    private static bool IsBuildPlacementActive()
    {
        return BuildingPlacementController.Instance != null && BuildingPlacementController.Instance.IsPlacing;
    }
}
