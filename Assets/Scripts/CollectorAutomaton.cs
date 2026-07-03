using System.Collections.Generic;
using UnityEngine;

public class CollectorAutomaton : MonoBehaviour, IPowerConsumer
{
    public const int DefaultCapacity = 500;
    public const int DefaultPowerDemand = 10;

    public AutomatonType automatonType = AutomatonType.Collector;
    public AutomatonGoal goal = AutomatonGoal.CollectResources;
    public float scanRange = 220f;
    public float scanInterval = 1f;
    public float moveSpeed = 18f;
    public float steeringResponsiveness = 10f;
    public float interactionDistance = 5f;
    public float interactionBuffer = 1.5f;
    public float routeWaypointArrivalDistance = 1.5f;
    public float routeRetryInterval = 0.35f;
    public float collectionDuration = 0.5f;
    public float collectingShadeMultiplier = 0.72f;

    [SerializeField]
    private ResourceStorage cargo;

    [SerializeField]
    private PlanetResourceExtractorBuilding targetBuilding;

    [SerializeField]
    private CollectorHub targetHub;

    [SerializeField]
    private PlanetLogisticsNetwork assignedNetwork;

    [SerializeField]
    private CollectorState state = CollectorState.Idle;

    [SerializeField]
    private bool isPowered = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 desiredVelocity;
    private readonly List<Vector2> routePath = new List<Vector2>();
    private float nextScanTime;
    private float nextRouteRetryTime;
    private float collectionProgressSeconds;
    private int routeIndex = -1;
    private int routeNetworkVersion = -1;
    private Transform lastDestination;
    private Color baseVisualColor = Color.white;
    private bool hasBaseVisualColor;
    private bool collectionVisualActive;

