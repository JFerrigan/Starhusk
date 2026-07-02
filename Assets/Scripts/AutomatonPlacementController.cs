using UnityEngine;
using UnityEngine.InputSystem;

public class AutomatonPlacementController : MonoBehaviour
{
    public Key collectorPlacementKey = Key.Digit6;
    public Key hubPlacementKey = Key.Digit7;
    public float ghostAlpha = 0.42f;

    public static AutomatonPlacementController Instance { get; private set; }

    private enum PlacementMode
    {
        None,
        Collector,
        Hub
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
        bool blocked = IsBlockedAt(cursorPosition, WorldPlacementRadius(placementMode));
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

    private void SpawnCurrent(Vector2 worldPosition)
    {
        if (placementMode == PlacementMode.Collector)
        {
            SpawnCollector(worldPosition);
            return;
        }

        if (placementMode == PlacementMode.Hub)
        {
            SpawnHub(worldPosition);
        }
    }

    private void SpawnCollector(Vector2 worldPosition)
    {
        GameObject collector = new GameObject("Collector Automaton");
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
body.bodyType = RigidbodyType2D.Kinematic;
body.gravityScale = 0f;
body.linearDamping = 0f;
body.angularDamping = 0f;
body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
body.interpolation = RigidbodyInterpolation2D.Interpolate;

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
        GameObject hub = new GameObject("Collector Hub");
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

    private static Sprite SpriteFor(PlacementMode mode)
    {
        return mode == PlacementMode.Hub ? PlaceholderSprites.CollectorHub : PlaceholderSprites.CollectorAutomaton;
    }

    private static Color ColorFor(PlacementMode mode)
    {
        return mode == PlacementMode.Hub ? HubColor() : CollectorColor();
    }

    private static Color CollectorColor()
    {
        return new Color(0.45f, 0.95f, 1f, 1f);
    }

    private static Color HubColor()
    {
        return new Color(1f, 0.86f, 0.38f, 1f);
    }

    private static float VisualScale(PlacementMode mode)
    {
        return mode == PlacementMode.Hub ? HubScale() : CollectorScale();
    }

    private static float CollectorScale()
    {
        return 4f;
    }

    private static float HubScale()
    {
        return 7f;
    }

    private static float LocalColliderRadius()
    {
        return 0.42f;
    }

    private static float WorldPlacementRadius(PlacementMode mode)
    {
        return VisualScale(mode) * LocalColliderRadius();
    }
}
