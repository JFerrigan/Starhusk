using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Ore;
    public int amount = 1;
    public float lifetime = 45f;
    public float magnetRadius = 8f;
    public float collectRadius = 0.8f;
    public float magnetAcceleration = 38f;
    public float maxSpeed = 12f;

    private Rigidbody2D rb;
    private ResourceInventory targetInventory;
    private float expiresAt;

    public void Initialize(ResourceType type, int pickupAmount, Vector2 velocity, Color color)
    {
        resourceType = type;
        amount = Mathf.Max(1, pickupAmount);
        EnsureComponents(color);

        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }

        expiresAt = Time.time + lifetime;
    }

    private void Awake()
    {
        EnsureComponents(ResourceVisuals.ColorFor(resourceType));
        expiresAt = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= expiresAt)
        {
            Destroy(gameObject);
            return;
        }

        if (targetInventory == null)
        {
            targetInventory = FindFirstObjectByType<ResourceInventory>();
        }

        if (targetInventory == null || rb == null)
        {
            return;
        }

        Vector2 toTarget = targetInventory.transform.position - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= collectRadius)
        {
            Collect(targetInventory);
            return;
        }

        if (distance <= magnetRadius && distance > 0.001f)
        {
            rb.AddForce(toTarget.normalized * magnetAcceleration);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ResourceInventory inventory = other.GetComponent<ResourceInventory>();

        if (inventory != null)
        {
            Collect(inventory);
        }
    }

    private void Collect(ResourceInventory inventory)
    {
        if (inventory == null || amount <= 0)
        {
            return;
        }

        inventory.AddResource(resourceType, amount);
        amount = 0;
        Destroy(gameObject);
    }

    private void EnsureComponents(Color color)
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 0.4f;
        rb.angularDamping = 1.5f;

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
        collider.radius = 0.22f;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = PlaceholderSprites.Circle;
        renderer.color = new Color(color.r, color.g, color.b, 0.9f);
        renderer.sortingOrder = 80;
        transform.localScale = Vector3.one * 0.55f;
    }
}
