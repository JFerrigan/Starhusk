using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 58f;
    public float lifetime = 2.4f;
    public float cutRadius = 1.65f;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.up;
    private float expiresAt;

    public void Launch(Vector2 launchDirection, Vector2 inheritedVelocity)
    {
        direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector2.up;
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
        if (other.GetComponent<ResourceInventory>() != null)
        {
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
            Destroy(gameObject);
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
        renderer.color = new Color(1f, 0.92f, 0.45f, 1f);
        renderer.sortingOrder = 90;
        transform.localScale = Vector3.one * 0.32f;
    }
}
