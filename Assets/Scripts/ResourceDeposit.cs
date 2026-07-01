using UnityEngine;

public class ResourceDeposit : MonoBehaviour
{
    public ResourceType resourceType = ResourceType.Ore;
    public int maxAmount = 100;
    public int remainingAmount = 100;
    public int mineAmountPerInteraction = 10;
    public bool destroyWhenDepleted = true;

    public bool IsDepleted => remainingAmount <= 0;

    private void Awake()
    {
        if (maxAmount <= 0)
        {
            maxAmount = Mathf.Max(1, remainingAmount);
        }

        remainingAmount = Mathf.Clamp(remainingAmount, 0, maxAmount);
        mineAmountPerInteraction = Mathf.Max(1, mineAmountPerInteraction);
    }

    public int Mine(ResourceInventory inventory)
    {
        if (inventory == null || IsDepleted)
        {
            return 0;
        }

        int minedAmount = Mathf.Min(mineAmountPerInteraction, remainingAmount);
        remainingAmount -= minedAmount;
        inventory.AddResource(resourceType, minedAmount);

        if (IsDepleted && destroyWhenDepleted)
        {
            Destroy(gameObject);
        }

        return minedAmount;
    }
}
