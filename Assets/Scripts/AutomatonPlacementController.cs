using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutomatonPlacementController : MonoBehaviour
{
    public Key collectorPlacementKey = Key.Digit6;
    public Key hubPlacementKey = Key.Digit7;
    public Key freighterPlacementKey = Key.Digit8;
    public Key freighterCargoStoragePlacementKey = Key.Digit9;
    public Key satelliteFactoryPlacementKey = Key.Digit0;
    public float ghostAlpha = 0.42f;

    public static AutomatonPlacementController Instance { get; private set; }

    private enum PlacementMode
    {
        None,
        Collector,
        Hub,
        Freighter,
        FreighterCargoStorage,
        SatelliteFactory
    }

    private PlacementMode placementMode;
    private GameObject ghostObject;
    private SpriteRenderer ghostRenderer;
    private bool waitingForMouseRelease;

    public bool IsPlacing => placementMode != PlacementMode.None;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        if (Keyboard.current[collectorPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(PlacementMode.Collector);
        }

        if (Keyboard.current[hubPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(PlacementMode.Hub);
        }

        if (Keyboard.current[freighterPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(PlacementMode.Freighter);
        }

        if (Keyboard.current[freighterCargoStoragePlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(PlacementMode.FreighterCargoStorage);
        }

        if (Keyboard.current[satelliteFactoryPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(PlacementMode.SatelliteFactory);
        }

        if (!IsPlacing)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        if (waitingForMouseRelease && !Mouse.current.leftButton.isPressed)
        {
            waitingForMouseRelease = false;
        }

        Vector2 cursorPosition = CursorWorldPosition();
        bool blocked = IsPlacementBlocked(placementMode, cursorPosition) || !BuildResourcePool.CanAfford(BuildCostFor(placementMode));

        UpdateGhost(cursorPosition, blocked);

        if (!waitingForMouseRelease && Mouse.current.leftButton.wasPressedThisFrame && !blocked)
        {
            SpawnCurrent(cursorPosition);
            CancelPlacement();
        }
    }

    private void TogglePlacement(PlacementMode mode)
    {
        if (placementMode == mode)
        {
            CancelPlacement();
            return;
        }

        BuildingSelectionController selectionController = FindFirstObjectByType<BuildingSelectionController>();
        if (selectionController != null)
        {
            selectionController.ClearSelection();
        }

        placementMode = mode;
        waitingForMouseRelease = Mouse.current != null && Mouse.current.leftButton.isPressed;
        EnsureGhost();
    }

    public void BeginCollectorPlacement()
    {
        TogglePlacement(PlacementMode.Collector);
    }

    public void BeginHubPlacement()
    {
        TogglePlacement(PlacementMode.Hub);
    }

    public void BeginFreighterPlacement()
    {
        TogglePlacement(PlacementMode.Freighter);
    }

    public void BeginFreighterCargoStoragePlacement()
    {
        TogglePlacement(PlacementMode.FreighterCargoStorage);
    }

    public void BeginSatelliteFactoryPlacement()
    {
        TogglePlacement(PlacementMode.SatelliteFactory);
    }

    private void SpawnCurrent(Vector2 worldPosition)
    {
        if (!BuildResourcePool.Spend(BuildCostFor(placementMode)))
        {
            return;
        }

        switch (placementMode)
        {
            case PlacementMode.Collector:
                SpawnCollector(worldPosition);
                return;

            case PlacementMode.Hub:
                SpawnHub(worldPosition);
                return;

            case PlacementMode.Freighter:
                ResourceStorage sourceStorage = FindStorageAt(worldPosition, null);
                FreighterAutomaton freighter = SpawnFreighter(worldPosition);
                freighter.AssignEndpoints(sourceStorage, null);

                BuildingSelectionController selectionController = FindFirstObjectByType<BuildingSelectionController>();
                if (selectionController != null)
                {
                    selectionController.SelectFreighter(freighter);
                }

                return;

            case PlacementMode.FreighterCargoStorage:
                SpawnFreighterCargoStorage(worldPosition);
                return;

            case PlacementMode.SatelliteFactory:
                SpawnSatelliteFactory(worldPosition);
                return;
        }
    }

    private void SpawnCollector(Vector2 worldPosition)
    {
        string displayName = ObjectNamer.NumberedManMadeName("Collector Automaton");
        GameObject collector = new GameObject(displayName);
        ObjectNamer.AssignIdentity(collector, displayName, ObjectIdentityCategory.ManMade);
        collector.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        collector.transform.localScale = Vector3.one * CollectorScale();
        ParentToGeneratedRoot(collector.transform);

        SpriteRenderer renderer = collector.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.CollectorAutomaton;
        renderer.color = CollectorColor();
        renderer.sortingOrder = 45;

        CircleCollider2D collider = collector.AddComponent<CircleCollider2D>();
        collider.radius = LocalColliderRadius();
        collider.isTrigger = true;

        Rigidbody2D body = collector.AddComponent<Rigidbody2D>();
        ConfigureNoCollisionBody(body);

        collector.AddComponent<ResourceStorage>();
        collector.AddComponent<CollectorAutomaton>();

        MapMarker marker = collector.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Collector;
        marker.markerColor = CollectorColor();
        marker.iconScale = 0.8f;
        marker.requireDiscovery = false;
    }

    private void SpawnHub(Vector2 worldPosition)
    {
        string displayName = ObjectNamer.NumberedManMadeName("Collector Hub");
        GameObject hub = new GameObject(displayName);
        ObjectNamer.AssignIdentity(hub, displayName, ObjectIdentityCategory.ManMade);
        hub.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        hub.transform.localScale = Vector3.one * HubScale();
        ParentToGeneratedRoot(hub.transform);

        SpriteRenderer renderer = hub.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.CollectorHub;
        renderer.color = HubColor();
        renderer.sortingOrder = 40;

        CircleCollider2D collider = hub.AddComponent<CircleCollider2D>();
        collider.radius = LocalColliderRadius();

        hub.AddComponent<ResourceStorage>();
        hub.AddComponent<CollectorHub>();

        MapMarker marker = hub.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Hub;
        marker.markerColor = HubColor();
        marker.iconScale = 1f;
        marker.requireDiscovery = false;
    }

    private FreighterAutomaton SpawnFreighter(Vector2 worldPosition)
    {
        string displayName = ObjectNamer.NumberedManMadeName("Freighter");
        GameObject freighter = new GameObject(displayName);
        ObjectNamer.AssignIdentity(freighter, displayName, ObjectIdentityCategory.ManMade);
        freighter.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        freighter.transform.localScale = Vector3.one * FreighterScale();
        ParentToGeneratedRoot(freighter.transform);

        SpriteRenderer renderer = freighter.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.CollectorAutomaton;
        renderer.color = FreighterColor();
        renderer.sortingOrder = 48;

        CircleCollider2D collider = freighter.AddComponent<CircleCollider2D>();
        collider.radius = LocalColliderRadius();
        collider.isTrigger = true;

        Rigidbody2D body = freighter.AddComponent<Rigidbody2D>();
        ConfigureNoCollisionBody(body);

        ResourceStorage cargo = freighter.AddComponent<ResourceStorage>();
        cargo.Configure(FreighterAutomaton.DefaultCapacity);

        FreighterAutomaton automaton = freighter.AddComponent<FreighterAutomaton>();

        MapMarker marker = freighter.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Collector;
        marker.markerColor = FreighterColor();
        marker.iconScale = 1f;
        marker.requireDiscovery = false;

        return automaton;
    }

    private void SpawnFreighterCargoStorage(Vector2 worldPosition)
    {
        string displayName = ObjectNamer.NumberedManMadeName(FreighterAutomaton.CargoStorageObjectName);
        GameObject storageObject = new GameObject(displayName);
        ObjectNamer.AssignIdentity(storageObject, displayName, ObjectIdentityCategory.ManMade);
        storageObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        storageObject.transform.localScale = Vector3.one * FreighterCargoStorageScale();
        ParentToGeneratedRoot(storageObject.transform);

        SpriteRenderer renderer = storageObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.CollectorHub;
        renderer.color = FreighterCargoStorageColor();
        renderer.sortingOrder = 42;

        CircleCollider2D collider = storageObject.AddComponent<CircleCollider2D>();
        collider.radius = LocalColliderRadius();
        collider.isTrigger = true;

        ResourceStorage storage = storageObject.AddComponent<ResourceStorage>();
        storage.Configure(FreighterAutomaton.DestinationCapacity);

        MapMarker marker = storageObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Hub;
        marker.markerColor = FreighterCargoStorageColor();
        marker.iconScale = 1.1f;
        marker.requireDiscovery = false;
    }

    private void SpawnSatelliteFactory(Vector2 worldPosition)
    {
        string displayName = ObjectNamer.NumberedManMadeName("Satellite Factory");
        GameObject factoryObject = new GameObject(displayName);
        ObjectNamer.AssignIdentity(factoryObject, displayName, ObjectIdentityCategory.ManMade);
        factoryObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        factoryObject.transform.localScale = Vector3.one * SatelliteFactoryScale();
        ParentToGeneratedRoot(factoryObject.transform);

        SpriteRenderer renderer = factoryObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.SatelliteFactory;
        renderer.color = SatelliteFactoryColor();
        renderer.sortingOrder = 43;

        CircleCollider2D collider = factoryObject.AddComponent<CircleCollider2D>();
        collider.radius = SatelliteFactoryLocalColliderRadius();
        collider.isTrigger = true;

        ResourceStorage storage = factoryObject.AddComponent<ResourceStorage>();
        storage.Configure(SatelliteFactory.DefaultCapacity);

        factoryObject.AddComponent<SatelliteFactory>();

        MapMarker marker = factoryObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Hub;
        marker.markerColor = SatelliteFactoryColor();
        marker.iconScale = 1.1f;
        marker.requireDiscovery = false;
    }

    private static void ConfigureNoCollisionBody(Rigidbody2D body)
    {
        if (body == null)
        {
            return;
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void UpdateGhost(Vector2 worldPosition, bool blocked)
    {
        EnsureGhost();
        ghostObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        ghostObject.transform.localScale = Vector3.one * VisualScale(placementMode);
        ghostRenderer.sprite = SpriteFor(placementMode);

        Color baseColor = ColorFor(placementMode);
        ghostRenderer.color = blocked
            ? new Color(1f, 0.24f, 0.2f, ghostAlpha)
            : new Color(baseColor.r, baseColor.g, baseColor.b, ghostAlpha);
    }

    private void EnsureGhost()
    {
        if (ghostObject != null)
        {
            return;
        }

        ghostObject = new GameObject("Automaton Placement Ghost");
        ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        ghostRenderer.sortingOrder = 90;
    }

    public static bool IsBlockedAt(Vector2 worldPosition, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && !hits[i].isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 CursorWorldPosition()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return Vector2.zero;
        }

        Vector3 screen = Mouse.current.position.ReadValue();
        Vector3 world = camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -camera.transform.position.z));
        return new Vector2(world.x, world.y);
    }

    private void CancelPlacement()
    {
        placementMode = PlacementMode.None;
        waitingForMouseRelease = false;

        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
            ghostRenderer = null;
        }
    }

    private void ParentToGeneratedRoot(Transform child)
    {
        StarSystemGenerator generator = FindFirstObjectByType<StarSystemGenerator>();
        if (generator != null && generator.generatedRoot != null)
        {
            child.SetParent(generator.generatedRoot);
        }
    }

    private static bool PlacementCanOverlap(PlacementMode mode)
    {
        return mode == PlacementMode.Freighter || mode == PlacementMode.SatelliteFactory;
    }

    private static bool IsPlacementBlocked(PlacementMode mode, Vector2 worldPosition)
    {
        if (mode == PlacementMode.SatelliteFactory && !IsValidSatelliteFactoryPosition(worldPosition))
        {
            return true;
        }

        return !PlacementCanOverlap(mode) && IsBlockedAt(worldPosition, WorldPlacementRadius(mode));
    }

    private static ResourceStack[] BuildCostFor(PlacementMode mode)
    {
        switch (mode)
        {
            case PlacementMode.Hub:
                return Cost(
                    new ResourceStack(ResourceType.Ore, 8),
                    new ResourceStack(ResourceType.Copper, 3));

            case PlacementMode.Freighter:
                return Cost(
                    new ResourceStack(ResourceType.Ore, 10),
                    new ResourceStack(ResourceType.Copper, 5),
                    new ResourceStack(ResourceType.Silicate, 3));

            case PlacementMode.FreighterCargoStorage:
                return Cost(
                    new ResourceStack(ResourceType.Ore, 8),
                    new ResourceStack(ResourceType.Silicate, 2));

            case PlacementMode.SatelliteFactory:
                return Cost(
                    new ResourceStack(ResourceType.Ore, 15),
                    new ResourceStack(ResourceType.Copper, 8),
                    new ResourceStack(ResourceType.Silicate, 8));

            case PlacementMode.Collector:
                return Cost(
                    new ResourceStack(ResourceType.Ore, 5),
                    new ResourceStack(ResourceType.Copper, 2));

            default:
                return System.Array.Empty<ResourceStack>();
        }
    }

    private static ResourceStack[] Cost(params ResourceStack[] cost)
    {
        return cost;
    }

    public static bool IsValidSatelliteFactoryPosition(Vector2 worldPosition)
    {
        float distanceFromSun = worldPosition.magnitude;
        return distanceFromSun >= SatelliteFactoryMinSunDistance() && distanceFromSun <= SatelliteFactoryMaxSunDistance();
    }

    private static ResourceStorage FindStorageAt(Vector2 worldPosition, ResourceStorage excludedStorage)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        for (int i = 0; i < hits.Length; i++)
        {
            ResourceStorage storage = hits[i].GetComponentInParent<ResourceStorage>();
            if (storage != null && storage != excludedStorage)
            {
                return storage;
            }
        }

        return null;
    }

    private static Sprite SpriteFor(PlacementMode mode)
    {
        switch (mode)
        {
            case PlacementMode.Hub:
            case PlacementMode.FreighterCargoStorage:
                return PlaceholderSprites.CollectorHub;
            case PlacementMode.SatelliteFactory:
                return PlaceholderSprites.SatelliteFactory;

            default:
                return PlaceholderSprites.CollectorAutomaton;
        }
    }

    private static Color ColorFor(PlacementMode mode)
    {
        switch (mode)
        {
            case PlacementMode.Hub:
                return HubColor();

            case PlacementMode.Freighter:
                return FreighterColor();

            case PlacementMode.FreighterCargoStorage:
                return FreighterCargoStorageColor();
            case PlacementMode.SatelliteFactory:
                return SatelliteFactoryColor();

            default:
                return CollectorColor();
        }
    }

    private static Color CollectorColor()
    {
        return new Color(0.45f, 0.95f, 1f, 1f);
    }

    private static Color HubColor()
    {
        return new Color(1f, 0.86f, 0.38f, 1f);
    }

    private static Color FreighterColor()
    {
        return new Color(0.78f, 0.58f, 1f, 1f);
    }

    private static Color FreighterCargoStorageColor()
    {
        return new Color(0.95f, 0.62f, 1f, 1f);
    }

    private static Color SatelliteFactoryColor()
    {
        return new Color(0.74f, 0.92f, 1f, 1f);
    }

    private static float VisualScale(PlacementMode mode)
    {
        switch (mode)
        {
            case PlacementMode.Hub:
                return HubScale();

            case PlacementMode.Freighter:
                return FreighterScale();

            case PlacementMode.FreighterCargoStorage:
                return FreighterCargoStorageScale();
            case PlacementMode.SatelliteFactory:
                return SatelliteFactoryScale();

            default:
                return CollectorScale();
        }
    }

    private static float CollectorScale()
    {
        return 4f;
    }

    private static float HubScale()
    {
        return 7f;
    }

    private static float FreighterScale()
    {
        return 5.5f;
    }

    private static float FreighterCargoStorageScale()
    {
        return 8f;
    }

    private static float SatelliteFactoryScale()
    {
        return 8f;
    }

    private static float LocalColliderRadius()
    {
        return 0.42f;
    }

    private static float WorldPlacementRadius(PlacementMode mode)
    {
        return VisualScale(mode) * LocalColliderRadius();
    }

    private static float SatelliteFactoryLocalColliderRadius()
    {
        return 0.48f;
    }

    private static float SatelliteFactoryMinSunDistance()
    {
        return 10f;
    }

    private static float SatelliteFactoryMaxSunDistance()
    {
        return 200f;
    }
}

public class FreighterAutomaton : MonoBehaviour
{
    public const string CargoStorageObjectName = "Freighter Cargo Storage";
    public const int DefaultCapacity = 5000;
    public const int DestinationCapacity = 250000;

    public float moveSpeed = 26f;
    public float interactionDistance = 8f;
    public float endpointRefreshInterval = 1f;
    public float loadingDuration = 0.4f;
    public float unloadingDuration = 0.4f;
    public int transferAmountPerTrip = 250;
    public float routeWaypointArrivalDistance = 1.5f;
    public float obstacleProbeRadius = 1.1f;
    public float obstacleClearance = 3f;
    public FreighterCargoPriority cargoPriority = FreighterCargoPriority.Mixed;

    [SerializeField]
    private ResourceStorage cargo;

    [SerializeField]
    private ResourceStorage sourceStorage;

    [SerializeField]
    private ResourceStorage destinationStorage;

    [SerializeField]
    private FreighterState state = FreighterState.FindingEndpoints;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform destinationTransform;
    private Transform sourceTransform;
    private float workCompleteTime;
    private Color baseColor = Color.white;
    private bool hasBaseColor;
    private readonly List<Vector2> routePath = new List<Vector2>();
    private int routeIndex = -1;
    private Transform lastDestination;

    public ResourceStorage Cargo => cargo;
    public ResourceStorage SourceStorage => sourceStorage;
    public ResourceStorage DestinationStorage => destinationStorage;
    public FreighterState State => state;
    public IReadOnlyList<Vector2> RoutePath => routePath;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
            hasBaseColor = true;
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
        if (!HasEndpoints())
        {
            state = FreighterState.FindingEndpoints;
            ApplyWorkingVisual(false);
            return;
        }

        if (state == FreighterState.FindingEndpoints)
        {
            state = cargo.IsEmpty ? FreighterState.MovingToSource : FreighterState.MovingToStorage;
        }

        if (state == FreighterState.LoadingAtSource && Time.time >= workCompleteTime)
        {
            CompleteLoading();
        }

        if (state == FreighterState.UnloadingAtStorage && Time.time >= workCompleteTime)
        {
            CompleteUnloading();
        }
    }

    private void FixedUpdate()
    {
        if (!HasEndpoints())
        {
            StopMovement();
            return;
        }

        if (state == FreighterState.LoadingAtSource || state == FreighterState.UnloadingAtStorage)
        {
            StopMovement();
            return;
        }

        Transform destination = CurrentDestination();
        if (destination == null)
        {
            StopMovement();
            return;
        }

        Vector2 currentPosition = rb == null ? (Vector2)transform.position : rb.position;
        Vector2 targetPosition = destination.position;

        if (destination != lastDestination)
        {
            BuildRouteTo(destination);
            lastDestination = destination;
        }

        if (Vector2.Distance(currentPosition, targetPosition) <= interactionDistance)
        {
            StopMovement();
            ClearRoute();

            if (state == FreighterState.MovingToSource)
            {
                BeginLoading();
                return;
            }

            if (state == FreighterState.MovingToStorage)
            {
                BeginUnloading();
                return;
            }
        }

        AdvanceRouteWaypoint();
        MoveToward(CurrentMovementTarget(destination));
        DrawActiveRoute();
    }

    public void AssignEndpoints(ResourceStorage source, ResourceStorage destination)
    {
        if (source == cargo || destination == cargo || source == destination)
        {
            return;
        }

        sourceStorage = source;
        sourceTransform = sourceStorage == null ? null : sourceStorage.transform;
        destinationStorage = destination;
        destinationTransform = destinationStorage == null ? null : destinationStorage.transform;
        state = HasEndpoints()
            ? (cargo != null && !cargo.IsEmpty ? FreighterState.MovingToStorage : FreighterState.MovingToSource)
            : FreighterState.FindingEndpoints;
        ClearRoute();
        lastDestination = null;
    }

    public void SetCargoPriority(FreighterCargoPriority priority)
    {
        cargoPriority = priority;
    }

    public void CompleteLoadingForTests()
    {
        LoadFromSourceStorage();
    }

    private bool HasEndpoints()
    {
        return sourceStorage != null && sourceTransform != null && destinationStorage != null && destinationTransform != null;
    }

    private Transform CurrentDestination()
    {
        if (state == FreighterState.MovingToStorage || state == FreighterState.UnloadingAtStorage)
        {
            return destinationTransform;
        }

        return sourceTransform;
    }

    private void BeginLoading()
    {
        state = FreighterState.LoadingAtSource;
        workCompleteTime = Time.time + Mathf.Max(0f, loadingDuration);
        ApplyWorkingVisual(true);
    }

    private void BeginUnloading()
    {
        state = FreighterState.UnloadingAtStorage;
        workCompleteTime = Time.time + Mathf.Max(0f, unloadingDuration);
        ApplyWorkingVisual(true);
    }

    private void CompleteLoading()
    {
        ApplyWorkingVisual(false);
        LoadFromSourceStorage();

        state = cargo.IsEmpty
            ? FreighterState.MovingToSource
            : FreighterState.MovingToStorage;
    }

    private void CompleteUnloading()
    {
        ApplyWorkingVisual(false);

        if (destinationStorage != null && cargo != null)
        {
            cargo.TransferAllTo(destinationStorage);
        }

        state = cargo != null && !cargo.IsEmpty
            ? FreighterState.MovingToStorage
            : FreighterState.MovingToSource;
    }

    private void LoadFromSourceStorage()
    {
        if (sourceStorage == null || cargo == null || cargo.IsFull)
        {
            return;
        }

        int remainingTransferBudget = Mathf.Max(1, transferAmountPerTrip);

        if (cargoPriority != FreighterCargoPriority.Mixed)
        {
            LoadPriorityResource(PriorityToResourceType(cargoPriority), remainingTransferBudget);
            return;
        }

        LoadMixedResources(remainingTransferBudget);
    }

    private void LoadPriorityResource(ResourceType resourceType, int transferBudget)
    {
        int requestedAmount = Mathf.Min(sourceStorage.GetAmount(resourceType), transferBudget, cargo.RemainingCapacity);
        if (requestedAmount <= 0)
        {
            return;
        }

        int removedAmount = sourceStorage.RemoveResource(resourceType, requestedAmount);
        int acceptedAmount = cargo.AddResource(resourceType, removedAmount);
        if (acceptedAmount < removedAmount)
        {
            sourceStorage.AddResource(resourceType, removedAmount - acceptedAmount);
        }
    }

    private void LoadMixedResources(int transferBudget)
    {
        IReadOnlyList<ResourceStack> availableResources = sourceStorage.GetResources();
        int availableTypeCount = 0;

        for (int i = 0; i < availableResources.Count; i++)
        {
            if (availableResources[i].amount > 0)
            {
                availableTypeCount++;
            }
        }

        if (availableTypeCount <= 0)
        {
            return;
        }

        int remainingTransferBudget = Mathf.Min(transferBudget, cargo.RemainingCapacity);
        int baseShare = Mathf.Max(1, remainingTransferBudget / availableTypeCount);
        int extraShare = remainingTransferBudget % availableTypeCount;

        for (int i = 0; i < availableResources.Count && remainingTransferBudget > 0 && !cargo.IsFull; i++)
        {
            ResourceType resourceType = availableResources[i].type;
            int availableAmount = sourceStorage.GetAmount(resourceType);
            if (availableAmount <= 0)
            {
                continue;
            }

            int requestedShare = baseShare + (extraShare > 0 ? 1 : 0);
            if (extraShare > 0)
            {
                extraShare--;
            }

            int requestedAmount = Mathf.Min(availableAmount, requestedShare, remainingTransferBudget, cargo.RemainingCapacity);
            int removedAmount = sourceStorage.RemoveResource(resourceType, requestedAmount);
            int acceptedAmount = cargo.AddResource(resourceType, removedAmount);

            remainingTransferBudget -= acceptedAmount;

            if (acceptedAmount < removedAmount)
            {
                sourceStorage.AddResource(resourceType, removedAmount - acceptedAmount);
                break;
            }
        }
    }

    private static ResourceType PriorityToResourceType(FreighterCargoPriority priority)
    {
        switch (priority)
        {
            case FreighterCargoPriority.Ice:
                return ResourceType.Ice;
            case FreighterCargoPriority.Silicate:
                return ResourceType.Silicate;
            case FreighterCargoPriority.Copper:
                return ResourceType.Copper;
            case FreighterCargoPriority.Biomass:
                return ResourceType.Biomass;
            case FreighterCargoPriority.Ore:
            default:
                return ResourceType.Ore;
        }
    }

    private void MoveToward(Vector2 targetPosition)
    {
        Vector2 currentPosition = rb == null ? (Vector2)transform.position : rb.position;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }

        Vector2 velocity = targetPosition - currentPosition;
        if (velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;

            if (rb != null)
            {
                rb.MoveRotation(angle);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    private Vector2 CurrentMovementTarget(Transform destination)
    {
        if (routePath.Count > 0 && routeIndex >= 0 && routeIndex < routePath.Count)
        {
            return routePath[routeIndex];
        }

        return destination == null ? (Vector2)transform.position : (Vector2)destination.position;
    }

    private void BuildRouteTo(Transform destination)
    {
        ClearRoute();
        if (destination == null)
        {
            return;
        }

        Collider2D ownCollider = GetComponent<Collider2D>();
        Collider2D targetCollider = destination.GetComponent<Collider2D>();
        List<Vector2> path = LogisticsRoutePlanner.BuildSegmentPath(
            transform.position,
            destination.position,
            null,
            obstacleProbeRadius,
            obstacleClearance,
            ownCollider,
            targetCollider);

        for (int i = 0; i < path.Count; i++)
        {
            routePath.Add(path[i]);
        }

        routeIndex = routePath.Count > 0 ? 0 : -1;
    }

    private void AdvanceRouteWaypoint()
    {
        Vector2 origin = rb == null ? (Vector2)transform.position : rb.position;
        while (routePath.Count > 0 && routeIndex >= 0 && routeIndex < routePath.Count && Vector2.Distance(origin, routePath[routeIndex]) <= routeWaypointArrivalDistance)
        {
            routeIndex++;
        }
    }

    private void ClearRoute()
    {
        routePath.Clear();
        routeIndex = -1;
    }

    private void DrawActiveRoute()
    {
        if (!HiddenRoutingDisplayController.RoutesVisible || routePath.Count <= 0)
        {
            return;
        }

        Color routeColor = new Color(0.95f, 0.34f, 1f, 0.95f);
        Vector2 previous = rb == null ? (Vector2)transform.position : rb.position;
        for (int i = Mathf.Max(0, routeIndex); i < routePath.Count; i++)
        {
            Debug.DrawLine(previous, routePath[i], routeColor);
            previous = routePath[i];
        }
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void ApplyWorkingVisual(bool active)
    {
        if (spriteRenderer == null || !hasBaseColor)
        {
            return;
        }

        spriteRenderer.color = active
            ? new Color(baseColor.r * 0.72f, baseColor.g * 0.72f, baseColor.b * 0.72f, baseColor.a)
            : baseColor;
    }
}

public enum FreighterState
{
    FindingEndpoints,
    MovingToSource,
    LoadingAtSource,
    MovingToStorage,
    UnloadingAtStorage
}

public enum FreighterCargoPriority
{
    Mixed,
    Ore,
    Ice,
    Silicate,
    Copper,
    Biomass
}
