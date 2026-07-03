using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapScene()
    {
        if (FindFirstObjectByType<GameBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("GameBootstrap");
        bootstrapObject.AddComponent<GameBootstrap>();
        bootstrapObject.AddComponent<FoundationHud>();
    }

    private void Awake()
    {
        EnsurePlayer();
        EnsureCompanionAutomaton();
        EnsureGenerator();
        EnsureCamera();
        EnsureSpaceBackground();
        EnsureMapController();
        EnsureHoverNameDisplay();
        EnsureBuildOptionsMenu();
        HideLegacyResourceUI();
        EnsureBuildingControllers();
        EnsureAutomatonControllers();
    }

    private void EnsurePlayer()
    {
        ResourceInventory inventory = FindFirstObjectByType<ResourceInventory>();
        GameObject playerObject;

        if (inventory == null)
{
    playerObject = new GameObject("PlayerShip");
    playerObject.transform.position = new Vector3(0f, -10f, 0f);
    playerObject.transform.localScale = Vector3.one * 8f;

    Rigidbody2D createdRb = playerObject.AddComponent<Rigidbody2D>();
    createdRb.gravityScale = 0f;
    createdRb.linearDamping = 0f;
    createdRb.angularDamping = 2f;

    playerObject.AddComponent<BoxCollider2D>();
    playerObject.AddComponent<PlayerMovement>();
    inventory = playerObject.AddComponent<ResourceInventory>();
    playerObject.AddComponent<PlayerScanner>();
}
else
{
    playerObject = inventory.gameObject;
}

playerObject.transform.localScale = Vector3.one * 15f;

SpriteRenderer renderer = EnsurePlayerVisual(playerObject);
if (renderer == null)
{
    renderer = playerObject.AddComponent<SpriteRenderer>();
}

renderer.sprite = LoadPlayerShipSprite();
renderer.color = Color.white;

BoxCollider2D box = playerObject.GetComponent<BoxCollider2D>();
if (box == null)
{
    box = playerObject.AddComponent<BoxCollider2D>();
}

if (renderer.sprite != null)
{
    box.size = renderer.sprite.bounds.size * 0.55f;
    box.offset = Vector2.zero;
}
        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = playerObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = Mathf.Max(rb.angularDamping, 2f);

        if (playerObject.GetComponent<Collider2D>() == null)
        {
            playerObject.AddComponent<BoxCollider2D>();
        }

        if (inventory.GetComponent<PlayerScanner>() == null)
        {
            inventory.gameObject.AddComponent<PlayerScanner>();
        }

        if (inventory.GetComponent<PlayerMovement>() == null)
        {
            inventory.gameObject.AddComponent<PlayerMovement>();
        }

        if (inventory.GetComponent<PlayerWeaponController>() == null)
        {
            inventory.gameObject.AddComponent<PlayerWeaponController>();
        }

        EnsurePrototypeResources(inventory);

        if (inventory.GetComponent<ObjectIdentity>() == null)
        {
            ObjectNamer.AssignIdentity(inventory.gameObject, "Player Ship", ObjectIdentityCategory.ManMade);
        }

        if (inventory.GetComponent<PlayerRadarPing>() == null)
        {
            inventory.gameObject.AddComponent<PlayerRadarPing>();
        }

        if (inventory.GetComponent<MapMarker>() == null)
        {
            MapMarker marker = inventory.gameObject.AddComponent<MapMarker>();
            marker.markerType = MapMarkerType.Player;
            marker.markerColor = new Color(0.25f, 1f, 0.95f);
            marker.iconScale = 1.2f;
            marker.requireDiscovery = false;
        }

        if (inventory.transform.position.sqrMagnitude < 1f)
        {
            inventory.transform.position = new Vector3(0f, -10f, 0f);
        }
    }

    private void EnsurePrototypeResources(ResourceInventory inventory)
    {
        EnsureMinimumResource(inventory, ResourceType.Ore, 1000);
        EnsureMinimumResource(inventory, ResourceType.Ice, 1000);
        EnsureMinimumResource(inventory, ResourceType.Silicate, 1000);
        EnsureMinimumResource(inventory, ResourceType.Copper, 1000);
        EnsureMinimumResource(inventory, ResourceType.Biomass, 1000);
    }

    private void EnsureCompanionAutomaton()
    {
        if (FindFirstObjectByType<CompanionAutomaton>() != null)
        {
            return;
        }

        ResourceInventory player = FindFirstObjectByType<ResourceInventory>();
        if (player == null)
        {
            return;
        }

        GameObject companionObject = new GameObject("Companion Automaton");
        ObjectNamer.AssignIdentity(companionObject, "Companion Automaton", ObjectIdentityCategory.ManMade);
        companionObject.transform.position = player.transform.position + new Vector3(-6f, -2f, 0f);
        companionObject.transform.localScale = Vector3.one * 4.6f;
        companionObject.AddComponent<CompanionAutomaton>();

        MapMarker marker = companionObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Collector;
        marker.markerColor = new Color(0.35f, 1f, 0.72f);
        marker.iconScale = 0.9f;
        marker.requireDiscovery = false;
    }

    private void EnsureMinimumResource(ResourceInventory inventory, ResourceType type, int minimum)
    {
        if (inventory == null)
        {
            return;
        }

        int currentAmount = inventory.GetAmount(type);
        if (currentAmount < minimum)
        {
            inventory.AddResource(type, minimum - currentAmount);
        }
    }

    private void EnsureGenerator()
    {
        if (FindFirstObjectByType<StarSystemGenerator>() != null)
        {
            return;
        }

        GameObject generatorObject = new GameObject("StarSystemGenerator");
        generatorObject.AddComponent<StarSystemGenerator>();
    }

    private void EnsureCamera()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(camera.orthographicSize, 16f);
        camera.backgroundColor = new Color(0.005f, 0.008f, 0.015f);
        camera.allowMSAA = true;

        CameraFollow follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }

        follow.followAcceleration = Mathf.Max(follow.followAcceleration, 55f);
        follow.velocityDamping = Mathf.Max(follow.velocityDamping, 5f);

        if (follow.target == null)
        {
            ResourceInventory inventory = FindFirstObjectByType<ResourceInventory>();
            if (inventory != null)
            {
                follow.target = inventory.transform;
            }
        }
    }

    private void EnsureSpaceBackground()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        if (camera.GetComponent<ProceduralSpaceBackground>() == null)
        {
            camera.gameObject.AddComponent<ProceduralSpaceBackground>();
        }
    }

    private void EnsureMapController()
    {
        if (FindFirstObjectByType<BasicMapController>() != null)
        {
            return;
        }

        GameObject mapObject = new GameObject("BasicMapController");
        mapObject.AddComponent<BasicMapController>();
    }

    private void EnsureHoverNameDisplay()
    {
        if (FindFirstObjectByType<HoverNameDisplay>() != null)
        {
            return;
        }

        GameObject hoverObject = new GameObject("HoverNameDisplay");
        hoverObject.AddComponent<HoverNameDisplay>();
    }

    private void EnsureBuildOptionsMenu()
    {
        if (FindFirstObjectByType<BuildOptionsMenu>() != null)
        {
            return;
        }

        GameObject buildOptionsObject = new GameObject("BuildOptionsMenu");
        buildOptionsObject.AddComponent<BuildOptionsMenu>();
    }

    private void HideLegacyResourceUI()
    {
        ResourceUI[] legacyResourceUis = FindObjectsByType<ResourceUI>(FindObjectsSortMode.None);
        for (int i = 0; i < legacyResourceUis.Length; i++)
        {
            if (legacyResourceUis[i] != null)
            {
                legacyResourceUis[i].HideLegacyDisplay();
            }
        }
    }

    private void EnsureBuildingControllers()
    {
        if (FindFirstObjectByType<BuildingPlacementController>() == null)
        {
            GameObject placementObject = new GameObject("BuildingPlacementController");
            placementObject.AddComponent<BuildingPlacementController>();
        }

        if (FindFirstObjectByType<BuildingSelectionController>() == null)
        {
            GameObject selectionObject = new GameObject("BuildingSelectionController");
            selectionObject.AddComponent<BuildingSelectionController>();
        }

        if (FindFirstObjectByType<HiddenRoutingDisplayController>() == null)
        {
            GameObject routingObject = new GameObject("HiddenRoutingDisplayController");
            routingObject.AddComponent<HiddenRoutingDisplayController>();
        }
    }

    private void EnsureAutomatonControllers()
    {
        if (FindFirstObjectByType<AutomatonPlacementController>() != null)
        {
            return;
        }

        GameObject placementObject = new GameObject("AutomatonPlacementController");
        placementObject.AddComponent<AutomatonPlacementController>();
    }

    private static Sprite LoadPlayerShipSprite()
    {
        Sprite shipSprite = Resources.Load<Sprite>("ship");

        if (shipSprite != null)
        {
            return shipSprite;
        }

        Debug.LogWarning("Could not find Assets/Resources/ship.png. Using placeholder ship sprite.");
        return PlaceholderSprites.Circle;
    }
private static SpriteRenderer EnsurePlayerVisual(GameObject playerObject)
{
    // Remove the old SpriteRenderer from the root PlayerShip.
    // We only want the child "ShipVisual" to draw the ship.
    SpriteRenderer rootRenderer = playerObject.GetComponent<SpriteRenderer>();
    if (rootRenderer != null)
    {
        Object.Destroy(rootRenderer);
    }

    Transform visualTransform = playerObject.transform.Find("ShipVisual");

    if (visualTransform == null)
    {
        GameObject visualObject = new GameObject("ShipVisual");
        visualObject.transform.SetParent(playerObject.transform, false);
        visualTransform = visualObject.transform;
    }

    visualTransform.localPosition = Vector3.zero;

    // Rotate only the image, not the movement/collider root.
    visualTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);

    visualTransform.localScale = Vector3.one;

    SpriteRenderer renderer = visualTransform.GetComponent<SpriteRenderer>();
    if (renderer == null)
    {
        renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
    }

    renderer.sprite = LoadPlayerShipSprite();
    renderer.color = Color.white;
    renderer.sortingOrder = 100;

    return renderer;
}
}
