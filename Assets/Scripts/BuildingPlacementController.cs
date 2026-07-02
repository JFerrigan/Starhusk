using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacementController : MonoBehaviour
{
    public Key minePlacementKey = Key.Digit2;
    public Key condenserPlacementKey = Key.Digit3;
    public Key harvesterPlacementKey = Key.Digit4;
    public Key dredgerPlacementKey = Key.Digit5;
    public float ghostAlpha = 0.4f;

    public static BuildingPlacementController Instance { get; private set; }

    public bool IsPlacing => placementMode != PlacementMode.None;

    private enum PlacementMode
    {
        None,
        PlaceBuilding,
        MoveBuilding
    }

    private PlacementMode placementMode;
    private BuildingType activeBuildingType = BuildingType.Mine;
    private PlanetResourceExtractorBuilding movingBuilding;
    private GameObject ghostObject;
    private SpriteRenderer ghostRenderer;
    private bool waitingForMouseRelease;

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

        DestroyGhost();
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
        {
            return;
        }

        if (Keyboard.current[minePlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(BuildingType.Mine);
        }

        if (Keyboard.current[condenserPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(BuildingType.Condenser);
        }

        if (Keyboard.current[harvesterPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(BuildingType.Harvester);
        }

        if (Keyboard.current[dredgerPlacementKey].wasPressedThisFrame)
        {
            TogglePlacement(BuildingType.Dredger);
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

        if (waitingForMouseRelease)
        {
            if (!Mouse.current.leftButton.isPressed)
            {
                waitingForMouseRelease = false;
            }
        }

        Vector2 cursorPosition = CursorWorldPosition();
        ResourceDeposit targetDeposit;
        Transform planetTransform;
        Vector2 surfaceNormal;
        bool hasTarget = TryGetHoverTarget(cursorPosition, out targetDeposit, out planetTransform, out surfaceNormal);
        bool canPlaceHere = hasTarget && IsPlacementAllowedAtTarget(targetDeposit);

        UpdateGhost(cursorPosition, hasTarget, canPlaceHere, planetTransform, surfaceNormal);

        if (waitingForMouseRelease)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && canPlaceHere)
        {
            ConfirmPlacement(targetDeposit, planetTransform, surfaceNormal);
        }
    }

    public void BeginMineMove(PlanetMine mine)
    {
        BeginMove(mine);
    }
public void BeginMove(PlanetResourceExtractorBuilding building)
{
    if (building == null)
    {
        return;
    }

    BuildingSelectionController selectionController = FindFirstObjectByType<BuildingSelectionController>();
    if (selectionController != null)
    {
        selectionController.ClearSelection();
    }

    placementMode = PlacementMode.MoveBuilding;
    activeBuildingType = building.buildingType;
    movingBuilding = building;
    waitingForMouseRelease = Mouse.current != null && Mouse.current.leftButton.isPressed;
    EnsureGhost(activeBuildingType);
}

    public static bool IsValidMinePlacementTarget(ResourceDeposit deposit)
    {
        return IsValidPlacementTarget(BuildingType.Mine, deposit);
    }

    public static bool IsValidPlacementTarget(BuildingType buildingType, ResourceDeposit deposit)
    {
        return BuildingCatalog.IsValidPlacementTarget(buildingType, deposit);
    }

 private void TogglePlacement(BuildingType buildingType)
{
    if (placementMode == PlacementMode.PlaceBuilding && movingBuilding == null && activeBuildingType == buildingType)
    {
        CancelPlacement();
        return;
    }

    BuildingSelectionController selectionController = FindFirstObjectByType<BuildingSelectionController>();
    if (selectionController != null)
    {
        selectionController.ClearSelection();
    }

    placementMode = PlacementMode.PlaceBuilding;
    activeBuildingType = buildingType;
    movingBuilding = null;
    waitingForMouseRelease = Mouse.current != null && Mouse.current.leftButton.isPressed;
    EnsureGhost(buildingType);
}

    private void ConfirmPlacement(ResourceDeposit deposit, Transform planetTransform, Vector2 surfaceNormal)
    {
        if (placementMode == PlacementMode.MoveBuilding && movingBuilding != null)
        {
            if (!movingBuilding.BindToPlanet(deposit, planetTransform, surfaceNormal))
            {
                return;
            }
        }
        else
        {
            SpawnBuilding(activeBuildingType, deposit, planetTransform, surfaceNormal);
        }

        waitingForMouseRelease = true;
        CancelPlacement();
    }

    private void SpawnBuilding(BuildingType buildingType, ResourceDeposit deposit, Transform planetTransform, Vector2 surfaceNormal)
    {
        BuildingDefinition definition = BuildingCatalog.GetDefinition(buildingType);
        GameObject buildingObject = new GameObject(definition.displayName);
        buildingObject.transform.localScale = Vector3.one * definition.visualScale;

        StarSystemGenerator generator = FindFirstObjectByType<StarSystemGenerator>();
        if (generator != null && generator.generatedRoot != null)
        {
            buildingObject.transform.SetParent(generator.generatedRoot);
        }

        SpriteRenderer renderer = buildingObject.AddComponent<SpriteRenderer>();
        renderer.sprite = definition.placeholderSprite;
        renderer.color = definition.tint;
        renderer.sortingOrder = 35;

        CircleCollider2D collider = buildingObject.AddComponent<CircleCollider2D>();
        collider.radius = definition.colliderRadius;

        buildingObject.AddComponent<PlanetSurfaceAnchor>();
        buildingObject.AddComponent<BuildingStorage>();

        PlanetResourceExtractorBuilding building = AddExtractorComponent(buildingObject, buildingType);
        building.Initialize(deposit, planetTransform, surfaceNormal);
    }

    private PlanetResourceExtractorBuilding AddExtractorComponent(GameObject buildingObject, BuildingType buildingType)
    {
        switch (buildingType)
        {
            case BuildingType.Condenser:
                return buildingObject.AddComponent<PlanetCondenser>();
            case BuildingType.Harvester:
                return buildingObject.AddComponent<PlanetHarvester>();
            case BuildingType.Dredger:
                return buildingObject.AddComponent<PlanetDredger>();
            case BuildingType.Mine:
            default:
                return buildingObject.AddComponent<PlanetMine>();
        }
    }

    private bool TryGetHoverTarget(Vector2 cursorWorldPosition, out ResourceDeposit deposit, out Transform planetTransform, out Vector2 surfaceNormal)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(cursorWorldPosition);

        for (int i = 0; i < hits.Length; i++)
        {
            ResourceDeposit candidateDeposit = hits[i].GetComponentInParent<ResourceDeposit>();
            if (candidateDeposit == null)
            {
                continue;
            }

            MapMarker marker = candidateDeposit.GetComponent<MapMarker>();
            if (marker == null || marker.markerType != MapMarkerType.Planet)
            {
                continue;
            }

            DiscoveryState discovery = candidateDeposit.GetComponent<DiscoveryState>();
            if (discovery != null && !discovery.discovered)
            {
                continue;
            }

            deposit = candidateDeposit;
            planetTransform = candidateDeposit.transform;
            surfaceNormal = CursorSurfaceNormal(planetTransform.position, cursorWorldPosition);
            return true;
        }

        deposit = null;
        planetTransform = null;
        surfaceNormal = Vector2.up;
        return false;
    }

   private bool IsPlacementAllowedAtTarget(ResourceDeposit targetDeposit)
{
    if (targetDeposit == null)
    {
        return false;
    }

    if (!BuildingCatalog.IsValidPlacementTarget(activeBuildingType, targetDeposit))
    {
        return false;
    }

    if (!BuildingCatalog.CanPlaceOnDeposit(activeBuildingType, targetDeposit))
    {
        return false;
    }

    if (placementMode == PlacementMode.MoveBuilding && movingBuilding != null && !movingBuilding.CanRelocateTo(targetDeposit))
    {
        return false;
    }

    return true;
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

    private Vector2 CursorSurfaceNormal(Vector2 planetPosition, Vector2 cursorPosition)
    {
        Vector2 delta = cursorPosition - planetPosition;
        if (delta.sqrMagnitude < 0.001f)
        {
            return Vector2.up;
        }

        return delta.normalized;
    }

    private void UpdateGhost(Vector2 cursorPosition, bool hasTarget, bool canPlaceHere, Transform planetTransform, Vector2 surfaceNormal)
    {
        EnsureGhost(activeBuildingType);
        ghostRenderer.enabled = true;

        if (!hasTarget || planetTransform == null)
        {
            ghostObject.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, ghostObject.transform.position.z);
            ghostObject.transform.rotation = Quaternion.identity;
            ghostRenderer.color = new Color(ghostRenderer.color.r, ghostRenderer.color.g, ghostRenderer.color.b, ghostAlpha);

            return;
        }

        PlanetSurfaceAnchor.ApplySurfacePose(ghostObject.transform, planetTransform, surfaceNormal, ghostObject.transform.position.z);
        Color tint = BuildingCatalog.GetDefinition(activeBuildingType).tint;
        float alpha = canPlaceHere ? ghostAlpha : Mathf.Clamp01(ghostAlpha * 0.9f);
        ghostRenderer.color = new Color(tint.r, tint.g, tint.b, alpha);
    }

    private void EnsureGhost(BuildingType buildingType)
    {
        BuildingDefinition definition = BuildingCatalog.GetDefinition(buildingType);

        if (ghostObject == null)
        {
            ghostObject = new GameObject(definition.displayName + " Ghost");
            ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
            ghostRenderer.sortingOrder = 80;
        }

        ghostObject.name = definition.displayName + " Ghost";
        ghostObject.SetActive(true);

        ghostObject.transform.localScale = Vector3.one * definition.visualScale;
        ghostRenderer.sprite = definition.placeholderSprite;
        ghostRenderer.color = new Color(definition.tint.r, definition.tint.g, definition.tint.b, ghostAlpha);
        ghostRenderer.enabled = true;
    }

    private void CancelPlacement()
    {
        placementMode = PlacementMode.None;
        movingBuilding = null;
        DestroyGhost();
    }

    private void DestroyGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
            ghostRenderer = null;
        }
    }
}
