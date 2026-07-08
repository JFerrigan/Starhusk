using UnityEngine;

[RequireComponent(typeof(ShipHealth))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ResourceInventory))]
public class PlayerRespawnController : MonoBehaviour
{
    private static readonly Vector2 DefaultHomeSpawn = new Vector2(0f, -10f);

    private ShipHealth health;
    private Rigidbody2D rb;
    private ResourceInventory inventory;
    private bool hasHomeSpawn;
    private Vector2 homeSpawn;
    private bool isRespawning;

    public bool HasHomeSpawn => hasHomeSpawn;
    public Vector2 HomeSpawn => hasHomeSpawn ? homeSpawn : DefaultHomeSpawn;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();
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

    public void SetHomeSpawn(Vector2 spawnPosition)
    {
        homeSpawn = spawnPosition;
        hasHomeSpawn = true;
    }

    private void HandleDeath(ShipHealth deadHealth)
    {
        if (isRespawning)
        {
            return;
        }

        isRespawning = true;

        if (inventory != null)
        {
            inventory.ClearResources();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Vector2 spawnPosition = hasHomeSpawn ? homeSpawn : DefaultHomeSpawn;
        transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);

        if (health != null)
        {
            health.HealToFull();
        }

        isRespawning = false;
    }

    private void CacheComponents()
    {
        if (health == null)
        {
            health = GetComponent<ShipHealth>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (inventory == null)
        {
            inventory = GetComponent<ResourceInventory>();
        }
    }
}
