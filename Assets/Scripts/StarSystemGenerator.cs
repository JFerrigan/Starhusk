using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarSystemGenerator : MonoBehaviour
{
    public const int EnemyPlanetCount = 2;
    public const int PiratesPerEnemyPlanet = 2;

    public int seed = 1107;
    public float systemRadius = 80f;
    public float worldScaleMultiplier = 10f;
    public int minPlanets = 10;
    public int maxPlanets = 10;
   public int asteroidFieldCount = 7;
public int asteroidsPerField = 28;
public int starterAsteroidCount = 8;
public float starterAsteroidMinDistance = 32f;
public float starterAsteroidMaxDistance = 130f;    public int stationaryDysonSatelliteCount = 2;
    public int dynamicDysonSatelliteCount = 1;
    public float celestialScaleMultiplier = 10f;
    public Transform generatedRoot;

    public StarSystemLayout CurrentLayout { get; private set; }
    public float EffectiveSystemRadius => systemRadius * worldScaleMultiplier;
    public GameModeRules CurrentRules { get; private set; }

    private bool rulesConfigured;

    private void Start()
    {
        if (!rulesConfigured)
        {
            ApplyGameModeRules(GameModeRuntime.ResolveActiveRules(SceneManager.GetActiveScene().name));
        }

        GenerateWorld();
    }

    public void ApplyGameModeRules(GameModeRules rules, Func<int> randomSeedProvider = null)
    {
        CurrentRules = rules != null ? rules.Clone() : GameModeCatalog.Create(GameModeId.Dev);
        starterAsteroidCount = Mathf.Max(0, CurrentRules.starterAsteroidCount);
        seed = ResolveSeed(CurrentRules, randomSeedProvider);
        rulesConfigured = true;
    }

    public static int ResolveSeed(GameModeRules rules, Func<int> randomSeedProvider = null)
    {
        if (rules == null)
        {
            return GameModeCatalog.DefaultDevSeed;
        }

        if (!rules.useRandomSeed)
        {
            return rules.fixedSeed;
        }

        Func<int> provider = randomSeedProvider ?? GameSeedProvider.NextSeed;
        return provider();
    }

    public void GenerateWorld()
    {
        if (generatedRoot == null)
        {
            GameObject rootObject = GameObject.Find("GeneratedSystemRoot");

            if (rootObject == null)
            {
                rootObject = new GameObject("GeneratedSystemRoot");
            }

            generatedRoot = rootObject.transform;
        }
        
        ClearGeneratedRoot();
        CurrentLayout = GenerateLayout(seed, systemRadius, minPlanets, maxPlanets, asteroidFieldCount, asteroidsPerField, worldScaleMultiplier, stationaryDysonSatelliteCount, dynamicDysonSatelliteCount);
        BuildLayout(CurrentLayout);
        SpawnPlayerNearMainPlanet(CurrentLayout);
        SpawnStarterAsteroidsNearPlayer(CurrentLayout);
    }

    public static StarSystemLayout GenerateLayout(
        int seed,
        float systemRadius,
        int minPlanets,
        int maxPlanets,
        int asteroidFieldCount,
        int asteroidsPerField,
        float worldScaleMultiplier = 1f,
        int stationaryDysonSatelliteCount = 2,
        int dynamicDysonSatelliteCount = 1)
    {
        System.Random random = new System.Random(seed);
        StarSystemLayout layout = new StarSystemLayout
        {
            seed = seed,
            starType = (StarType)random.Next(0, Enum.GetValues(typeof(StarType)).Length),
            starName = ObjectNamer.StarNameForSeed(seed)
        };

        int planetCount = random.Next(minPlanets, maxPlanets + 1);
        float scaledSystemRadius = systemRadius * worldScaleMultiplier;
        float orbitStep = scaledSystemRadius / (planetCount + 2);

       for (int i = 0; i < planetCount; i++)
{
    float orbitRadius = orbitStep * (i + 1.5f);
    float angle = RandomRange(random, 0f, Mathf.PI * 2f);
    CelestialBodyType bodyType = (CelestialBodyType)random.Next(0, 3);
    List<ResourceStack> planetResources = PlanetResourcesForBody(bodyType, random, i == 0);
    ResourceType primaryResource = planetResources.Count > 0 ? planetResources[0].type : ResourceType.Ore;

    int totalResourceAmount = 0;
    for (int resourceIndex = 0; resourceIndex < planetResources.Count; resourceIndex++)
    {
        totalResourceAmount += planetResources[resourceIndex].amount;
    }

    bool hasPirateBase = IsEnemyPlanetIndex(i, planetCount);

    layout.planets.Add(new CelestialBodyDefinition
    {
        name = hasPirateBase ? ObjectNamer.PlanetNameFor(seed, i) + " Raider Base" : ObjectNamer.PlanetNameFor(seed, i),
        bodyType = bodyType,
        position = Direction(angle) * orbitRadius,
        radius = RandomRange(random, 1.8f, 3.5f),
        primaryResource = primaryResource,
        resourceAmount = totalResourceAmount,
        resources = planetResources,
        discoveredAtStart = i == 0,
        hasPirateBase = hasPirateBase
    });
}

        int asteroidCount = Mathf.Max(0, asteroidFieldCount * asteroidsPerField);
        float innerAsteroidRadius = scaledSystemRadius * 0.14f;

        for (int i = 0; i < asteroidCount; i++)
        {
            float t = (i + (float)random.NextDouble()) / Mathf.Max(1, asteroidCount);
            float radius = Mathf.Lerp(innerAsteroidRadius, scaledSystemRadius * 0.94f, Mathf.Sqrt(t));
            float angle = RandomRange(random, 0f, Mathf.PI * 2f);
            Vector2 position = Direction(angle) * radius;
            ResourceType resourceType = RandomAsteroidResource(random);

            layout.asteroids.Add(new CelestialBodyDefinition
            {
                name = "Asteroid " + (i + 1).ToString("000"),
                bodyType = CelestialBodyType.Asteroid,
                position = position,
                radius = RandomRange(random, 0.55f, 1.45f),
                primaryResource = resourceType,
                resourceAmount = random.Next(60, 220),
                discoveredAtStart = i < 4
            });
        }

        GenerateDysonSatellites(layout, random, scaledSystemRadius, stationaryDysonSatelliteCount, dynamicDysonSatelliteCount);

        return layout;
    }

    private void BuildLayout(StarSystemLayout layout)
    {
        ObjectNamer.ResetManMadeCounters();
        BuildSpriteObject(layout.starName, Vector2.zero, 6f * celestialScaleMultiplier, StarColor(layout.starType), true, MapMarkerType.Star, 6f, false);

        for (int i = 0; i < layout.planets.Count; i++)
        {
            CelestialBodyDefinition body = layout.planets[i];
            Color planetColor = body.hasPirateBase
                ? Color.Lerp(PlanetColor(body.bodyType), new Color(1f, 0.1f, 0.08f, 1f), 0.45f)
                : PlanetColor(body.bodyType);
            GameObject planet = BuildSpriteObject(body.name, body.position, body.radius * celestialScaleMultiplier, planetColor, body.discoveredAtStart, MapMarkerType.Planet, body.radius, false);
            planet.AddComponent<PlanetLogisticsNetwork>();
            PlanetGravityWell gravityWell = planet.AddComponent<PlanetGravityWell>();
            gravityWell.surfaceRadius = body.radius * celestialScaleMultiplier * ColliderRadiusForMarker(MapMarkerType.Planet);
            gravityWell.influenceRadius = gravityWell.surfaceRadius + Mathf.Max(28f, body.radius * celestialScaleMultiplier * 1.7f);

            ResourceDeposit deposit = planet.AddComponent<ResourceDeposit>();

if (body.resources != null && body.resources.Count > 0)
{
    deposit.ConfigureResources(body.resources, 25);
}
else
{
    deposit.ConfigureSingleResource(body.primaryResource, body.resourceAmount, 25);
}

deposit.destroyWhenDepleted = false;

            if (body.hasPirateBase)
            {
                SpawnPiratesAroundEnemyPlanet(body);
            }
        }

        for (int i = 0; i < layout.asteroids.Count; i++)
        {
            CelestialBodyDefinition body = layout.asteroids[i];
            BuildAsteroidObject(body.name, body.position, body.radius, body.primaryResource, body.resourceAmount, body.discoveredAtStart);
        }

        BuildDysonSatellites(layout);
    }

    public static bool IsEnemyPlanetIndex(int planetIndex, int planetCount)
    {
        return planetIndex > 0 && planetIndex >= Mathf.Max(1, planetCount - EnemyPlanetCount);
    }

    public static Vector2 CalculatePirateSpawnPosition(CelestialBodyDefinition planet, float celestialScaleMultiplier, int pirateIndex)
    {
        float planetWorldRadius = planet.radius * celestialScaleMultiplier;
        Vector2 baseDirection = planet.position.sqrMagnitude > 0.01f ? planet.position.normalized : Vector2.up;
        Vector2 direction = Quaternion.Euler(0f, 0f, pirateIndex * 140f - 40f) * baseDirection;
        return planet.position + direction.normalized * (planetWorldRadius + 24f);
    }

    private void SpawnPiratesAroundEnemyPlanet(CelestialBodyDefinition planet)
    {
        float leashRadius = Mathf.Max(120f, planet.radius * celestialScaleMultiplier + 95f);
        for (int i = 0; i < PiratesPerEnemyPlanet; i++)
        {
            Vector2 spawnPosition = CalculatePirateSpawnPosition(planet, celestialScaleMultiplier, i);
            PirateShipController.SpawnAnchoredAt(new Vector3(spawnPosition.x, spawnPosition.y, 0f), planet.position, generatedRoot, leashRadius);
        }
    }

    private GameObject BuildSpriteObject(string objectName, Vector2 position, float visualRadius, Color color, bool discoveredAtStart, MapMarkerType markerType, float baseRadius, bool colliderIsTrigger = false)
    {
        GameObject instance = new GameObject(objectName);
        ObjectNamer.AssignIdentity(instance, objectName, ObjectIdentityCategory.Celestial);
        instance.transform.SetParent(generatedRoot);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * visualRadius;

        SpriteRenderer spriteRenderer = instance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteForMarker(markerType);
        spriteRenderer.color = color;

        CircleCollider2D collider = instance.AddComponent<CircleCollider2D>();
        collider.isTrigger = colliderIsTrigger;
        collider.radius = ColliderRadiusForMarker(markerType);

        DiscoveryState discovery = instance.AddComponent<DiscoveryState>();
        discovery.SetDiscovered(discoveredAtStart);
        discovery.passiveRevealRadius = Mathf.Max(4f, visualRadius + 3f);

        MapMarker marker = instance.AddComponent<MapMarker>();
        marker.markerType = markerType;
        marker.markerColor = color;
        marker.iconScale = Mathf.Max(0.75f, baseRadius * 0.45f);
        marker.requireDiscovery = markerType != MapMarkerType.Star;
        marker.discoveryState = discovery;

        return instance;
    }

    private GameObject BuildAsteroidObject(string asteroidName, Vector2 position, float baseRadius, ResourceType resourceType, int resourceAmount, bool discoveredAtStart)
    {
        Color color = ResourceColor(resourceType);
        float visualRadius = baseRadius * celestialScaleMultiplier;

        GameObject asteroid = new GameObject(asteroidName);
        ObjectNamer.AssignIdentity(asteroid, asteroidName, ObjectIdentityCategory.Celestial);
        asteroid.transform.SetParent(generatedRoot);
        asteroid.transform.position = position;
        asteroid.transform.localScale = Vector3.one * visualRadius;

        DiscoveryState discovery = asteroid.AddComponent<DiscoveryState>();
        discovery.SetDiscovered(discoveredAtStart);
        discovery.passiveRevealRadius = Mathf.Max(4f, visualRadius + 3f);

        MapMarker marker = asteroid.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Asteroid;
        marker.markerColor = color;
        marker.iconScale = Mathf.Max(0.75f, baseRadius * 0.45f);
        marker.requireDiscovery = true;
        marker.discoveryState = discovery;

        ResourceDeposit deposit = asteroid.AddComponent<ResourceDeposit>();
        deposit.ConfigureSingleResource(resourceType, resourceAmount, 12);
        deposit.destroyWhenDepleted = false;
        CircularDestructibleAsteroid destructible = asteroid.AddComponent<CircularDestructibleAsteroid>();
        destructible.SetTint(color);
        return asteroid;
    }

    private static Sprite SpriteForMarker(MapMarkerType markerType)
    {
        switch (markerType)
        {
            case MapMarkerType.Asteroid:
                return PlaceholderSprites.Circle;
            case MapMarkerType.DysonSatellite:
                return PlaceholderSprites.DysonSatellite;
            case MapMarkerType.Planet:
                return PlaceholderSprites.Planet;
            case MapMarkerType.Star:
                return PlaceholderSprites.Star;
            default:
                return PlaceholderSprites.Circle;
        }
    }

    private static void GenerateDysonSatellites(
        StarSystemLayout layout,
        System.Random random,
        float scaledSystemRadius,
        int stationaryCount,
        int dynamicCount)
    {
        float stationaryOrbitRadius = scaledSystemRadius * 0.06f;
        float dynamicOrbitRadius = scaledSystemRadius * 0.05f;

        for (int i = 0; i < stationaryCount; i++)
        {
            float angle = ((float)i / Mathf.Max(1, stationaryCount)) * 360f;
            layout.dysonSatellites.Add(new DysonSatelliteDefinition
            {
                name = "Dyson Receiver",
                mode = DysonSatelliteMode.Stationary,
                position = Direction(angle * Mathf.Deg2Rad) * stationaryOrbitRadius,
                orbitRadius = stationaryOrbitRadius,
                startAngleDegrees = angle,
                orbitSpeedDegrees = 0f,
                discoveredAtStart = false
            });
        }

        for (int i = 0; i < dynamicCount; i++)
        {
            float angle = ((float)i / Mathf.Max(1, dynamicCount) * 360f) + RandomRange(random, -12f, 12f);
            float speed = RandomRange(random, 5f, 10f) * (i % 2 == 0 ? 1f : -1f);
            layout.dysonSatellites.Add(new DysonSatelliteDefinition
            {
                name = "Dyson Reflector",
                mode = DysonSatelliteMode.Dynamic,
                position = Direction(angle * Mathf.Deg2Rad) * dynamicOrbitRadius,
                orbitRadius = dynamicOrbitRadius,
                startAngleDegrees = angle,
                orbitSpeedDegrees = speed,
                discoveredAtStart = false
            });
        }
    }

    private void BuildDysonSatellites(StarSystemLayout layout)
    {
        DysonSatellite[] dynamicSatellites = new DysonSatellite[CountSatellites(layout, DysonSatelliteMode.Dynamic)];
        DysonSatellite[] stationarySatellites = new DysonSatellite[CountSatellites(layout, DysonSatelliteMode.Stationary)];
        int dynamicIndex = 0;
        int stationaryIndex = 0;

        for (int i = 0; i < layout.dysonSatellites.Count; i++)
        {
            DysonSatelliteDefinition definition = layout.dysonSatellites[i];
            GameObject satelliteObject = BuildDysonSatelliteObject(definition);
            DysonSatellite satellite = satelliteObject.GetComponent<DysonSatellite>();

            if (definition.mode == DysonSatelliteMode.Dynamic)
            {
                dynamicSatellites[dynamicIndex] = satellite;
                dynamicIndex++;
            }
            else
            {
                stationarySatellites[stationaryIndex] = satellite;
                stationaryIndex++;
            }
        }

        if (dynamicSatellites.Length > 0 && stationarySatellites.Length > 0)
        {
            GameObject networkObject = new GameObject("DysonBeamNetwork");
            networkObject.transform.SetParent(generatedRoot);
            DysonBeamNetwork network = networkObject.AddComponent<DysonBeamNetwork>();
            network.Initialize(dynamicSatellites, stationarySatellites);
        }
    }

    private GameObject BuildDysonSatelliteObject(DysonSatelliteDefinition definition)
    {
        string displayName = ObjectNamer.NumberedManMadeName(definition.name);
        GameObject instance = new GameObject(displayName);
        ObjectNamer.AssignIdentity(instance, displayName, ObjectIdentityCategory.ManMade);
        instance.transform.SetParent(generatedRoot);
        instance.transform.position = definition.position;
        instance.transform.localScale = Vector3.one * 1f;

        SpriteRenderer spriteRenderer = instance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = PlaceholderSprites.DysonSatellite;
        spriteRenderer.color = definition.mode == DysonSatelliteMode.Dynamic
            ? new Color(0.72f, 0.95f, 1f)
            : new Color(1f, 0.86f, 0.34f);

        DysonSatellite satellite = instance.AddComponent<DysonSatellite>();
        satellite.mode = definition.mode;
        satellite.orbitRadius = definition.orbitRadius;
        satellite.startAngleDegrees = definition.startAngleDegrees;
        satellite.orbitSpeedDegrees = definition.orbitSpeedDegrees;

        CircleCollider2D collider = instance.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = ColliderRadiusForMarker(MapMarkerType.DysonSatellite);

        if (definition.mode == DysonSatelliteMode.Stationary)
        {
            instance.AddComponent<ManMadeMovableObject>();
        }

        DiscoveryState discovery = instance.AddComponent<DiscoveryState>();
        discovery.SetDiscovered(definition.discoveredAtStart);
        discovery.passiveRevealRadius = 18f;

        MapMarker marker = instance.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.DysonSatellite;
        marker.markerColor = spriteRenderer.color;
        marker.iconScale = 0.8f;
        marker.requireDiscovery = true;
        marker.discoveryState = discovery;

        return instance;
    }

    private static int CountSatellites(StarSystemLayout layout, DysonSatelliteMode mode)
    {
        int count = 0;
        for (int i = 0; i < layout.dysonSatellites.Count; i++)
        {
            if (layout.dysonSatellites[i].mode == mode)
            {
                count++;
            }
        }

        return count;
    }

    public static float ColliderRadiusForMarker(MapMarkerType markerType)
    {
        switch (markerType)
        {
            case MapMarkerType.Planet:
                return 0.48f;
            case MapMarkerType.Star:
                return 0.47f;
            default:
                return 0.5f;
        }
    }

    private void ClearGeneratedRoot()
    {
        for (int i = generatedRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(generatedRoot.GetChild(i).gameObject);
        }
    }


private static List<ResourceStack> PlanetResourcesForBody(CelestialBodyType bodyType, System.Random random, bool isMainPlanet)
{
    List<ResourceStack> resources = new List<ResourceStack>();

    if (isMainPlanet)
    {
        resources.Add(new ResourceStack(ResourceType.Ore, random.Next(2200, 3600)));
        resources.Add(new ResourceStack(ResourceType.Ice, random.Next(2200, 3600)));
        resources.Add(new ResourceStack(ResourceType.Silicate, random.Next(2200, 3600)));
        resources.Add(new ResourceStack(ResourceType.Copper, random.Next(2200, 3600)));
        resources.Add(new ResourceStack(ResourceType.Biomass, random.Next(2200, 3600)));
        return resources;
    }

    switch (bodyType)
    {
        case CelestialBodyType.IceMoon:
            AddPlanetResource(resources, ResourceType.Ice, random.Next(1200, 2600));

            if (random.Next(0, 100) < 45)
            {
                AddPlanetResource(resources, ResourceType.Silicate, random.Next(700, 1800));
            }

            if (random.Next(0, 100) < 20)
            {
                AddPlanetResource(resources, ResourceType.Ore, random.Next(500, 1400));
            }

            break;

        case CelestialBodyType.MetallicBody:
            AddPlanetResource(resources, ResourceType.Ore, random.Next(1000, 2400));

            if (random.Next(0, 100) < 65)
            {
                AddPlanetResource(resources, ResourceType.Copper, random.Next(800, 2200));
            }

            if (random.Next(0, 100) < 45)
            {
                AddPlanetResource(resources, ResourceType.Silicate, random.Next(700, 1800));
            }

            break;

        case CelestialBodyType.RockyPlanet:
            AddPlanetResource(resources, ResourceType.Ore, random.Next(800, 2200));

            if (random.Next(0, 100) < 70)
            {
                AddPlanetResource(resources, ResourceType.Silicate, random.Next(800, 2200));
            }

            if (random.Next(0, 100) < 35)
            {
                AddPlanetResource(resources, ResourceType.Biomass, random.Next(500, 1600));
            }

            if (random.Next(0, 100) < 25)
            {
                AddPlanetResource(resources, ResourceType.Ice, random.Next(500, 1500));
            }

            break;

        default:
            AddPlanetResource(resources, ResourceForBody(bodyType, random), random.Next(800, 2200));
            break;
    }

    if (resources.Count <= 0)
    {
        AddPlanetResource(resources, ResourceType.Ore, random.Next(800, 2200));
    }

    return resources;
}

private static void AddPlanetResource(List<ResourceStack> resources, ResourceType type, int amount)
{
    if (resources == null || amount <= 0)
    {
        return;
    }

    for (int i = 0; i < resources.Count; i++)
    {
        if (resources[i].type != type)
        {
            continue;
        }

        ResourceStack stack = resources[i];
        stack.amount += amount;
        resources[i] = stack;
        return;
    }

    resources.Add(new ResourceStack(type, amount));
}
    private static ResourceType ResourceForBody(CelestialBodyType bodyType, System.Random random)
    {
        switch (bodyType)
        {
            case CelestialBodyType.IceMoon:
                return ResourceType.Ice;
            case CelestialBodyType.MetallicBody:
                if (random.Next(0, 100) < 20)
                {
                    return ResourceType.Silicate;
                }

                if (random.Next(0, 100) < 40)
                {
                    return ResourceType.Biomass;
                }

                return ResourceType.Copper;
            case CelestialBodyType.RockyPlanet:
                int rockyRoll = random.Next(0, 100);
                if (rockyRoll < 35)
                {
                    return ResourceType.Ore;
                }

                if (rockyRoll < 70)
                {
                    return ResourceType.Silicate;
                }

                return ResourceType.Biomass;
            default:
                return random.Next(0, 100) < 18 ? ResourceType.Biomass : ResourceType.Silicate;
        }
    }

    private static ResourceType RandomAsteroidResource(System.Random random)
    {
        int roll = random.Next(0, 100);

        if (roll < 55)
        {
            return ResourceType.Ore;
        }

        if (roll < 75)
        {
            return ResourceType.Silicate;
        }

        if (roll < 90)
        {
            return ResourceType.Copper;
        }

        return ResourceType.Biomass;
    }

    private static Color StarColor(StarType starType)
    {
        switch (starType)
        {
            case StarType.RedDwarf:
                return new Color(1f, 0.35f, 0.25f);
            case StarType.Blue:
                return new Color(0.45f, 0.7f, 1f);
            case StarType.White:
                return new Color(0.9f, 0.95f, 1f);
            default:
                return new Color(1f, 0.82f, 0.34f);
        }
    }

    private static Color PlanetColor(CelestialBodyType bodyType)
    {
        switch (bodyType)
        {
            case CelestialBodyType.IceMoon:
                return new Color(0.55f, 0.8f, 1f);
            case CelestialBodyType.MetallicBody:
                return new Color(0.75f, 0.72f, 0.65f);
            default:
                return new Color(0.55f, 0.45f, 0.35f);
        }
    }

    private static Color ResourceColor(ResourceType resourceType)
    {
        return ResourceVisuals.ColorFor(resourceType);
    }

    private static Vector2 Direction(float radians)
    {
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static float RandomRange(System.Random random, float min, float max)
    {
        return min + ((float)random.NextDouble() * (max - min));
    }

    private void SpawnStarterAsteroidsNearPlayer(StarSystemLayout layout)
{
    if (starterAsteroidCount <= 0 || layout == null || layout.planets == null || layout.planets.Count == 0)
    {
        return;
    }

    ResourceInventory player = FindFirstObjectByType<ResourceInventory>();
    if (player == null)
    {
        return;
    }

    System.Random random = new System.Random(seed ^ 0x5f3759df);
    Vector2 playerPosition = player.transform.position;
    CelestialBodyDefinition mainPlanet = layout.planets[0];
    float planetWorldRadius = mainPlanet.radius * celestialScaleMultiplier;

    Vector2 awayFromPlanet = (playerPosition - mainPlanet.position).sqrMagnitude > 0.01f
        ? (playerPosition - mainPlanet.position).normalized
        : Vector2.up;

    Vector2 sideways = Vector2.Perpendicular(awayFromPlanet).normalized;

    float minDistance = Mathf.Max(28f, starterAsteroidMinDistance);
    float maxDistance = Mathf.Max(minDistance + 90f, starterAsteroidMaxDistance);

    List<Vector2> placedPositions = new List<Vector2>();
    List<float> placedWorldRadii = new List<float>();

    for (int i = 0; i < starterAsteroidCount; i++)
    {
        float baseRadius = RandomRange(random, 0.55f, 1.05f);
        float worldRadius = baseRadius * celestialScaleMultiplier;
        Vector2 position = playerPosition + awayFromPlanet * RandomRange(random, minDistance, maxDistance);

        bool foundPosition = false;

        for (int attempt = 0; attempt < 80; attempt++)
        {
            float forwardDistance = RandomRange(random, minDistance, maxDistance);
            float sideSpread = Mathf.Lerp(22f, 95f, (float)i / Mathf.Max(1, starterAsteroidCount - 1));
            float sideOffset = RandomRange(random, -sideSpread, sideSpread);

            // Bias starter asteroids into a loose fan in front of the player, away from the starter planet.
            Vector2 candidate = playerPosition + (awayFromPlanet * forwardDistance) + (sideways * sideOffset);

            float minimumPlanetDistance = planetWorldRadius + worldRadius + 12f;
            if (Vector2.Distance(candidate, mainPlanet.position) < minimumPlanetDistance)
            {
                continue;
            }

            if (!IsStarterAsteroidPositionClear(candidate, worldRadius, placedPositions, placedWorldRadii))
            {
                continue;
            }

            position = candidate;
            foundPosition = true;
            break;
        }

        if (!foundPosition)
        {
            float fallbackAngle = ((float)i / Mathf.Max(1, starterAsteroidCount)) * Mathf.PI * 2f;
            float fallbackDistance = minDistance + (i * (worldRadius + 18f));
            position = playerPosition
                + (awayFromPlanet * fallbackDistance)
                + (sideways * Mathf.Sin(fallbackAngle) * 80f);
        }

        placedPositions.Add(position);
        placedWorldRadii.Add(worldRadius);

        ResourceType resourceType = RandomAsteroidResource(random);
        int resourceAmount = random.Next(45, 130);
        string asteroidName = "Starter Asteroid " + (i + 1).ToString("000");
        BuildAsteroidObject(asteroidName, position, baseRadius, resourceType, resourceAmount, true);
    }
}

private static bool IsStarterAsteroidPositionClear(
    Vector2 candidatePosition,
    float candidateWorldRadius,
    List<Vector2> placedPositions,
    List<float> placedWorldRadii)
{
    for (int i = 0; i < placedPositions.Count; i++)
    {
        float requiredDistance = candidateWorldRadius + placedWorldRadii[i] + 16f;
        if (Vector2.Distance(candidatePosition, placedPositions[i]) < requiredDistance)
        {
            return false;
        }
    }

    return true;
}

    public static Vector2 CalculateMainPlanetSpawn(CelestialBodyDefinition mainPlanet, float celestialScaleMultiplier)
    {
        float planetWorldRadius = mainPlanet.radius * celestialScaleMultiplier;
        Vector2 directionAwayFromSun = mainPlanet.position.sqrMagnitude > 0.01f
            ? mainPlanet.position.normalized
            : Vector2.up;

        return mainPlanet.position + directionAwayFromSun * (planetWorldRadius + 8f);
    }

    public void SpawnPlayerNearMainPlanet(StarSystemLayout layout)
{
    if (layout == null || layout.planets == null || layout.planets.Count == 0)
    {
        return;
    }

    ResourceInventory player = FindFirstObjectByType<ResourceInventory>();
    if (player == null)
    {
        return;
    }

    CelestialBodyDefinition mainPlanet = layout.planets[0];
    Vector2 spawnPosition = CalculateMainPlanetSpawn(mainPlanet, celestialScaleMultiplier);

    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    player.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, player.transform.position.z);

    PlayerRespawnController respawnController = player.GetComponent<PlayerRespawnController>();
    if (respawnController != null)
    {
        respawnController.SetHomeSpawn(spawnPosition);
    }
}
}
