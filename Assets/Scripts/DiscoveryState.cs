using UnityEngine;

public class DiscoveryState : MonoBehaviour
{
    public bool discovered;
    public float passiveRevealRadius = 5f;

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
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
        renderers = GetComponentsInChildren<Renderer>();

        if (renderers == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null)
            {
                continue;
            }

            Color color = renderers[i].material.color;
            color.a = discovered ? 1f : 0.18f;
            renderers[i].material.color = color;
        }
    }
}
