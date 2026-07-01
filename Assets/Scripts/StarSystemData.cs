using System.Collections.Generic;
using UnityEngine;

public enum StarType
{
    Yellow,
    RedDwarf,
    Blue,
    White
}

public enum CelestialBodyType
{
    RockyPlanet,
    IceMoon,
    MetallicBody,
    Asteroid
}

public enum DysonSatelliteMode
{
    Stationary,
    Dynamic
}

public struct CelestialBodyDefinition
{
    public string name;
    public CelestialBodyType bodyType;
    public Vector2 position;
    public float radius;
    public ResourceType primaryResource;
    public int resourceAmount;
    public bool discoveredAtStart;
}

public struct DysonSatelliteDefinition
{
    public string name;
    public DysonSatelliteMode mode;
    public Vector2 position;
    public float orbitRadius;
    public float startAngleDegrees;
    public float orbitSpeedDegrees;
    public bool discoveredAtStart;
}

public class StarSystemLayout
{
    public int seed;
    public StarType starType;
    public List<CelestialBodyDefinition> planets = new List<CelestialBodyDefinition>();
    public List<CelestialBodyDefinition> asteroids = new List<CelestialBodyDefinition>();
    public List<DysonSatelliteDefinition> dysonSatellites = new List<DysonSatelliteDefinition>();
}
