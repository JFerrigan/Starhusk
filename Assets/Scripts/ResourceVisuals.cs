using UnityEngine;

public static class ResourceVisuals
{
    public static Color ColorFor(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Ice:
                return new Color(0.45f, 0.9f, 1f);
            case ResourceType.Silicate:
                return new Color(0.75f, 0.72f, 0.95f);
            case ResourceType.Copper:
                return new Color(0.95f, 0.45f, 0.2f);
            case ResourceType.Biomass:
                return new Color(0.42f, 0.95f, 0.44f);
            default:
                return new Color(0.6f, 0.58f, 0.52f);
        }
    }
}
