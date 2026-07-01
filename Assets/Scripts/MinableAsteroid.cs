using UnityEngine;
using UnityEngine.InputSystem;

public class MineableAsteroid : MonoBehaviour
{
    public int oreAmount = 5;
    public float interactionRadius = 0.82f;

    private bool playerNearby = false;
    private ResourceInventory nearbyInventory;
    private ResourceDeposit deposit;

    private void Awake()
    {
        deposit = GetComponent<ResourceDeposit>();

        if (deposit == null)
        {
            deposit = gameObject.AddComponent<ResourceDeposit>();
            deposit.resourceType = ResourceType.Ore;
            deposit.maxAmount = Mathf.Max(oreAmount, 1);
            deposit.remainingAmount = deposit.maxAmount;
            deposit.mineAmountPerInteraction = Mathf.Max(oreAmount, 1);
        }

        EnsureInteractionTrigger();
    }

    private void Update()
    {
        if (Keyboard.current != null && playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Mine();
        }
    }

    public void Mine()
    {
        if (nearbyInventory == null)
        {
            return;
        }

        int minedAmount = deposit.Mine(nearbyInventory);
        if (minedAmount > 0)
        {
            Debug.Log("Mined " + minedAmount + " " + deposit.resourceType);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ResourceInventory inventory = other.GetComponent<ResourceInventory>();

        if (inventory != null)
        {
            playerNearby = true;
            nearbyInventory = inventory;
            Debug.Log("Press E to mine " + deposit.resourceType);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ResourceInventory inventory = other.GetComponent<ResourceInventory>();

        if (inventory != null)
        {
            playerNearby = false;
            nearbyInventory = null;
        }
    }

    private void EnsureInteractionTrigger()
    {
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].isTrigger)
            {
                colliders[i].radius = Mathf.Max(colliders[i].radius, interactionRadius);
                return;
            }
        }

        CircleCollider2D trigger = gameObject.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = interactionRadius;
    }
}
