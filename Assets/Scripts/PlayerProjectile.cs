using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 58f;
    public float lifetime = 2.4f;
    public float cutRadius = 1.65f;
    public float damage = 25f;
    public ShipFaction faction = ShipFaction.Player;

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

        if (asteroid.ApplyCircularCut(impactPoint, cutRadius, direction))
        {
            DestroyProjectile(gameObject);
        }
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
