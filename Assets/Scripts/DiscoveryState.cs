using UnityEngine;

public class DiscoveryState : MonoBehaviour
{
    public bool discovered;
    public float passiveRevealRadius = 5f;

    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        ApplyVisibility();
    }

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
        ApplyVisibility();
    }

    public void SetDiscovered(bool value)
    {
        discovered = value;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = spriteRenderers[i].color;
            color.a = discovered ? 1f : 0.18f;
            spriteRenderers[i].color = color;
        }
    }
}
