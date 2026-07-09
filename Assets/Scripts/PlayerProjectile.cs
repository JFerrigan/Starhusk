using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 58f;
    public float lifetime = 2.4f;
    public float cutRadius = 1.65f;
    public float damage = 25f;
    public ShipFaction faction = ShipFaction.Player;
    public bool homingEnabled;
    public float homingRange = 160f;
    public float homingTurnSpeedDegrees = 260f;
    public bool annihilateAsteroids;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.up;
    private float expiresAt;
    private Transform ownerRoot;

    public void Launch(Vector2 launchDirection, Vector2 inheritedVelocity)
    {
        Launch(launchDirection, inheritedVelocity, null, faction);
    }

    public void Launch(Vector2 launchDirection, Vector2 inheritedVelocity, Transform owner, ShipFaction projectileFaction)
    {
        direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.up;
        ownerRoot = owner == null ? null : owner.root;
        faction = projectileFaction;
        EnsureComponents();
        rb.linearVelocity = (direction * speed) + inheritedVelocity;
        transform.up = direction;
        expiresAt = Time.time + lifetime;
    }

    private void Awake()
    {
        EnsureComponents();
        expiresAt = Time.time + lifetime;
    }

    private void Update()
    {
        UpdateHoming();

        if (Time.time >= expiresAt)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ownerRoot != null && other.transform.root == ownerRoot)
        {
            return;
        }

        PlayerProjectile otherProjectile = other.GetComponent<PlayerProjectile>();
        if (otherProjectile != null)
        {
            if (otherProjectile.faction != faction)
            {
                DestroyProjectile(otherProjectile.gameObject);
                DestroyProjectile(gameObject);
            }

            return;
        }

        ShipHealth health = other.GetComponentInParent<ShipHealth>();
        if (health != null)
        {
            if (health.ApplyDamage(damage, faction))
            {
                DestroyProjectile(gameObject);
            }

            return;
        }

        CircularDestructibleAsteroid asteroid = other.GetComponent<CircularDestructibleAsteroid>();
        if (asteroid == null)
        {
            asteroid = other.GetComponentInParent<CircularDestructibleAsteroid>();
        }

        if (asteroid == null)
        {
            return;
        }

        Vector2 projectilePosition = transform.position;
        Vector2 impactPoint = other.ClosestPoint(projectilePosition);

        if ((impactPoint - projectilePosition).sqrMagnitude < 0.0001f)
        {
            impactPoint = projectilePosition;
        }

        bool impacted = annihilateAsteroids
            ? asteroid.DestroyEntireAsteroid(impactPoint, direction)
            : asteroid.ApplyCircularCut(impactPoint, cutRadius, direction);

        if (impacted)
        {
            DestroyProjectile(gameObject);
        }
    }

    private void UpdateHoming()
    {
        if (!homingEnabled || rb == null)
        {
            return;
        }

        Transform target = FindHomingTarget(transform.position, homingRange, faction, ownerRoot);
        if (target == null)
        {
            return;
        }

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        if (toTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 rotated = Vector3.RotateTowards(direction, toTarget.normalized, homingTurnSpeedDegrees * Mathf.Deg2Rad * Time.deltaTime, 0f);
        direction = ((Vector2)rotated).normalized;
        rb.linearVelocity = direction * speed;
        transform.up = direction;
    }

    public static Transform FindHomingTarget(Vector2 position, float range, ShipFaction projectileFaction, Transform owner)
    {
        float rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range);
        Transform nearestEnemy = null;
        float nearestEnemyDistance = float.MaxValue;

        ShipHealth[] ships = FindObjectsByType<ShipHealth>(FindObjectsSortMode.None);
        for (int i = 0; i < ships.Length; i++)
        {
            ShipHealth ship = ships[i];
            if (ship == null || ship.transform.root == owner || ship.faction == projectileFaction)
            {
                continue;
            }

            float distanceSquared = ((Vector2)ship.transform.position - position).sqrMagnitude;
            if (distanceSquared <= rangeSquared && distanceSquared < nearestEnemyDistance)
            {
                nearestEnemyDistance = distanceSquared;
                nearestEnemy = ship.transform;
            }
        }

        if (nearestEnemy != null)
        {
            return nearestEnemy;
        }

        Transform nearestAsteroid = null;
        float nearestAsteroidDistance = float.MaxValue;
        MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);
        for (int i = 0; i < markers.Length; i++)
        {
            MapMarker marker = markers[i];
            if (marker == null || marker.markerType != MapMarkerType.Asteroid || !marker.CanAppearOnMapAndRadar)
            {
                continue;
            }

            float distanceSquared = ((Vector2)marker.transform.position - position).sqrMagnitude;
            if (distanceSquared <= rangeSquared && distanceSquared < nearestAsteroidDistance)
            {
                nearestAsteroidDistance = distanceSquared;
                nearestAsteroid = marker.transform;
            }
        }

        return nearestAsteroid;
    }

    private void EnsureComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
        collider.radius = 0.11f;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = PlaceholderSprites.Circle;
        renderer.color = faction == ShipFaction.Pirate
            ? new Color(1f, 0.24f, 0.16f, 1f)
            : new Color(1f, 0.92f, 0.45f, 1f);
        renderer.sortingOrder = 90;
        transform.localScale = Vector3.one * 0.32f;
    }

    private static void DestroyProjectile(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
