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
    public Color autopilotRouteColor = new Color(0.35f, 1f, 0.78f, 0.85f);
    public Color autopilotDestinationColor = new Color(1f, 0.92f, 0.35f, 0.95f);

    private bool fullMapOpen;
    private StarSystemGenerator generator;
    private ResourceInventory playerInventory;
    private PlayerAutopilotController autopilot;
    private Texture2D pixel;
    private GUIStyle labelStyle;

    public bool IsFullMapOpen => fullMapOpen;

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
            ToggleFullMap();
        }

        if (generator == null)
        {
            generator = FindFirstObjectByType<StarSystemGenerator>();
        }

        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<ResourceInventory>();
        }

        if (autopilot == null && playerInventory != null)
        {
            autopilot = playerInventory.GetComponent<PlayerAutopilotController>();
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

        if (fullMapOpen)
        {
            Rect fullMapRect = new Rect(
                fullMapMargin,
                fullMapMargin,
                Mathf.Max(160f, Screen.width - (fullMapMargin * 2f)),
                Mathf.Max(160f, Screen.height - (fullMapMargin * 2f))
            );

            DrawMap(fullMapRect, fullMapBackground, true);
            HandleFullMapClick(fullMapRect);
            return;
        }

        DrawMap(minimapRect, minimapBackground, false);
        HandleMinimapClick(minimapRect);
    }

    private void ToggleFullMap()
    {
        fullMapOpen = !fullMapOpen;
    }

    public void OpenFullMap()
    {
        fullMapOpen = true;
    }

    public bool CloseFullMap()
    {
        if (!fullMapOpen)
        {
            return false;
        }

        fullMapOpen = false;
        return true;
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

    public static Vector2 MapToWorldPosition(Vector2 mapPosition, Rect mapRect, float systemRadius)
    {
        float radius = Mathf.Max(systemRadius, 1f);
        float normalizedX = (mapPosition.x - mapRect.center.x) / (mapRect.width * 0.5f);
        float normalizedY = (mapRect.center.y - mapPosition.y) / (mapRect.height * 0.5f);
        return Vector2.ClampMagnitude(new Vector2(normalizedX, normalizedY), 1f) * radius;
    }

    public static bool TryGetAutopilotDestinationFromMapClick(Vector2 mousePosition, Rect mapRect, float systemRadius, out Vector2 destination)
    {
        return TryGetAutopilotDestinationFromMapClick(mousePosition, mapRect, systemRadius, IsUpgradeUnlocked(UpgradeId.AutopilotUnlock), out destination);
    }

    public static bool TryGetAutopilotDestinationFromMapClick(Vector2 mousePosition, Rect mapRect, float systemRadius, bool autopilotUnlocked, out Vector2 destination)
    {
        destination = Vector2.zero;
        if (!autopilotUnlocked || !mapRect.Contains(mousePosition) || FullMapUiRect(mapRect).Contains(mousePosition))
        {
            return false;
        }

        destination = MapToWorldPosition(mousePosition, mapRect, systemRadius);
        return true;
    }

    private static bool IsUpgradeUnlocked(UpgradeId upgradeId)
    {
        PlayerUpgradeState state = PlayerUpgradeState.Current;
        return state != null && state.IsUnlocked(upgradeId);
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

            if (markers[i] != null && !markers[i].CanAppearOnMapAndRadar)
            {
                continue;
            }

            DrawMarker(markers[i], rect, includeLabels);
        }

        if (playerMarker != null)
        {
            DrawMarker(playerMarker, rect, includeLabels);
        }

        if (includeLabels)
        {
            DrawAutopilotRoute(rect);
        }

        if (includeLabels)
        {
            GUI.Label(
                FullMapUiRect(rect),
                "System Map  Seed " + generator.seed + "\nPress M to close\nLeft-click destination. Manual controls cancel autopilot.",
                labelStyle
            );
        }
    }

    private void HandleFullMapClick(Rect fullMapRect)
    {
        Event current = Event.current;
        if (current == null || current.type != EventType.MouseDown || current.button != 0)
        {
            return;
        }

        if (autopilot == null || generator == null)
        {
            return;
        }

        Vector2 destination;
        if (!TryGetAutopilotDestinationFromMapClick(current.mousePosition, fullMapRect, generator.EffectiveSystemRadius, out destination))
        {
            return;
        }

        autopilot.SetDestination(destination);
        current.Use();
    }

    private void HandleMinimapClick(Rect minimapRect)
    {
        Event current = Event.current;
        if (current == null || current.type != EventType.MouseDown || current.button != 0)
        {
            return;
        }

        if (!minimapRect.Contains(current.mousePosition))
        {
            return;
        }

        OpenFullMap();
        current.Use();
    }

    private void DrawAutopilotRoute(Rect rect)
    {
        if (autopilot == null || !autopilot.HasDestination)
        {
            return;
        }

        IReadOnlyList<Vector2> route = autopilot.Waypoints;
        if (route.Count >= 2)
        {
            Vector2 previous = WorldToMapPosition(route[0], rect, generator.EffectiveSystemRadius);
            for (int i = 1; i < route.Count; i++)
            {
                Vector2 next = WorldToMapPosition(route[i], rect, generator.EffectiveSystemRadius);
                DrawLine(previous, next, autopilotRouteColor, 2f);
                DrawRect(new Rect(next.x - 3f, next.y - 3f, 6f, 6f), autopilotRouteColor);
                previous = next;
            }
        }

        Vector2 destination = WorldToMapPosition(autopilot.Destination, rect, generator.EffectiveSystemRadius);
        DrawCircle(destination, 8f, autopilotDestinationColor, 24);
        DrawLine(destination + new Vector2(-6f, 0f), destination + new Vector2(6f, 0f), autopilotDestinationColor, 2f);
        DrawLine(destination + new Vector2(0f, -6f), destination + new Vector2(0f, 6f), autopilotDestinationColor, 2f);
    }

    private static Rect FullMapUiRect(Rect mapRect)
    {
        return new Rect(mapRect.x + 12f, mapRect.y + 10f, 380f, 64f);
    }

    private void DrawMarker(MapMarker marker, Rect rect, bool includeLabels)
    {
        if (marker == null)
        {
            return;
        }

        if (!marker.CanAppearOnMapAndRadar)
        {
            return;
        }

        bool visible = marker.IsVisible;
        if (!visible)
        {
            return;
        }

        Vector2 position = WorldToMapPosition(marker.transform.position, rect, generator.EffectiveSystemRadius);
        float size = MarkerSize(marker.markerType) * Mathf.Max(0.5f, marker.iconScale) * (includeLabels ? 1.25f : 1f);

        if (marker.markerType == MapMarkerType.Player)
        {
            DrawPlayerMarker(position, size, marker.markerColor, marker.transform.eulerAngles.z);
        }
        else
        {
            DrawRect(new Rect(position.x - (size * 0.5f), position.y - (size * 0.5f), size, size), marker.markerColor);
        }

        if (includeLabels && marker.markerType != MapMarkerType.Asteroid)
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
            case MapMarkerType.Pirate:
                return 7f;
            case MapMarkerType.DysonSatellite:
                return 5f;
            case MapMarkerType.Collector:
                return 5f;
            case MapMarkerType.Hub:
                return 7f;
            case MapMarkerType.PowerRelay:
                return 6f;
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
