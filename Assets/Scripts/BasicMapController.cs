using System.Collections.Generic;
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
    private PlayerRadarPing radarPing;
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

        if (radarPing == null && playerInventory != null)
        {
            radarPing = playerInventory.GetComponent<PlayerRadarPing>();
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
        MapMarker playerMarker = null;
        for (int i = 0; i < markers.Length; i++)
        {
            if (markers[i] != null && markers[i].markerType == MapMarkerType.Player)
            {
                playerMarker = markers[i];
                continue;
            }

            DrawMarker(markers[i], rect, includeLabels);
        }

        if (playerMarker != null)
        {
            DrawRadarOverlay(rect, playerMarker);
            DrawMarker(playerMarker, rect, includeLabels);
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

    private void DrawRadarOverlay(Rect rect, MapMarker playerMarker)
    {
        if (radarPing == null || playerMarker == null)
        {
            return;
        }

        Vector2 playerPosition = WorldToMapPosition(playerMarker.transform.position, rect, generator.EffectiveSystemRadius);
        float progress = radarPing.PulseProgress;
        if (progress < 1f)
        {
            float easedProgress = 1f - ((1f - progress) * (1f - progress));
            float mapRadius = (radarPing.pingRange / Mathf.Max(1f, generator.EffectiveSystemRadius)) * (Mathf.Min(rect.width, rect.height) * 0.5f);
            Color pulseColor = new Color(0.22f, 0.95f, 1f, Mathf.Lerp(0.85f, 0.05f, progress));
            DrawCircle(playerPosition, mapRadius * easedProgress, pulseColor, 80);
        }

        IReadOnlyList<RadarContact> contacts = radarPing.ActiveContacts;
        for (int i = 0; i < contacts.Count; i++)
        {
            RadarContact contact = contacts[i];
            if (!PlayerRadarPing.IsContactActive(Time.time, contact.expiresAt))
            {
                continue;
            }

            DrawRadarContact(contact, rect);
        }
    }

    private void DrawRadarContact(RadarContact contact, Rect rect)
    {
        Vector2 position = WorldToMapPosition(contact.worldPosition, rect, generator.EffectiveSystemRadius);
        float size = Mathf.Max(5f, MarkerSize(contact.markerType) * 0.85f);
        Color color = RadarContactColor(contact);

        DrawRect(new Rect(position.x - (size * 0.5f), position.y - (size * 0.5f), size, size), color);
        DrawRectOutline(new Rect(position.x - (size * 0.8f), position.y - (size * 0.8f), size * 1.6f, size * 1.6f), new Color(color.r, color.g, color.b, 0.45f), 1f);
    }

    private Color RadarContactColor(RadarContact contact)
    {
        switch (contact.markerType)
        {
            case MapMarkerType.Star:
                return new Color(1f, 0.76f, 0.22f, 0.95f);
            case MapMarkerType.Planet:
                return new Color(0.28f, 0.95f, 1f, 0.95f);
            case MapMarkerType.Collector:
                return new Color(0.45f, 0.95f, 1f, 0.95f);
            case MapMarkerType.Hub:
                return new Color(1f, 0.86f, 0.38f, 0.95f);
            default:
                return new Color(Mathf.Max(0.48f, contact.color.r), Mathf.Max(0.82f, contact.color.g), Mathf.Max(0.9f, contact.color.b), 0.9f);
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

        Vector2 position = WorldToMapPosition(marker.transform.position, rect, generator.EffectiveSystemRadius);
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
            case MapMarkerType.DysonSatellite:
                return 5f;
            case MapMarkerType.Collector:
                return 5f;
            case MapMarkerType.Hub:
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
