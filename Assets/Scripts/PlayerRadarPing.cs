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
    private readonly List<AsteroidCandidate> asteroidCandidates = new List<AsteroidCandidate>();
    private readonly List<Vector2> nearestAsteroidPositions = new List<Vector2>();
    private readonly List<Vector2> visiblePointerBuffer = new List<Vector2>();
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
    public bool HasActivePing => Time.time <= lastPingTime + contactDuration || HasPersistentContacts(Time.time);

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
        asteroidCandidates.Clear();
        nearestAsteroidPositions.Clear();
        hasNearestPlanet = false;
hasNearestStar = false;
hasNearestAsteroid = false;

        MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);
        float nearestPlanetDistance = float.MaxValue;
float nearestStarDistance = float.MaxValue;

        for (int i = 0; i < markers.Length; i++)
        {
            MapMarker marker = markers[i];
            if (marker == null || marker.markerType == MapMarkerType.Player)
            {
                continue;
            }

            if (!marker.CanAppearOnMapAndRadar)
            {
                continue;
            }

            float effectivePingRange = EffectivePingRange();
            float distance = Vector2.Distance(transform.position, marker.transform.position);
            if (distance > effectivePingRange)
            {
                continue;
            }

           if (marker.markerType == MapMarkerType.Asteroid)
{
    asteroidCandidates.Add(new AsteroidCandidate(marker, distance));
}
else if (marker.markerType == MapMarkerType.Planet && distance < nearestPlanetDistance)
{
    AddContact(marker, false);
    nearestPlanetDistance = distance;
    nearestPlanetPosition = marker.transform.position;
    hasNearestPlanet = true;
}
else if (marker.markerType == MapMarkerType.Star && distance < nearestStarDistance)
{
    AddContact(marker, false);
    nearestStarDistance = distance;
    nearestStarPosition = marker.transform.position;
    hasNearestStar = true;
}
else
{
    AddContact(marker, false);
}
        }

        AddNearestAsteroidContacts();
        return contacts.Count;
    }

    private void AddNearestAsteroidContacts()
    {
        asteroidCandidates.Sort((left, right) => left.distance.CompareTo(right.distance));
        int limit = Mathf.Min(AsteroidContactLimit(), asteroidCandidates.Count);
        bool revealResourceType = IsUpgradeUnlocked(UpgradeId.PingAsteroidResourceType);

        for (int i = 0; i < limit; i++)
        {
            MapMarker marker = asteroidCandidates[i].marker;
            if (marker == null || !marker.CanAppearOnMapAndRadar)
            {
                continue;
            }

            AddContact(marker, revealResourceType);
            Vector2 position = marker.transform.position;
            nearestAsteroidPositions.Add(position);
            if (!hasNearestAsteroid)
            {
                nearestAsteroidPosition = position;
                hasNearestAsteroid = true;
            }
        }
    }

    private void AddContact(MapMarker marker, bool revealResourceType)
    {
        bool persistentDiscovery = IsUpgradeUnlocked(UpgradeId.PersistentRadarDiscovery);
        if (persistentDiscovery)
        {
            DiscoveryState discovery = marker.GetComponent<DiscoveryState>();
            if (discovery != null)
            {
                discovery.SetDiscovered(true);
            }

            marker.hiddenFromMapAndRadar = false;
        }

        ResourceType? resourceType = null;
        if (revealResourceType)
        {
            ResourceDeposit deposit = marker.GetComponent<ResourceDeposit>();
            if (deposit != null)
            {
                resourceType = deposit.resourceType;
            }
        }

        contacts.Add(new RadarContact(
            marker,
            marker.transform.position,
            marker.markerType,
            marker.markerColor,
            persistentDiscovery ? float.PositiveInfinity : lastPingTime + contactDuration,
            resourceType
        ));
    }

    private float EffectivePingRange()
    {
        return IsUpgradeUnlocked(UpgradeId.InfiniteRadarRange) ? float.PositiveInfinity : pingRange;
    }

    private static int AsteroidContactLimit()
    {
        if (IsUpgradeUnlocked(UpgradeId.Ping10Asteroids))
        {
            return 10;
        }

        if (IsUpgradeUnlocked(UpgradeId.Ping3Asteroids))
        {
            return 3;
        }

        return 1;
    }

    private static bool IsUpgradeUnlocked(UpgradeId upgradeId)
    {
        PlayerUpgradeState state = PlayerUpgradeState.Current;
        return state != null && state.IsUnlocked(upgradeId);
    }

    public bool TryGetPointer(MapMarkerType markerType, out Vector2 worldPosition)
    {
        if (!HasActivePing)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        for (int i = 0; i < contacts.Count; i++)
        {
            RadarContact contact = contacts[i];
            if (contact.markerType == markerType && IsContactAvailable(contact, Time.time))
            {
                worldPosition = contact.worldPosition;
                return true;
            }
        }

        worldPosition = Vector2.zero;
        return false;
    }

    public IReadOnlyList<Vector2> GetPointers(MapMarkerType markerType)
    {
        if (!HasActivePing)
        {
            return System.Array.Empty<Vector2>();
        }

        if (markerType == MapMarkerType.Asteroid)
        {
            visiblePointerBuffer.Clear();
            for (int i = 0; i < contacts.Count; i++)
            {
                RadarContact contact = contacts[i];
                if (contact.markerType == MapMarkerType.Asteroid && IsContactAvailable(contact, Time.time))
                {
                    visiblePointerBuffer.Add(contact.worldPosition);
                }
            }

            return visiblePointerBuffer;
        }

        if (TryGetPointer(markerType, out Vector2 pointer))
        {
            return new[] { pointer };
        }

        return System.Array.Empty<Vector2>();
    }

    public void PruneExpiredContacts(float now)
    {
        for (int i = contacts.Count - 1; i >= 0; i--)
        {
            if (!IsContactAvailable(contacts[i], now))
            {
                contacts.RemoveAt(i);
            }
        }
    }

    private bool HasPersistentContacts(float now)
    {
        for (int i = 0; i < contacts.Count; i++)
        {
            if (float.IsPositiveInfinity(contacts[i].expiresAt) && IsContactAvailable(contacts[i], now))
            {
                return true;
            }
        }

        return false;
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

    public static bool IsContactAvailable(RadarContact contact, float now)
    {
        return IsContactActive(now, contact.expiresAt)
            && contact.marker != null
            && contact.marker.CanAppearOnMapAndRadar;
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
        float pulseRange = float.IsInfinity(EffectivePingRange()) ? pingRange * 4f : pingRange;
        float radius = pulseRange * easedProgress;
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
    public ResourceType? resourceType;

    public RadarContact(MapMarker marker, Vector2 worldPosition, MapMarkerType markerType, Color color, float expiresAt, ResourceType? resourceType = null)
    {
        this.marker = marker;
        this.worldPosition = worldPosition;
        this.markerType = markerType;
        this.color = color;
        this.expiresAt = expiresAt;
        this.resourceType = resourceType;
    }
}

public struct AsteroidCandidate
{
    public MapMarker marker;
    public float distance;

    public AsteroidCandidate(MapMarker marker, float distance)
    {
        this.marker = marker;
        this.distance = distance;
    }
}
