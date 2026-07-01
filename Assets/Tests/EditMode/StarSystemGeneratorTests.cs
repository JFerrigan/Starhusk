using NUnit.Framework;
using UnityEngine;

public class StarSystemGeneratorTests
{
    [Test]
    public void SameSeedProducesSameLayout()
    {
        StarSystemLayout first = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);
        StarSystemLayout second = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);

        Assert.AreEqual(first.starType, second.starType);
        Assert.AreEqual(first.planets.Count, second.planets.Count);
        Assert.AreEqual(first.asteroids.Count, second.asteroids.Count);

        for (int i = 0; i < first.planets.Count; i++)
        {
            Assert.That(Vector2.Distance(first.planets[i].position, second.planets[i].position), Is.LessThan(0.001f));
            Assert.AreEqual(first.planets[i].primaryResource, second.planets[i].primaryResource);
        }

        for (int i = 0; i < first.asteroids.Count; i++)
        {
            Assert.That(Vector2.Distance(first.asteroids[i].position, second.asteroids[i].position), Is.LessThan(0.001f));
            Assert.AreEqual(first.asteroids[i].resourceAmount, second.asteroids[i].resourceAmount);
        }
    }

    [Test]
    public void DifferentSeedsProduceDifferentLayouts()
    {
        StarSystemLayout first = StarSystemGenerator.GenerateLayout(42, 80f, 3, 5, 3, 8);
        StarSystemLayout second = StarSystemGenerator.GenerateLayout(43, 80f, 3, 5, 3, 8);

        Assert.AreNotEqual(first.planets[0].position, second.planets[0].position);
    }

    [Test]
    public void GeneratedBodiesStayInsideSystemBoundsAndHaveFiniteResources()
    {
        const float systemRadius = 80f;
        StarSystemLayout layout = StarSystemGenerator.GenerateLayout(1107, systemRadius, 3, 5, 3, 16);

        Assert.That(layout.planets.Count, Is.InRange(3, 5));
        Assert.That(layout.asteroids.Count, Is.EqualTo(48));

        foreach (CelestialBodyDefinition planet in layout.planets)
        {
            Assert.That(planet.position.magnitude, Is.LessThanOrEqualTo(systemRadius));
            Assert.That(planet.resourceAmount, Is.GreaterThan(0));
        }

        foreach (CelestialBodyDefinition asteroid in layout.asteroids)
        {
            Assert.That(asteroid.position.magnitude, Is.LessThanOrEqualTo(systemRadius));
            Assert.That(asteroid.resourceAmount, Is.GreaterThan(0));
        }
    }

    [Test]
    public void AsteroidsAreDistributedAcrossSystemRadius()
    {
        const float systemRadius = 80f;
        StarSystemLayout layout = StarSystemGenerator.GenerateLayout(1107, systemRadius, 3, 5, 3, 16);

        bool hasInnerAsteroid = false;
        bool hasOuterAsteroid = false;

        foreach (CelestialBodyDefinition asteroid in layout.asteroids)
        {
            if (asteroid.position.magnitude < systemRadius * 0.45f)
            {
                hasInnerAsteroid = true;
            }

            if (asteroid.position.magnitude > systemRadius * 0.75f)
            {
                hasOuterAsteroid = true;
            }
        }

        Assert.IsTrue(hasInnerAsteroid);
        Assert.IsTrue(hasOuterAsteroid);
    }

    [Test]
    public void ColliderRadiiMatchGeneratedSpriteEdges()
    {
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Planet), Is.EqualTo(0.48f).Within(0.001f));
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Star), Is.EqualTo(0.47f).Within(0.001f));
        Assert.That(StarSystemGenerator.ColliderRadiusForMarker(MapMarkerType.Asteroid), Is.EqualTo(0.5f).Within(0.001f));
    }
}
