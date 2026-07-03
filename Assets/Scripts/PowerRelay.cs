using UnityEngine;

public class PowerRelay : MonoBehaviour
{
    public const int OreCost = 40;
    public const int SilicateCost = 25;
    public const float DefaultRange = 260f;

    public float range = DefaultRange;
    public Color poweredColor = new Color(1f, 0.9f, 0.36f, 1f);
    public Color unpoweredColor = new Color(0.34f, 0.36f, 0.42f, 1f);

    [SerializeField]
    private bool connected;

    private SpriteRenderer spriteRenderer;
    private MapMarker mapMarker;

    public bool IsConnected => connected;
    public float Range => Mathf.Max(0f, range);

    private void Awake()
    {
        EnsureComponents();
        ApplyVisualState();
    }

    public void SetConnected(bool value)
    {
        if (connected == value)
        {
            return;
        }

        connected = value;
        ApplyVisualState();
    }

    private void EnsureComponents()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = PlaceholderSprites.CollectorHub;
            spriteRenderer.sortingOrder = 42;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.42f;
            circle.isTrigger = true;
        }

        mapMarker = GetComponent<MapMarker>();
        if (mapMarker == null)
        {
            mapMarker = gameObject.AddComponent<MapMarker>();
            mapMarker.markerType = MapMarkerType.PowerRelay;
            mapMarker.iconScale = 1f;
            mapMarker.requireDiscovery = false;
        }
    }

    private void ApplyVisualState()
    {
        Color color = connected ? poweredColor : unpoweredColor;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        if (mapMarker != null)
        {
            mapMarker.markerColor = color;
        }
    }
}