    public ResourceStorage Cargo => cargo;
    public CollectorState State => state;
    public PlanetResourceExtractorBuilding TargetBuilding => targetBuilding;
    public CollectorHub TargetHub => targetHub;
    public PlanetLogisticsNetwork AssignedNetwork => assignedNetwork;
    public IReadOnlyList<Vector2> RoutePath => routePath;
    public float CurrentInteractionRange => InteractionRangeFor(CurrentDestination());
    public bool IsCollecting => state == CollectorState.CollectingFromBuilding;
    public float CollectionTimeRemaining => IsCollecting ? Mathf.Max(0f, Mathf.Max(0f, collectionDuration) - collectionProgressSeconds) : 0f;
    public int PowerDemand => DefaultPowerDemand;
    public bool IsPowered => isPowered;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseVisualColor = spriteRenderer.color;
            hasBaseVisualColor = true;
        }

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

Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
for (int i = 0; i < colliders.Length; i++)
{
    colliders[i].isTrigger = true;
}

        cargo = GetComponent<ResourceStorage>();
        if (cargo == null)
        {
            cargo = gameObject.AddComponent<ResourceStorage>();
        }

        cargo.Configure(DefaultCapacity);
    }

    private void Update()
    {
        if (!isPowered)
        {
            return;
        }

        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime = Time.time + scanInterval;
        EvaluateGoal();
    }

    private void FixedUpdate()
    {
        if (!isPowered)
        {
            StopMovement();
            return;
        }

        if (state == CollectorState.CollectingFromBuilding)
        {
            UpdateCollection();
            StopMovement();
            return;
        }

        Transform destination = CurrentDestination();
        if (destination == null)
        {
            ClearRoute();
            lastDestination = null;
            desiredVelocity = Vector2.zero;
            ApplyVelocity();
            return;
        }

        if (destination != lastDestination)
        {
            lastDestination = destination;
            BuildRouteTo(destination);
        }
        else if (assignedNetwork != null && routeNetworkVersion != assignedNetwork.RouteVersion)
        {
            BuildRouteTo(destination);
        }

        if (IsWithinInteractionRange(destination))
        {
            ArriveAtDestination();
            ClearRoute();
            desiredVelocity = Vector2.zero;
            ApplyVelocity();
            return;
        }

        AdvanceRouteWaypoint();

        if (!HasRoute)
{
    if (Time.time >= nextRouteRetryTime)
    {
        BuildRouteTo(destination);
        nextRouteRetryTime = Time.time + Mathf.Max(0.05f, routeRetryInterval);
    }

    Vector2 directOffset = (Vector2)destination.position - CurrentPosition();
    desiredVelocity = directOffset.sqrMagnitude > 0.001f
        ? directOffset.normalized * moveSpeed
        : Vector2.zero;

    ApplyVelocity();
    return;
}

        Vector2 direction = DirectionForDestination(destination);
        desiredVelocity = direction * moveSpeed;
        ApplyVelocity();
        DrawActiveRoute();
    }

    public PlanetResourceExtractorBuilding FindBestBuilding()
    {
        IReadOnlyList<PlanetResourceExtractorBuilding> extractors = PlanetResourceExtractorBuilding.ActiveExtractors;
        PlanetResourceExtractorBuilding bestBuilding = null;
        int bestAmount = 0;

        for (int i = 0; i < extractors.Count; i++)
        {
            PlanetResourceExtractorBuilding candidate = extractors[i];
            if (candidate == null || candidate.Storage == null || candidate.Storage.CurrentAmount <= 0)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance > scanRange)
            {
                continue;
            }

            if (candidate.Storage.CurrentAmount > bestAmount)
            {
                bestAmount = candidate.Storage.CurrentAmount;
                bestBuilding = candidate;
            }
        }

        return bestBuilding;
    }

    public CollectorHub FindNearestHub()
    {
        IReadOnlyList<CollectorHub> hubs = CollectorHub.ActiveHubs;
        CollectorHub nearestHub = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hubs.Count; i++)
        {
            CollectorHub candidate = hubs[i];
            if (candidate == null || candidate.Storage == null || candidate.Storage.IsFull)
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude(candidate.transform.position - transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHub = candidate;
            }
        }

        return nearestHub;
    }

    public void EvaluateGoal()
    {
        if (state == CollectorState.CollectingFromBuilding)
        {
            return;
        }

        UpdateAssignedNetwork();
        if (assignedNetwork == null)
        {
            targetBuilding = null;
            targetHub = null;
            state = CollectorState.Idle;
            return;
        }

        if (cargo == null)
{
    state = CollectorState.Idle;
    return;
}

if (!cargo.IsFull)
{
    targetBuilding = assignedNetwork.FindBestExtractor(transform.position, scanRange);

    if (targetBuilding != null)
    {
        targetHub = null;
        state = CollectorState.MovingToBuilding;
        return;
    }
}

if (!cargo.IsEmpty)
{
    targetBuilding = null;
    targetHub = assignedNetwork.FindNearestCargoWithCapacity(transform.position);
    state = targetHub == null ? CollectorState.Idle : CollectorState.DeliveringToHub;
    return;
}

state = CollectorState.Idle;
    }

    public float InteractionRangeFor(Transform destination)
    {
        float range = interactionDistance + interactionBuffer;
        range += LargestColliderRadius(transform);

        if (destination != null)
        {
            range += LargestColliderRadius(destination);
        }

        return range;
    }

    public bool IsWithinInteractionRange(Transform destination)
    {
        if (destination == null)
        {
            return false;
        }

        Vector2 origin = rb == null ? (Vector2)transform.position : rb.position;
        return Vector2.Distance(origin, destination.position) <= InteractionRangeFor(destination);
    }

    public Vector2 DirectionForDestination(Transform destination)
    {
        if (destination == null)
        {
            return Vector2.zero;
        }

        Vector2 origin = rb == null ? (Vector2)transform.position : rb.position;
        Vector2 movementTarget = CurrentMovementTarget(destination);
        Vector2 toTarget = movementTarget - origin;
        return toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.zero;
    }

    public bool TryBuildSharedRouteTo(Transform destination)
    {
        BuildRouteTo(destination);
        return HasRoute;
    }

    public bool TryInteractWithCurrentDestination()
    {
        Transform destination = CurrentDestination();
        if (!IsWithinInteractionRange(destination))
        {
            return false;
        }

        ArriveAtDestination();
        return true;
    }

    public void CompleteCollectionNow()
    {
        if (state == CollectorState.CollectingFromBuilding)
        {
            FinishCollection();
        }
    }

    public void SetPowered(bool powered)
    {
        if (isPowered == powered)
        {
            return;
        }

        isPowered = powered;
        if (!isPowered)
        {
            StopMovement();
        }

        ApplyVisualState();
    }

    private void ArriveAtDestination()
    {
        if (state == CollectorState.MovingToBuilding)
        {
            BeginCollection();
            return;
        }

        if (state == CollectorState.DeliveringToHub)
        {
            DepositIntoHub();
            targetBuilding = null;
            targetHub = null;
            state = CollectorState.Idle;
            ClearRoute();
            nextScanTime = 0f;
            EvaluateGoal();
            BuildRouteTo(CurrentDestination());
        }
    }

    private void BeginCollection()
    {
        if (targetBuilding == null)
        {
            state = CollectorState.Idle;
            return;
        }

        state = CollectorState.CollectingFromBuilding;
        collectionProgressSeconds = 0f;
        StopMovement();
        ApplyCollectionVisual(true);
    }

    private void UpdateCollection()
    {
        if (targetBuilding == null || cargo == null || cargo.IsFull)
        {
            FinishCollection();
            return;
        }

        collectionProgressSeconds += Time.fixedDeltaTime;
        if (collectionProgressSeconds >= Mathf.Max(0f, collectionDuration))
        {
            FinishCollection();
        }
    }

