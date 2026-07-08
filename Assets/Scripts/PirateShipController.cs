using UnityEngine;

public class PirateShipController : MonoBehaviour
{
    public float sightRange = 170f;
    public float attackRange = 90f;
    public float preferredRange = 42f;
    public float thrustForce = 16f;
    public float maxSpeed = 24f;
    public float rotationSpeed = 220f;
    public float fireCooldown = 0.75f;
    public float muzzleOffset = 1.05f;
    public float projectileSpeed = 48f;
    public float projectileLifetime = 2.8f;
    public float projectileDamage = 5f;
    public float leashRadius = 145f;
    public float returnRadius = 70f;
    public int oreDropAmount = 35;
    public int copperDropAmount = 15;

    private Rigidbody2D rb;
    private ShipHealth health;
    private Transform target;
    private float nextFireTime;
    private bool hasHomeAnchor;
    private Vector2 homeAnchor;

    public bool HasHomeAnchor => hasHomeAnchor;
    public Vector2 HomeAnchor => homeAnchor;

    public static PirateShipController SpawnNear(Transform origin)
    {
        Vector3 basePosition = origin == null ? Vector3.zero : origin.position;
        Vector2 offset = origin == null
            ? new Vector2(85f, 30f)
            : (Vector2)(origin.up * 70f) + (Vector2)(origin.right * 28f);

        return SpawnAt(basePosition + new Vector3(offset.x, offset.y, 0f));
    }

    public static PirateShipController SpawnAt(Vector3 position)
    {
        GameObject pirateObject = new GameObject("Pirate Raider");
        pirateObject.transform.position = position;
        pirateObject.transform.localScale = Vector3.one * 12f;
        return pirateObject.AddComponent<PirateShipController>();
    }

    public static PirateShipController SpawnAnchoredAt(Vector3 position, Vector2 homeAnchor, Transform parent, float leashRadius = 145f)
    {
        PirateShipController controller = SpawnAt(position);
        if (parent != null)
        {
            controller.transform.SetParent(parent);
        }

        controller.ConfigureHomeAnchor(homeAnchor, leashRadius);
        return controller;
    }

    public void ConfigureHomeAnchor(Vector2 anchor, float radius)
    {
        homeAnchor = anchor;
        hasHomeAnchor = true;
        leashRadius = Mathf.Max(1f, radius);
    }

