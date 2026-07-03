using System.Collections.Generic;
using UnityEngine;

public class CompanionAutomaton : MonoBehaviour
{
    public const int DefaultCapacity = 1200;

    public float followDistance = 6f;
    public float followSpeed = 24f;
    public float pickupScanRadius = 38f;
    public float pickupCollectRadius = 1.2f;
    public float resourceSeekSpeedMultiplier = 1.25f;
    public float storageDepositRadius = 7f;
    public float depositInterval = 0.2f;

    [SerializeField]
    private ResourceStorage cargo;

    private ResourceInventory playerInventory;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private ResourcePickup targetPickup;
    private float nextDepositTime;

    public ResourceStorage Cargo => cargo;
    public bool HasCapacity => cargo != null && !cargo.IsFull;

    private void Awake()
    {
        EnsureComponents();
    }

    private void Update()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<ResourceInventory>();
        }

        CollectNearbyPickups();

        if (Time.time >= nextDepositTime)
        {
            nextDepositTime = Time.time + Mathf.Max(0.05f, depositInterval);
            DepositToNearbyStorage();
        }
    }

    private void FixedUpdate()
    {
        if (playerInventory == null || rb == null)
        {
            return;
        }

        Vector2 currentPosition = rb.position;
        ResourcePickup pickup = FindTargetPickup();
        Vector2 desiredPosition = pickup == null ? FollowPosition(currentPosition) : (Vector2)pickup.transform.position;
        float speed = pickup == null ? followSpeed : followSpeed * Mathf.Max(0.1f, resourceSeekSpeedMultiplier);
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, desiredPosition, speed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);

        Vector2 travel = nextPosition - currentPosition;
        if (travel.sqrMagnitude > 0.0001f)
        {
            rb.MoveRotation(Mathf.Atan2(travel.y, travel.x) * Mathf.Rad2Deg - 90f);
        }

        if (pickup != null && Vector2.Distance(nextPosition, pickup.transform.position) <= pickupCollectRadius)
        {
            TryCollectPickup(pickup);
        }
    }

    public bool CanAccept(ResourceType resourceType)
    {
        return cargo != null && !cargo.IsFull;
    }

    public bool TryCollectPickup(ResourcePickup pickup)
    {
        if (pickup == null || cargo == null || cargo.IsFull)
        {
            return false;
        }

        bool collected = pickup.TryCollect(cargo);
        if (collected && pickup == targetPickup)
        {
            targetPickup = null;
        }

        return collected;
    }

    public ResourcePickup FindTargetPickupForTests()
    {
        return FindTargetPickup();
    }

    private void CollectNearbyPickups()
    {
        if (cargo == null || cargo.IsFull)
        {
            return;
        }

        ResourcePickup[] pickups = FindObjectsByType<ResourcePickup>(FindObjectsSortMode.None);
        float collectRadiusSquared = pickupCollectRadius * pickupCollectRadius;
        float scanRadiusSquared = pickupScanRadius * pickupScanRadius;

        for (int i = 0; i < pickups.Length && !cargo.IsFull; i++)
        {
            ResourcePickup pickup = pickups[i];
            if (pickup == null || !CanAccept(pickup.resourceType))
            {
                continue;
            }

            float distanceSquared = Vector2.SqrMagnitude((Vector2)pickup.transform.position - (Vector2)transform.position);
            if (distanceSquared <= collectRadiusSquared || distanceSquared <= scanRadiusSquared && pickup.DistanceToTarget(transform) <= pickupCollectRadius)
            {
                TryCollectPickup(pickup);
            }
        }
    }

    private ResourcePickup FindTargetPickup()
    {
        if (cargo == null || cargo.IsFull)
        {
            targetPickup = null;
            return null;
        }

        if (targetPickup != null && IsPickupReachable(targetPickup))
        {
            return targetPickup;
        }

        targetPickup = null;
        ResourcePickup[] pickups = FindObjectsByType<ResourcePickup>(FindObjectsSortMode.None);
        float bestDistanceSquared = float.MaxValue;

        for (int i = 0; i < pickups.Length; i++)
        {
            ResourcePickup pickup = pickups[i];
            if (!IsPickupReachable(pickup))
            {
                continue;
            }

            float distanceSquared = Vector2.SqrMagnitude((Vector2)pickup.transform.position - (Vector2)transform.position);
            if (distanceSquared < bestDistanceSquared)
            {
                bestDistanceSquared = distanceSquared;
                targetPickup = pickup;
            }
        }

        return targetPickup;
    }

    private bool IsPickupReachable(ResourcePickup pickup)
    {
        if (pickup == null || !CanAccept(pickup.resourceType))
        {
            return false;
        }

        float distanceSquared = Vector2.SqrMagnitude((Vector2)pickup.transform.position - (Vector2)transform.position);
        return distanceSquared <= pickupScanRadius * pickupScanRadius;
    }

    private Vector2 FollowPosition(Vector2 currentPosition)
    {
        Vector2 playerPosition = playerInventory.transform.position;
        Vector2 offset = currentPosition - playerPosition;
        return playerPosition + (offset.sqrMagnitude > 0.001f ? offset.normalized : -Vector2.right) * followDistance;
    }

    private void DepositToNearbyStorage()
    {
        if (cargo == null || cargo.IsEmpty)
        {
            return;
        }

        ResourceStorage destination = FindNearbyStorage();
        if (destination != null)
        {
            cargo.TransferAllTo(destination);
        }
    }

    private ResourceStorage FindNearbyStorage()
    {
        ResourceStorage[] storages = FindObjectsByType<ResourceStorage>(FindObjectsSortMode.None);
        ResourceStorage best = null;
        float bestDistanceSquared = float.MaxValue;
        float radiusSquared = storageDepositRadius * storageDepositRadius;

        for (int i = 0; i < storages.Length; i++)
        {
            ResourceStorage storage = storages[i];
            if (storage == null || storage == cargo || storage.IsFull)
            {
                continue;
            }

            float companionDistanceSquared = Vector2.SqrMagnitude((Vector2)storage.transform.position - (Vector2)transform.position);
            float playerDistanceSquared = playerInventory == null
                ? float.MaxValue
                : Vector2.SqrMagnitude((Vector2)storage.transform.position - (Vector2)playerInventory.transform.position);
            float distanceSquared = Mathf.Min(companionDistanceSquared, playerDistanceSquared);

            if (distanceSquared <= radiusSquared && distanceSquared < bestDistanceSquared)
            {
                best = storage;
                bestDistanceSquared = distanceSquared;
            }
        }

        return best;
    }

    private void EnsureComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
        collider.radius = 0.46f;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = PlaceholderSprites.CollectorAutomaton;
        spriteRenderer.color = new Color(0.35f, 1f, 0.72f, 1f);
        spriteRenderer.sortingOrder = 95;

        cargo = GetComponent<ResourceStorage>();
        if (cargo == null)
        {
            cargo = gameObject.AddComponent<ResourceStorage>();
        }

        cargo.Configure(DefaultCapacity);
    }
}
