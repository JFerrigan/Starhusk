using System.Collections.Generic;
using UnityEngine;

public class SatelliteFactory : MonoBehaviour
{
    public const int DefaultCapacity = 5000;
    public const int OreCost = 25;
    public const int CopperCost = 25;
    public const int SilicateCost = 25;
    public const float DefaultBuildDuration = 10f;
    public const float DefaultOrbitSpeedDegrees = 5f;

    public float buildDuration = DefaultBuildDuration;
    public float orbitSpeedDegrees = DefaultOrbitSpeedDegrees;
    public float ejectDistance = 3f;

    [SerializeField]
    private ResourceStorage storage;

    [SerializeField]
    private bool isBuilding;

    [SerializeField]
    private int producedCount;

    private float buildCompleteTime;

    public ResourceStorage Storage => storage;
    public bool IsBuilding => isBuilding;
    public int ProducedCount => producedCount;
    public float BuildProgress
    {
        get
        {
            if (!isBuilding)
            {
                return 0f;
            }

            float duration = Mathf.Max(0.01f, buildDuration);
            return Mathf.Clamp01(1f - ((buildCompleteTime - Time.time) / duration));
        }
    }

    public float BuildTimeRemaining => isBuilding ? Mathf.Max(0f, buildCompleteTime - Time.time) : 0f;

    private void Awake()
    {
        storage = GetComponent<ResourceStorage>();
        if (storage == null)
        {
            storage = gameObject.AddComponent<ResourceStorage>();
        }

        storage.Configure(DefaultCapacity);
    }

    private void Update()
    {
        if (isBuilding)
        {
            if (Time.time >= buildCompleteTime)
            {
                CompleteBuild();
            }

            return;
        }

        TryStartBuild();
    }

    public bool CanStartBuild()
    {
        return storage != null &&
            storage.GetAmount(ResourceType.Ore) >= OreCost &&
            storage.GetAmount(ResourceType.Copper) >= CopperCost &&
            storage.GetAmount(ResourceType.Silicate) >= SilicateCost;
    }

    public bool TryStartBuild()
    {
        if (isBuilding || !CanStartBuild())
        {
            return false;
        }

        storage.RemoveResource(ResourceType.Ore, OreCost);
        storage.RemoveResource(ResourceType.Copper, CopperCost);
        storage.RemoveResource(ResourceType.Silicate, SilicateCost);

        isBuilding = true;
        buildCompleteTime = Time.time + Mathf.Max(0.01f, buildDuration);
        return true;
    }

    public DysonSatellite CompleteBuildNow()
    {
        if (!isBuilding && !TryStartBuild())
        {
            return null;
        }

        return CompleteBuild();
    }

    private DysonSatellite CompleteBuild()
    {
        isBuilding = false;
        producedCount++;
        return SpawnSatellite();
    }

    private DysonSatellite SpawnSatellite()
    {
        Vector2 factoryPosition = transform.position;
        Vector2 fromSun = factoryPosition.sqrMagnitude > 0.001f ? factoryPosition.normalized : Vector2.right;
        Vector2 spawnPosition = factoryPosition + (fromSun * Mathf.Max(0f, ejectDistance));
        float orbitRadius = spawnPosition.magnitude;
        float angle = DysonSatellite.AngleFromSun(Vector2.zero, spawnPosition);

        string displayName = ObjectNamer.NumberedManMadeName("Dyson Reflector");
        GameObject satelliteObject = new GameObject(displayName);
        ObjectNamer.AssignIdentity(satelliteObject, displayName, ObjectIdentityCategory.ManMade);
        satelliteObject.transform.position = spawnPosition;
        satelliteObject.transform.localScale = Vector3.one;

        StarSystemGenerator generator = FindFirstObjectByType<StarSystemGenerator>();
        if (generator != null && generator.generatedRoot != null)
        {
            satelliteObject.transform.SetParent(generator.generatedRoot);
        }
        else if (transform.parent != null)
        {
            satelliteObject.transform.SetParent(transform.parent);
        }

        SpriteRenderer renderer = satelliteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = PlaceholderSprites.DysonSatellite;
        renderer.color = new Color(0.72f, 0.95f, 1f);

        DysonSatellite satellite = satelliteObject.AddComponent<DysonSatellite>();
        satellite.mode = DysonSatelliteMode.Dynamic;
        satellite.sunPosition = Vector2.zero;
        satellite.orbitRadius = orbitRadius;
        satellite.startAngleDegrees = angle;
        satellite.orbitSpeedDegrees = orbitSpeedDegrees;

        CircleCollider2D collider = satelliteObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.DysonSatellite);

        MapMarker marker = satelliteObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.DysonSatellite;
        marker.markerColor = renderer.color;
        marker.iconScale = 0.8f;
        marker.requireDiscovery = false;

        return satellite;
    }

    public static IReadOnlyList<ResourceStack> Recipe
    {
        get
        {
            return new[]
            {
                new ResourceStack(ResourceType.Ore, OreCost),
                new ResourceStack(ResourceType.Copper, CopperCost),
                new ResourceStack(ResourceType.Silicate, SilicateCost)
            };
        }
    }
}
