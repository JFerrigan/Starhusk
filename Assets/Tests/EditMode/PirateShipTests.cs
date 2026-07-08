#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PirateShipTests
{
    [SetUp]
    public void SetUp()
    {
        Cleanup();
    }

    [TearDown]
    public void TearDown()
    {
        Cleanup();
    }

    [Test]
    public void PlayerProjectileDamagesPirateShip()
    {
        GameObject pirate = new GameObject("Projectile Damage Pirate");
        ShipHealth health = pirate.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Pirate;
        health.maxHealth = 50f;
        health.currentHealth = 50f;
        health.destroyOnDeath = false;
        BoxCollider2D collider = pirate.AddComponent<BoxCollider2D>();

        GameObject projectileObject = new GameObject("Test Player Projectile");
        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.damage = 20f;
        projectile.faction = ShipFaction.Player;

        projectileObject.SendMessage("OnTriggerEnter2D", collider);

        Assert.That(health.currentHealth, Is.EqualTo(30f).Within(0.001f));
    }

    [Test]
    public void PirateProjectileDamagesPlayerShip()
    {
        GameObject player = new GameObject("Projectile Damage Player");
        ShipHealth health = player.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Player;
        health.maxHealth = 100f;
        health.currentHealth = 100f;
        health.destroyOnDeath = false;
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();

        GameObject projectileObject = new GameObject("Test Pirate Projectile");
        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.damage = 5f;
        projectile.faction = ShipFaction.Pirate;

        projectileObject.SendMessage("OnTriggerEnter2D", collider);

        Assert.That(health.currentHealth, Is.EqualTo(95f).Within(0.001f));
    }

    [Test]
    public void PirateControllerUsesLowProjectileDamage()
    {
        GameObject pirate = new GameObject("Damage Tuned Pirate");
        PirateShipController controller = pirate.AddComponent<PirateShipController>();

        Assert.That(controller.projectileDamage, Is.EqualTo(5f).Within(0.001f));
    }

    [Test]
    public void SameFactionProjectileDoesNotDamageShip()
    {
        GameObject pirate = new GameObject("Friendly Fire Pirate");
        ShipHealth health = pirate.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Pirate;
        health.maxHealth = 50f;
        health.currentHealth = 50f;
        health.destroyOnDeath = false;
        BoxCollider2D collider = pirate.AddComponent<BoxCollider2D>();

        GameObject projectileObject = new GameObject("Test Pirate Projectile");
        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.damage = 20f;
        projectile.faction = ShipFaction.Pirate;

        projectileObject.SendMessage("OnTriggerEnter2D", collider);

        Assert.That(health.currentHealth, Is.EqualTo(50f).Within(0.001f));
    }

    [Test]
    public void ProjectileIgnoresOwnerShip()
    {
        GameObject player = new GameObject("Projectile Owner");
        ShipHealth health = player.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Player;
        health.maxHealth = 100f;
        health.currentHealth = 100f;
        health.destroyOnDeath = false;
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();

        GameObject projectileObject = new GameObject("Owner Projectile");
        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.damage = 20f;
        projectile.Launch(Vector2.up, Vector2.zero, player.transform, ShipFaction.Player);

        projectileObject.SendMessage("OnTriggerEnter2D", collider);

        Assert.That(health.currentHealth, Is.EqualTo(100f).Within(0.001f));
    }

    [Test]
    public void PirateDropsResourcesOnDeath()
    {
        GameObject pirate = new GameObject("Loot Pirate");
        PirateShipController controller = pirate.AddComponent<PirateShipController>();
        controller.oreDropAmount = 12;
        controller.copperDropAmount = 4;

        ShipHealth health = pirate.GetComponent<ShipHealth>();
        health.ApplyDamage(999f, ShipFaction.Player);

        Assert.That(TotalPickupAmount(ResourceType.Ore), Is.EqualTo(12));
        Assert.That(TotalPickupAmount(ResourceType.Copper), Is.EqualTo(4));
    }

    [Test]
    public void SpawnNearCreatesPirateWithFoundation()
    {
        GameObject player = new GameObject("Spawn Origin");
        player.transform.position = new Vector3(5f, 10f, 0f);

        PirateShipController pirate = PirateShipController.SpawnNear(player.transform);

        Assert.IsNotNull(pirate);
        Assert.IsNotNull(pirate.GetComponent<Rigidbody2D>());
        Assert.IsNotNull(pirate.GetComponent<BoxCollider2D>());
        Assert.AreEqual(ShipFaction.Pirate, pirate.GetComponent<ShipHealth>().faction);
        Assert.IsNotNull(pirate.GetComponent<ShipCrashDamage>());
        Assert.AreEqual(MapMarkerType.Pirate, pirate.GetComponent<MapMarker>().markerType);
    }

    [Test]
    public void SpawnAnchoredAtConfiguresHomeLeash()
    {
        GameObject parent = new GameObject("Pirate Parent");
        Vector2 home = new Vector2(25f, -12f);

        PirateShipController pirate = PirateShipController.SpawnAnchoredAt(new Vector3(40f, -12f, 0f), home, parent.transform, 80f);

        Assert.IsTrue(pirate.HasHomeAnchor);
        Assert.That(Vector2.Distance(pirate.HomeAnchor, home), Is.LessThan(0.001f));
        Assert.That(pirate.leashRadius, Is.EqualTo(80f).Within(0.001f));
        Assert.AreEqual(parent.transform, pirate.transform.parent);
    }

    [Test]
    public void LeashRejectsPositionsTooFarFromHome()
    {
        Vector2 home = new Vector2(10f, 5f);

        Assert.IsTrue(PirateShipController.IsInsideLeash(new Vector2(30f, 5f), home, 25f));
        Assert.IsFalse(PirateShipController.IsInsideLeash(new Vector2(40.1f, 5f), home, 25f));
    }

    [Test]
    public void OpposingProjectilesDestroyEachOther()
    {
        GameObject playerShotObject = new GameObject("Player Shot");
        PlayerProjectile playerShot = playerShotObject.AddComponent<PlayerProjectile>();
        playerShot.faction = ShipFaction.Player;

        GameObject pirateShotObject = new GameObject("Pirate Shot");
        PlayerProjectile pirateShot = pirateShotObject.AddComponent<PlayerProjectile>();
        pirateShot.faction = ShipFaction.Pirate;
        CircleCollider2D pirateCollider = pirateShotObject.GetComponent<CircleCollider2D>();

        playerShotObject.SendMessage("OnTriggerEnter2D", pirateCollider);

        Assert.IsTrue(playerShotObject == null);
        Assert.IsTrue(pirateShotObject == null);
    }

    [Test]
    public void CrashDamageReturnsZeroBelowThreshold()
    {
        float damage = ShipCrashDamage.CalculateDamage(7.5f, 8f, 4f, 65f);

        Assert.That(damage, Is.EqualTo(0f).Within(0.001f));
    }

    [Test]
    public void CrashDamageScalesAboveThreshold()
    {
        float damage = ShipCrashDamage.CalculateDamage(12f, 8f, 4f, 65f);

        Assert.That(damage, Is.EqualTo(16f).Within(0.001f));
    }

    [Test]
    public void CrashDamageClampsToMaximum()
    {
        float damage = ShipCrashDamage.CalculateDamage(40f, 8f, 4f, 65f);

        Assert.That(damage, Is.EqualTo(65f).Within(0.001f));
    }

    [Test]
    public void ShipCrashWithAsteroidAppliesDamageAboveThreshold()
    {
        ShipHealth health = CreateCrashTestShip(out ShipCrashDamage crashDamage);
        GameObject asteroid = CreateAsteroidCollider("Crash Asteroid");

        bool damaged = crashDamage.TryApplyCrashDamage(asteroid.GetComponent<Collider2D>(), 12f, 1f);

        Assert.IsTrue(damaged);
        Assert.That(health.currentHealth, Is.EqualTo(84f).Within(0.001f));
    }

    [Test]
    public void ShipCrashWithPlanetMarkerAppliesDamageAboveThreshold()
    {
        ShipHealth health = CreateCrashTestShip(out ShipCrashDamage crashDamage);
        GameObject planet = CreateMarkerCollider("Crash Planet", MapMarkerType.Planet);

        bool damaged = crashDamage.TryApplyCrashDamage(planet.GetComponent<Collider2D>(), 12f, 1f);

        Assert.IsTrue(damaged);
        Assert.That(health.currentHealth, Is.EqualTo(84f).Within(0.001f));
    }

    [Test]
    public void ShipCrashWithStarMarkerAppliesDamageAboveThreshold()
    {
        ShipHealth health = CreateCrashTestShip(out ShipCrashDamage crashDamage);
        GameObject star = CreateMarkerCollider("Crash Star", MapMarkerType.Star);

        bool damaged = crashDamage.TryApplyCrashDamage(star.GetComponent<Collider2D>(), 12f, 1f);

        Assert.IsTrue(damaged);
        Assert.That(health.currentHealth, Is.EqualTo(84f).Within(0.001f));
    }

    [Test]
    public void LowSpeedShipCrashAppliesNoDamage()
    {
        ShipHealth health = CreateCrashTestShip(out ShipCrashDamage crashDamage);
        GameObject asteroid = CreateAsteroidCollider("Slow Asteroid");

        bool damaged = crashDamage.TryApplyCrashDamage(asteroid.GetComponent<Collider2D>(), 7f, 1f);

        Assert.IsFalse(damaged);
        Assert.That(health.currentHealth, Is.EqualTo(100f).Within(0.001f));
    }

    [Test]
    public void CrashCooldownPreventsRepeatedDamage()
    {
        ShipHealth health = CreateCrashTestShip(out ShipCrashDamage crashDamage);
        GameObject asteroid = CreateAsteroidCollider("Cooldown Asteroid");
        Collider2D asteroidCollider = asteroid.GetComponent<Collider2D>();

        bool firstDamaged = crashDamage.TryApplyCrashDamage(asteroidCollider, 12f, 1f);
        bool secondDamaged = crashDamage.TryApplyCrashDamage(asteroidCollider, 12f, 1.1f);

        Assert.IsTrue(firstDamaged);
        Assert.IsFalse(secondDamaged);
        Assert.That(health.currentHealth, Is.EqualTo(84f).Within(0.001f));
    }

    private static ShipHealth CreateCrashTestShip(out ShipCrashDamage crashDamage)
    {
        GameObject ship = new GameObject("Crash Test Ship");
        ShipHealth health = ship.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Player;
        health.maxHealth = 100f;
        health.currentHealth = 100f;
        health.destroyOnDeath = false;
        crashDamage = ship.AddComponent<ShipCrashDamage>();
        return health;
    }

    private static GameObject CreateAsteroidCollider(string name)
    {
        GameObject asteroid = new GameObject(name);
        asteroid.AddComponent<CircleCollider2D>();
        asteroid.AddComponent<CircularDestructibleAsteroid>();
        return asteroid;
    }

    private static GameObject CreateMarkerCollider(string name, MapMarkerType markerType)
    {
        GameObject markerObject = new GameObject(name);
        markerObject.AddComponent<CircleCollider2D>();
        MapMarker marker = markerObject.AddComponent<MapMarker>();
        marker.markerType = markerType;
        return markerObject;
    }

    private static int TotalPickupAmount(ResourceType type)
    {
        int total = 0;
        ResourcePickup[] pickups = Object.FindObjectsByType<ResourcePickup>(FindObjectsSortMode.None);

        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i] != null && pickups[i].resourceType == type)
            {
                total += pickups[i].amount;
            }
        }

        return total;
    }

    private static void Cleanup()
    {
        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                Object.DestroyImmediate(objects[i]);
            }
        }
    }
}
#endif
