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

public class StarSystemLayout
{
    public int seed;
    public StarType starType;
    public List<CelestialBodyDefinition> planets = new List<CelestialBodyDefinition>();
    public List<CelestialBodyDefinition> asteroids = new List<CelestialBodyDefinition>();
}
