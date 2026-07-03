using UnityEngine;

public class DiscoveryState : MonoBehaviour
{
    public bool discovered;
    public float passiveRevealRadius = 5f;

    private SpriteRenderer[] spriteRenderers;
    private MeshRenderer[] meshRenderers;

    private void Awake()
    {
        CacheRenderers();
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
        CacheRenderers();

        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null)
                {
                    continue;
                }

                Color color = spriteRenderers[i].color;
                color.a = discovered ? 1f : 0.18f;
                spriteRenderers[i].color = color;
            }
        }

        if (meshRenderers != null)
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] == null || meshRenderers[i].sharedMaterial == null)
                {
                    continue;
                }

                Color color = meshRenderers[i].sharedMaterial.color;
                color.a = discovered ? 1f : 0.18f;
                meshRenderers[i].sharedMaterial.color = color;
            }
        }
    }

    private void CacheRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
}
