using System.Collections.Generic;
using UnityEngine;

public class SatelliteFactory : MonoBehaviour, IPowerConsumer
{
    public const int DefaultCapacity = 5000;
    public const int OreCost = 50;
    public const int SilicateCost = 25;
    public const float DefaultBuildDuration = 10f;
    public const float DefaultOrbitSpeedDegrees = 5f;
    public const int DefaultPowerDemand = 30;

    public float buildDuration = DefaultBuildDuration;
    public float orbitSpeedDegrees = DefaultOrbitSpeedDegrees;
    public float ejectDistance = 3f;

    [SerializeField]
    private ResourceStorage storage;

    [SerializeField]
    private bool isBuilding;

    [SerializeField]
    private int producedCount;

    [SerializeField]
    private int orePool;

    [SerializeField]
    private int silicatePool;

    [SerializeField]
    private bool isPowered = true;

    private float buildProgressSeconds;
    private SpriteRenderer spriteRenderer;
    private Color baseVisualColor = Color.white;
    private bool hasBaseVisualColor;

    public ResourceStorage Storage => storage;
    public bool IsBuilding => isBuilding;
    public int ProducedCount => producedCount;
    public int OrePool => orePool;
    public int SilicatePool => silicatePool;
    public int PowerDemand => DefaultPowerDemand;
    public bool IsPowered => isPowered;
    public float BuildProgress
    {
        get
        {
            if (!isBuilding)
            {
                return 0f;
            }

            float duration = Mathf.Max(0.01f, buildDuration);
            return Mathf.Clamp01(buildProgressSeconds / duration);
        }
    }

    public float BuildTimeRemaining => isBuilding ? Mathf.Max(0f, Mathf.Max(0.01f, buildDuration) - buildProgressSeconds) : 0f;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseVisualColor = spriteRenderer.color;
            hasBaseVisualColor = true;
        }

        storage = GetComponent<ResourceStorage>();
        if (storage == null)
        {
            storage = gameObject.AddComponent<ResourceStorage>();
        }

        storage.Configure(DefaultCapacity);
        ApplyPowerVisual();
    }

    private void Update()
    {
        if (!isPowered)
        {
            return;
        }

        if (isBuilding)
        {
            buildProgressSeconds += Time.deltaTime;
            if (buildProgressSeconds >= Mathf.Max(0.01f, buildDuration))
            {
                CompleteBuild();
            }

            return;
        }

        TryStartBuild();
    }

    public bool CanStartBuild()
    {
        IntakeDeliveredResources();

        return isPowered &&
            orePool >= OreCost &&
            silicatePool >= SilicateCost;
    }

    public bool TryStartBuild()
    {
        if (isBuilding || !CanStartBuild())
        {
            return false;
        }

        orePool -= OreCost;
        silicatePool -= SilicateCost;

        isBuilding = true;
        buildProgressSeconds = 0f;
        return true;
    }

    public void SetPowered(bool powered)
    {
        if (isPowered == powered)
        {
            return;
        }

        isPowered = powered;
        ApplyPowerVisual();
    }

    public int PoolAmountFor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Silicate:
                return silicatePool;
            case ResourceType.Ore:
                return orePool;
            default:
                return 0;
        }
    }

    public int PoolCapacityFor(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Silicate:
                return SilicateCost;
            case ResourceType.Ore:
                return OreCost;
            default:
                return 0;
        }
    }

    private void IntakeDeliveredResources()
    {
        if (storage == null)
        {
            return;
        }

        FillPool(ResourceType.Ore, OreCost, ref orePool);
        FillPool(ResourceType.Silicate, SilicateCost, ref silicatePool);
    }

    private void FillPool(ResourceType type, int capacity, ref int pool)
    {
        int needed = Mathf.Max(0, capacity - pool);
        if (needed <= 0)
        {
            return;
        }

        pool += storage.RemoveResource(type, needed);
    }

    public DysonSatellite CompleteBuildNow()
    {
        if (!isPowered)
        {
            return null;
        }

        if (!isBuilding && !TryStartBuild())
        {
            return null;
        }

        return CompleteBuild();
    }

    private DysonSatellite CompleteBuild()
    {
        isBuilding = false;
        buildProgressSeconds = 0f;
        producedCount++;
        return SpawnSatellite();
    }

    private void ApplyPowerVisual()
    {
        if (spriteRenderer == null || !hasBaseVisualColor)
        {
            return;
        }

        spriteRenderer.color = isPowered
            ? baseVisualColor
            : new Color(baseVisualColor.r * 0.36f, baseVisualColor.g * 0.36f, baseVisualColor.b * 0.36f, baseVisualColor.a);
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
                new ResourceStack(ResourceType.Silicate, SilicateCost)
            };
        }
    }
}