private void FinishCollection()
{
    ApplyCollectionVisual(false);
    collectionProgressSeconds = 0f;
    CollectFromBuilding();

    ClearRoute();
    lastDestination = null;
    nextScanTime = 0f;

    if (assignedNetwork == null || cargo == null)
    {
        targetBuilding = null;
        targetHub = null;
        state = CollectorState.Idle;
        return;
    }

    if (!cargo.IsFull)
    {
        targetBuilding = assignedNetwork.FindBestExtractor(transform.position, scanRange);

        if (targetBuilding != null)
        {
            targetHub = null;
            state = CollectorState.MovingToBuilding;
            lastDestination = targetBuilding.transform;
            BuildRouteTo(targetBuilding.transform);
            return;
        }
    }

    targetBuilding = null;
    targetHub = !cargo.IsEmpty
        ? assignedNetwork.FindNearestCargoWithCapacity(transform.position)
        : null;

    state = targetHub != null
        ? CollectorState.DeliveringToHub
        : CollectorState.Idle;

    if (state == CollectorState.DeliveringToHub)
    {
        lastDestination = targetHub.transform;
        BuildRouteTo(targetHub.transform);
    }
}

    private void CollectFromBuilding()
    {
        if (targetBuilding == null || targetBuilding.Storage == null || cargo == null || cargo.IsFull)
        {
            return;
        }

        BuildingStorage storage = targetBuilding.Storage;
        int removedAmount = storage.RemoveResource(cargo.RemainingCapacity);
        cargo.AddResource(storage.ResourceType, removedAmount);
    }

    private void DepositIntoHub()
    {
        if (targetHub == null || targetHub.Storage == null || cargo == null)
        {
            return;
        }

        cargo.TransferAllTo(targetHub.Storage);
    }

    private Transform CurrentDestination()
    {
        if (state == CollectorState.MovingToBuilding && targetBuilding != null)
        {
            return targetBuilding.transform;
        }

        if (state == CollectorState.CollectingFromBuilding && targetBuilding != null)
        {
            return targetBuilding.transform;
        }

        if (state == CollectorState.DeliveringToHub && targetHub != null)
        {
            return targetHub.transform;
        }

        return null;
    }

    private Vector2 CurrentMovementTarget(Transform destination)
    {
        if (HasRoute)
        {
            return routePath[routeIndex];
        }

        return destination == null ? (Vector2)transform.position : CurrentPosition();
    }

    private bool HasRoute => routePath.Count > 0 && routeIndex >= 0 && routeIndex < routePath.Count;

    private void BuildRouteTo(Transform destination)
    {
        ClearRoute();
        if (destination == null)
        {
            return;
        }

        UpdateAssignedNetwork();
        if (assignedNetwork == null)
        {
            return;
        }

        IReadOnlyList<Vector2> path = assignedNetwork.GetRoutePath(CurrentPosition(), destination);
        for (int i = 0; i < path.Count; i++)
        {
            routePath.Add(path[i]);
        }

        routeIndex = routePath.Count > 0 ? 0 : -1;
        routeNetworkVersion = assignedNetwork.RouteVersion;
    }

    private void AdvanceRouteWaypoint()
    {
        if (!HasRoute)
        {
            return;
        }

        Vector2 origin = CurrentPosition();
        while (HasRoute && Vector2.Distance(origin, routePath[routeIndex]) <= routeWaypointArrivalDistance)
        {
            routeIndex++;
        }
    }

    private void ClearRoute()
    {
        routePath.Clear();
        routeIndex = -1;
        routeNetworkVersion = -1;
        nextRouteRetryTime = 0f;
    }

    private void UpdateAssignedNetwork()
    {
        PlanetLogisticsNetwork nearestNetwork = PlanetLogisticsNetwork.FindNearest(transform.position, scanRange);
        if (nearestNetwork == assignedNetwork)
        {
            return;
        }

        assignedNetwork = nearestNetwork;
        ClearRoute();
        lastDestination = null;
    }

    private Vector2 CurrentPosition()
    {
        return rb == null ? (Vector2)transform.position : rb.position;
    }

    private void ApplyCollectionVisual(bool active)
    {
        if (spriteRenderer == null || !hasBaseVisualColor || collectionVisualActive == active)
        {
            return;
        }

        collectionVisualActive = active;
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (spriteRenderer == null || !hasBaseVisualColor)
        {
            return;
        }

        Color color = collectionVisualActive
            ? new Color(
                baseVisualColor.r * collectingShadeMultiplier,
                baseVisualColor.g * collectingShadeMultiplier,
                baseVisualColor.b * collectingShadeMultiplier,
                baseVisualColor.a)
            : baseVisualColor;

        if (!isPowered)
        {
            color = new Color(color.r * 0.34f, color.g * 0.34f, color.b * 0.34f, color.a);
        }

        spriteRenderer.color = color;
    }

    private void ApplyVelocity()
    {
        if (rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVelocity, Mathf.Clamp01(steeringResponsiveness * Time.fixedDeltaTime));

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            rb.MoveRotation(angle);
        }
    }

    private void StopMovement()
    {
        desiredVelocity = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void DrawActiveRoute()
    {
        if (!HiddenRoutingDisplayController.RoutesVisible || routePath.Count <= 0)
        {
            return;
        }

        Color routeColor = new Color(1f, 0.78f, 0.18f, 0.95f);
        Vector2 previous = CurrentPosition();
        for (int i = Mathf.Max(0, routeIndex); i < routePath.Count; i++)
        {
            Debug.DrawLine(previous, routePath[i], routeColor);
            previous = routePath[i];
        }
    }

    private static float LargestColliderRadius(Transform root)
    {
        if (root == null)
        {
            return 0f;
        }

        Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>();
        float largestRadius = 0f;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D candidate = colliders[i];
            if (candidate == null || candidate.isTrigger)
            {
                continue;
            }

            Vector2 extents = candidate.bounds.extents;
            largestRadius = Mathf.Max(largestRadius, extents.magnitude);
        }

        return largestRadius;
    }
}

public enum CollectorState
{
    Idle,
    MovingToBuilding,
    CollectingFromBuilding,
    DeliveringToHub
}
