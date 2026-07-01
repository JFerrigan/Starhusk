using UnityEngine;
using UnityEngine.InputSystem;

public class BasicMapController : MonoBehaviour
{
    public float minimapSize = 180f;
    public float minimapMargin = 16f;
    public float fullMapMargin = 48f;
    public Color minimapBackground = new Color(0.02f, 0.04f, 0.07f, 0.82f);
    public Color fullMapBackground = new Color(0.01f, 0.02f, 0.04f, 0.94f);
    public Color boundaryColor = new Color(0.35f, 0.58f, 0.72f, 0.75f);
    public Color undiscoveredColor = new Color(0.45f, 0.48f, 0.52f, 0.35f);

    private bool fullMapOpen;
    private StarSystemGenerator generator;
    private ResourceInventory playerInventory;
    private Texture2D pixel;
    private GUIStyle labelStyle;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        labelStyle = new GUIStyle
        {
            fontSize = 13,
            normal = { textColor = Color.white },
            alignment = TextAnchor.UpperLeft
        };
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            fullMapOpen = !fullMapOpen;
        }

        if (generator == null)
        {
            generator = FindFirstObjectByType<StarSystemGenerator>();
        }

        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<ResourceInventory>();
        }
    }

    private void OnGUI()
    {
        if (generator == null)
        {
            return;
        }

        Rect minimapRect = new Rect(
            Screen.width - minimapSize - minimapMargin,
            minimapMargin,
            minimapSize,
            minimapSize
        );

        DrawMap(minimapRect, minimapBackground, false);

        if (fullMapOpen)
        {
            float size = Mathf.Min(Screen.width, Screen.height) - (fullMapMargin * 2f);
            Rect fullMapRect = new Rect(
                (Screen.width - size) * 0.5f,
                (Screen.height - size) * 0.5f,
                size,
                size
            );

            DrawMap(fullMapRect, fullMapBackground, true);
        }
    }

    public static Vector2 WorldToMapPosition(Vector2 worldPosition, Rect mapRect, float systemRadius)
    {
        float radius = Mathf.Max(systemRadius, 1f);
        Vector2 normalized = worldPosition / radius;
        normalized = Vector2.ClampMagnitude(normalized, 1f);

        return new Vector2(
            mapRect.center.x + (normalized.x * mapRect.width * 0.5f),
            mapRect.center.y - (normalized.y * mapRect.height * 0.5f)
        );
    }

    private void DrawMap(Rect rect, Color backgroundColor, bool includeLabels)
    {
        DrawRect(rect, backgroundColor);
        DrawRectOutline(rect, new Color(0.7f, 0.9f, 1f, 0.35f), 2f);

        float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
        DrawCircle(rect.center, radius, boundaryColor, 96);

        MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);
        for (int i = 0; i < markers.Length; i++)
        {
            DrawMarker(markers[i], rect, includeLabels);
        }

        if (includeLabels)
        {
            GUI.Label(
                new Rect(rect.x + 12f, rect.y + 10f, 260f, 42f),
                "System Map  Seed " + generator.seed + "\nPress M to close",
                labelStyle
            );
        }
    }

    private void DrawMarker(MapMarker marker, Rect rect, bool includeLabels)
    {
        if (marker == null)
        {
            return;
        }

        bool visible = marker.IsVisible;
        if (!visible && !includeLabels)
        {
            return;
        }

        Vector2 position = WorldToMapPosition(marker.transform.position, rect, generator.systemRadius);
        float size = MarkerSize(marker.markerType) * Mathf.Max(0.5f, marker.iconScale) * (includeLabels ? 1.25f : 1f);
        Color color = visible ? marker.markerColor : undiscoveredColor;

        if (marker.markerType == MapMarkerType.Player)
        {
            DrawPlayerMarker(position, size, color, marker.transform.eulerAngles.z);
        }
        else
        {
            DrawRect(new Rect(position.x - (size * 0.5f), position.y - (size * 0.5f), size, size), color);
        }

        if (includeLabels && visible && marker.markerType != MapMarkerType.Asteroid)
        {
            GUI.Label(new Rect(position.x + size, position.y - 8f, 160f, 22f), marker.name, labelStyle);
        }
    }

    private float MarkerSize(MapMarkerType markerType)
    {
        switch (markerType)
        {
            case MapMarkerType.Player:
                return 8f;
            case MapMarkerType.Star:
                return 12f;
            case MapMarkerType.Planet:
                return 7f;
            default:
                return 4f;
        }
    }

    private void DrawPlayerMarker(Vector2 center, float size, Color color, float zRotation)
    {
        DrawRect(new Rect(center.x - (size * 0.5f), center.y - (size * 0.5f), size, size), color);

        float radians = (zRotation + 90f) * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians));
        DrawLine(center, center + (direction * size * 1.25f), Color.white, 2f);
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    private void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }

    private void DrawCircle(Vector2 center, float radius, Color color, int segments)
    {
        Vector2 previous = center + new Vector2(radius, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;
            Vector2 next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            DrawLine(previous, next, color, 1f);
            previous = next;
        }
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;

        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - (width * 0.5f), delta.magnitude, width), pixel);

        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }
}
