using UnityEngine;

public class DiscoveryState : MonoBehaviour
{
    public bool discovered;
    public float passiveRevealRadius = 5f;

    private void Update()
    {
        ResourceInventory player = FindFirstObjectByType<ResourceInventory>();

        if (!discovered && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance <= passiveRevealRadius)
            {
                Reveal();
            }
        }
    }

    public void Reveal()
    {
        if (discovered)
        {
            return;
        }

        discovered = true;
    }

    public void SetDiscovered(bool value)
    {
        discovered = value;
    }
}