    private void Awake()
    {
        EnsureFoundation();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Died += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= HandleDeath;
        }
    }

    private void Update()
    {
        AcquireTarget();

        if (target == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange && Time.time >= nextFireTime)
        {
            FireAtTarget();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (target == null)
        {
            if (hasHomeAnchor)
            {
                ReturnTowardHome();
                return;
            }

            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, thrustForce * Time.fixedDeltaTime);
            return;
        }

        if (hasHomeAnchor && !IsInsideLeash(target.position, homeAnchor, leashRadius))
        {
            target = null;
            ReturnTowardHome();
            return;
        }

        Vector2 toTarget = target.position - transform.position;
        if (toTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        RotateToward(toTarget.normalized);

        float distance = toTarget.magnitude;
        Vector2 desiredVelocity = distance > preferredRange
            ? toTarget.normalized * maxSpeed
            : -toTarget.normalized * (maxSpeed * 0.35f);

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            thrustForce * Time.fixedDeltaTime);
    }

    private void AcquireTarget()
    {
        if (target != null)
        {
            ShipHealth targetHealth = target.GetComponent<ShipHealth>();
            if (targetHealth != null &&
                targetHealth.IsAlive &&
                Vector2.Distance(transform.position, target.position) <= sightRange * 1.25f &&
                (!hasHomeAnchor || IsInsideLeash(target.position, homeAnchor, leashRadius)))
            {
                return;
            }
        }

        ResourceInventory playerInventory = FindFirstObjectByType<ResourceInventory>();
        if (playerInventory == null)
        {
            target = null;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerInventory.transform.position);
        bool playerInsideHomeLeash = !hasHomeAnchor || IsInsideLeash(playerInventory.transform.position, homeAnchor, leashRadius);
        target = distance <= sightRange && playerInsideHomeLeash ? playerInventory.transform : null;
    }

    public static bool IsInsideLeash(Vector2 position, Vector2 anchor, float radius)
    {
        return Vector2.Distance(position, anchor) <= radius;
    }

    private void ReturnTowardHome()
    {
        Vector2 toHome = homeAnchor - (Vector2)transform.position;
        if (toHome.magnitude <= returnRadius)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, thrustForce * Time.fixedDeltaTime);
            return;
        }

        RotateToward(toHome.normalized);
        Vector2 desiredVelocity = toHome.normalized * (maxSpeed * 0.65f);
        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            desiredVelocity,
            thrustForce * Time.fixedDeltaTime);
    }

    private void RotateToward(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float nextAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextAngle);
    }

    private void FireAtTarget()
    {
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        Vector2 spawnPosition = (Vector2)transform.position + (direction * muzzleOffset * Mathf.Max(1f, transform.localScale.x * 0.08f));

        GameObject projectileObject = new GameObject("Pirate Projectile");
        projectileObject.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.speed = projectileSpeed;
        projectile.lifetime = projectileLifetime;
        projectile.damage = projectileDamage;
        projectile.faction = ShipFaction.Pirate;
        projectile.Launch(direction, rb == null ? Vector2.zero : rb.linearVelocity, transform, ShipFaction.Pirate);
    }

    private void HandleDeath(ShipHealth deadHealth)
    {
        DropResource(ResourceType.Ore, oreDropAmount, 0);
        DropResource(ResourceType.Copper, copperDropAmount, 1);
    }

    private void DropResource(ResourceType type, int amount, int index)
    {
        if (amount <= 0)
        {
            return;
        }

        Vector2 direction = Quaternion.Euler(0f, 0f, index * 110f) * Vector2.up;
        Vector2 velocity = direction.normalized * Random.Range(2f, 5f);

        GameObject pickupObject = new GameObject(type + " Pirate Drop");
        pickupObject.transform.position = transform.position;
        ResourcePickup pickup = pickupObject.AddComponent<ResourcePickup>();
        pickup.Initialize(type, amount, velocity, ResourceVisuals.ColorFor(type));
    }

    private void EnsureFoundation()
    {
        if (transform.localScale == Vector3.one)
        {
            transform.localScale = Vector3.one * 12f;
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 0.15f;
        rb.angularDamping = 2f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        SpriteRenderer renderer = EnsureVisual();
        if (renderer.sprite != null)
        {
            collider.size = renderer.sprite.bounds.size * 0.55f;
            collider.offset = Vector2.zero;
        }

        health = GetComponent<ShipHealth>();
        if (health == null)
        {
            health = gameObject.AddComponent<ShipHealth>();
        }

        health.faction = ShipFaction.Pirate;
        health.maxHealth = Mathf.Max(1f, health.maxHealth);
        health.currentHealth = health.currentHealth <= 0f ? health.maxHealth : health.currentHealth;
        health.destroyOnDeath = true;

        if (GetComponent<ShipCrashDamage>() == null)
        {
            gameObject.AddComponent<ShipCrashDamage>();
        }

        MapMarker marker = GetComponent<MapMarker>();
        if (marker == null)
        {
            marker = gameObject.AddComponent<MapMarker>();
        }

        marker.markerType = MapMarkerType.Pirate;
        marker.markerColor = new Color(1f, 0.2f, 0.14f, 0.95f);
        marker.iconScale = 1f;
        marker.requireDiscovery = false;

        if (GetComponent<ObjectIdentity>() == null)
        {
            ObjectNamer.AssignIdentity(gameObject, "Pirate Raider", ObjectIdentityCategory.ManMade);
        }
    }

    private SpriteRenderer EnsureVisual()
    {
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            Destroy(rootRenderer);
        }

        Transform visualTransform = transform.Find("ShipVisual");
        if (visualTransform == null)
        {
            GameObject visualObject = new GameObject("ShipVisual");
            visualObject.transform.SetParent(transform, false);
            visualTransform = visualObject.transform;
        }

        visualTransform.localPosition = Vector3.zero;
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        visualTransform.localScale = Vector3.one;

        SpriteRenderer renderer = visualTransform.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite shipSprite = Resources.Load<Sprite>("ship");
        renderer.sprite = shipSprite == null ? PlaceholderSprites.Circle : shipSprite;
        renderer.color = new Color(1f, 0.34f, 0.28f, 1f);
        renderer.sortingOrder = 98;
        return renderer;
    }
}
