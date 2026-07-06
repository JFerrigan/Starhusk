using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRadarPing : MonoBehaviour
{
    public float pingRange = 520f;
    public float cooldownSeconds = 15f;
    public float pulseDuration = 0.45f;
    public float contactDuration = 15f;

    private const int PulseSegments = 96;

    private readonly List<RadarContact> contacts = new List<RadarContact>();
    private LineRenderer pulseRenderer;
    private float lastPingTime = -999f;
   private Vector2 nearestPlanetPosition;
private Vector2 nearestStarPosition;
private Vector2 nearestAsteroidPosition;
private bool hasNearestPlanet;
private bool hasNearestStar;
private bool hasNearestAsteroid;

    public IReadOnlyList<RadarContact> ActiveContacts => contacts;
    public bool IsReady => IsReadyAtTime(Time.time, lastPingTime, cooldownSeconds);
    public float CooldownRemaining => Mathf.Max(0f, (lastPingTime + cooldownSeconds) - Time.time);
    public float LastPingTime => lastPingTime;
    public float PulseProgress => CalculatePulseProgress(Time.time, lastPingTime, pulseDuration);
    public bool HasActivePing => Time.time <= lastPingTime + contactDuration;

    private void Awake()
    {
        CreatePulseRenderer();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Ping();
        }

        PruneExpiredContacts(Time.time);
        UpdatePulseVisual();
    }

    public int Ping()
    {
        if (!IsReady)
        {
            return 0;
        }

        lastPingTime = Time.time;
        contacts.Clear();
        hasNearestPlanet = false;
hasNearestStar = false;
hasNearestAsteroid = false;

        MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);
        float nearestPlanetDistance = float.MaxValue;
float nearestStarDistance = float.MaxValue;
float nearestAsteroidDistance = float.MaxValue;

        for (int i = 0; i < markers.Length; i++)
        {
            MapMarker marker = markers[i];
            if (marker == null || marker.markerType == MapMarkerType.Player)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, marker.transform.position);
            if (distance > pingRange)
            {
                continue;
            }

            contacts.Add(new RadarContact(
                marker,
                marker.transform.position,
                marker.markerType,
                marker.markerColor,
                lastPingTime + contactDuration
            ));

           if (marker.markerType == MapMarkerType.Asteroid && distance < nearestAsteroidDistance)
{
    nearestAsteroidDistance = distance;
    nearestAsteroidPosition = marker.transform.position;
    hasNearestAsteroid = true;
}
else if (marker.markerType == MapMarkerType.Planet && distance < nearestPlanetDistance)
{
    nearestPlanetDistance = distance;
    nearestPlanetPosition = marker.transform.position;
    hasNearestPlanet = true;
}
else if (marker.markerType == MapMarkerType.Star && distance < nearestStarDistance)
{
    nearestStarDistance = distance;
    nearestStarPosition = marker.transform.position;
    hasNearestStar = true;
}
        }

        return contacts.Count;
    }

    public bool TryGetPointer(MapMarkerType markerType, out Vector2 worldPosition)
    {
        if (!HasActivePing)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        if (markerType == MapMarkerType.Asteroid && hasNearestAsteroid)
{
    worldPosition = nearestAsteroidPosition;
    return true;
}

if (markerType == MapMarkerType.Planet && hasNearestPlanet)
{
    worldPosition = nearestPlanetPosition;
    return true;
}

if (markerType == MapMarkerType.Star && hasNearestStar)
{
    worldPosition = nearestStarPosition;
    return true;
}

        worldPosition = Vector2.zero;
        return false;
    }

    public void PruneExpiredContacts(float now)
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            if (!IsContactActive(now, contacts[i].expiresAt))
            {
                contacts.RemoveAt(i);
            }
        }
    }

    public static bool IsReadyAtTime(float now, float lastPingTime, float cooldownSeconds)
    {
        return now >= lastPingTime + Mathf.Max(0f, cooldownSeconds);
    }

    public static float CalculatePulseProgress(float now, float lastPingTime, float pulseDuration)
    {
        if (pulseDuration <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01((now - lastPingTime) / pulseDuration);
    }

    public static bool IsContactActive(float now, float expiresAt)
    {
        return now <= expiresAt;
    }

    private void CreatePulseRenderer()
    {
        GameObject pulseObject = new GameObject("Radar Ping Pulse");
        pulseObject.transform.SetParent(transform, false);

        pulseRenderer = pulseObject.AddComponent<LineRenderer>();
        pulseRenderer.useWorldSpace = true;
        pulseRenderer.loop = true;
        pulseRenderer.positionCount = PulseSegments;
        pulseRenderer.widthMultiplier = 0.2f;
        pulseRenderer.sortingOrder = 500;
        pulseRenderer.enabled = false;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        pulseRenderer.material = new Material(shader);
    }

    private void UpdatePulseVisual()
    {
        if (pulseRenderer == null)
        {
            return;
        }

        float progress = PulseProgress;
        if (progress >= 1f)
        {
            pulseRenderer.enabled = false;
            return;
        }

        pulseRenderer.enabled = true;
        float easedProgress = 1f - ((1f - progress) * (1f - progress));
        float radius = pingRange * easedProgress;
        Vector3 center = transform.position;

        for (int i = 0; i < PulseSegments; i++)
        {
            float angle = ((float)i / PulseSegments) * Mathf.PI * 2f;
            pulseRenderer.SetPosition(
                i,
                new Vector3(
                    center.x + (Mathf.Cos(angle) * radius),
                    center.y + (Mathf.Sin(angle) * radius),
                    center.z
                )
            );
        }

        Color color = new Color(0.25f, 0.95f, 1f, Mathf.Lerp(0.85f, 0f, progress));
        pulseRenderer.startColor = color;
        pulseRenderer.endColor = color;
    }
}

public struct RadarContact
{
    public MapMarker marker;
    public Vector2 worldPosition;
    public MapMarkerType markerType;
    public Color color;
    public float expiresAt;

    public RadarContact(MapMarker marker, Vector2 worldPosition, MapMarkerType markerType, Color color, float expiresAt)
    {
        this.marker = marker;
        this.worldPosition = worldPosition;
        this.markerType = markerType;
        this.color = color;
        this.expiresAt = expiresAt;
    }
}
