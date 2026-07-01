using UnityEngine;
using UnityEngine.InputSystem;

public class MineableAsteroid : MonoBehaviour
{
    public int oreAmount = 5;

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
}
