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
        EnsureGenerator();
        EnsureCamera();
        EnsureMapController();
    }

    private void EnsurePlayer()
    {
        ResourceInventory inventory = FindFirstObjectByType<ResourceInventory>();
        GameObject playerObject;

        if (inventory == null)
        {
            playerObject = new GameObject("PlayerShip");
            playerObject.transform.position = new Vector3(0f, -10f, 0f);
            playerObject.transform.localScale = new Vector3(0.6f, 0.9f, 1f);

            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprites.Circle;
            renderer.color = new Color(0.55f, 0.9f, 1f);

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

        if (playerObject.GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprites.Circle;
            renderer.color = new Color(0.55f, 0.9f, 1f);
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

    private void EnsureMapController()
    {
        if (FindFirstObjectByType<BasicMapController>() != null)
        {
            return;
        }

        GameObject mapObject = new GameObject("BasicMapController");
        mapObject.AddComponent<BasicMapController>();
    }
}
