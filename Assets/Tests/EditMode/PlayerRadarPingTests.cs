#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PlayerRadarPingTests
{
    [Test]
    public void PulseProgressClampsToDuration()
    {
        Assert.That(PlayerRadarPing.CalculatePulseProgress(2f, 2f, 0.5f), Is.EqualTo(0f).Within(0.001f));
        Assert.That(PlayerRadarPing.CalculatePulseProgress(2.25f, 2f, 0.5f), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(PlayerRadarPing.CalculatePulseProgress(3f, 2f, 0.5f), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void CooldownReadinessUsesLastPingTime()
    {
        Assert.IsFalse(PlayerRadarPing.IsReadyAtTime(5f, 2f, 4f));
        Assert.IsTrue(PlayerRadarPing.IsReadyAtTime(6f, 2f, 4f));
    }

    [Test]
    public void ContactsExpireAfterContactWindow()
    {
        Assert.IsTrue(PlayerRadarPing.IsContactActive(7f, 7f));
        Assert.IsFalse(PlayerRadarPing.IsContactActive(7.01f, 7f));
    }

    [Test]
    public void RadarDefaultsKeepGuidanceAndCooldownForFifteenSeconds()
    {
        PlayerRadarPing radar = new GameObject("Radar").AddComponent<PlayerRadarPing>();

        try
        {
            Assert.That(radar.cooldownSeconds, Is.EqualTo(15f).Within(0.001f));
            Assert.That(radar.contactDuration, Is.EqualTo(15f).Within(0.001f));
            Assert.That(radar.pulseDuration, Is.LessThan(1f));
        }
        finally
        {
            Object.DestroyImmediate(radar.gameObject);
        }
    }

    [Test]
    public void EdgePointerPositionUsesNearestScreenEdge()
    {
        Vector2 right = FoundationHud.EdgePointerPosition(Vector2.right, 200f, 100f, 10f);
        Vector2 up = FoundationHud.EdgePointerPosition(Vector2.up, 200f, 100f, 10f);

        Assert.That(right.x, Is.EqualTo(190f).Within(0.001f));
        Assert.That(right.y, Is.EqualTo(50f).Within(0.001f));
        Assert.That(up.x, Is.EqualTo(100f).Within(0.001f));
        Assert.That(up.y, Is.EqualTo(90f).Within(0.001f));
    }

    [Test]
    public void RadarDefaultsToNearestAsteroidContact()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            CreateAsteroid("Near", ResourceType.Ore, new Vector2(10f, 0f));
            CreateAsteroid("Far", ResourceType.Copper, new Vector2(30f, 0f));

            scene.Radar.Ping();

            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(1));
            Assert.That(scene.Radar.ActiveContacts[0].marker.name, Is.EqualTo("Near"));
            Assert.IsTrue(scene.Radar.TryGetPointer(MapMarkerType.Asteroid, out Vector2 pointer));
            Assert.That(pointer.x, Is.EqualTo(10f).Within(0.001f));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByName("Near", "Far");
        }
    }

    [Test]
    public void RadarSkipsAsteroidsHiddenFromMapAndRadar()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            MapMarker hidden = CreateAsteroid("Hidden Rock", ResourceType.Ore, new Vector2(10f, 0f)).GetComponent<MapMarker>();
            hidden.hiddenFromMapAndRadar = true;
            CreateAsteroid("Visible Rock", ResourceType.Copper, new Vector2(20f, 0f));

            scene.Radar.Ping();

            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(1));
            Assert.That(scene.Radar.ActiveContacts[0].marker.name, Is.EqualTo("Visible Rock"));
            Assert.IsTrue(scene.Radar.TryGetPointer(MapMarkerType.Asteroid, out Vector2 pointer));
            Assert.That(pointer.x, Is.EqualTo(20f).Within(0.001f));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByName("Hidden Rock", "Visible Rock");
        }
    }

    [Test]
    public void HiddenAsteroidStopsExistingRadarPointers()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            MapMarker marker = CreateAsteroid("Fading Rock", ResourceType.Ore, new Vector2(10f, 0f)).GetComponent<MapMarker>();

            scene.Radar.Ping();
            marker.hiddenFromMapAndRadar = true;

            Assert.IsFalse(scene.Radar.TryGetPointer(MapMarkerType.Asteroid, out _));
            Assert.That(scene.Radar.GetPointers(MapMarkerType.Asteroid).Count, Is.EqualTo(0));

            scene.Radar.PruneExpiredContacts(Time.time);
            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(0));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByName("Fading Rock");
        }
    }

    [Test]
    public void Ping3UpgradeTracksThreeNearestAsteroids()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            scene.Upgrades.Unlock(UpgradeId.Ping3Asteroids);
            for (int i = 0; i < 5; i++)
            {
                CreateAsteroid("Asteroid " + i, ResourceType.Ore, new Vector2(10f + i, 0f));
            }

            scene.Radar.Ping();

            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(3));
            Assert.That(scene.Radar.GetPointers(MapMarkerType.Asteroid).Count, Is.EqualTo(3));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByPrefix("Asteroid ");
        }
    }

    [Test]
    public void Ping10UpgradeOverridesPing3AsteroidLimit()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            scene.Upgrades.Unlock(UpgradeId.Ping3Asteroids);
            scene.Upgrades.Unlock(UpgradeId.Ping10Asteroids);
            for (int i = 0; i < 12; i++)
            {
                CreateAsteroid("Big Ping " + i, ResourceType.Ore, new Vector2(10f + i, 0f));
            }

            scene.Radar.Ping();

            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(10));
            Assert.That(scene.Radar.GetPointers(MapMarkerType.Asteroid).Count, Is.EqualTo(10));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByPrefix("Big Ping ");
        }
    }

    [Test]
    public void ResourceRevealUpgradeAddsAsteroidResourceTypeToContacts()
    {
        RadarScene scene = CreateRadarScene();

        try
        {
            scene.Upgrades.Unlock(UpgradeId.PingAsteroidResourceType);
            CreateAsteroid("Copper Rock", ResourceType.Copper, new Vector2(10f, 0f));

            scene.Radar.Ping();

            Assert.That(scene.Radar.ActiveContacts.Count, Is.EqualTo(1));
            Assert.That(scene.Radar.ActiveContacts[0].resourceType, Is.EqualTo(ResourceType.Copper));
        }
        finally
        {
            scene.Destroy();
            DestroyObjectsByName("Copper Rock");
        }
    }

    private static RadarScene CreateRadarScene()
    {
        GameObject player = new GameObject("Radar Player");
        player.AddComponent<ResourceInventory>();
        PlayerUpgradeState upgrades = player.AddComponent<PlayerUpgradeState>();
        PlayerRadarPing radar = player.AddComponent<PlayerRadarPing>();
        radar.pingRange = 100f;
        return new RadarScene(player, radar, upgrades);
    }

    private static GameObject CreateAsteroid(string name, ResourceType type, Vector2 position)
    {
        GameObject asteroid = new GameObject(name);
        asteroid.transform.position = position;
        ResourceDeposit deposit = asteroid.AddComponent<ResourceDeposit>();
        deposit.ConfigureSingleResource(type, 100);
        MapMarker marker = asteroid.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Asteroid;
        marker.requireDiscovery = false;
        return asteroid;
    }

    private static void DestroyObjectsByName(params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            GameObject target = GameObject.Find(names[i]);
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
        }
    }

    private static void DestroyObjectsByPrefix(string prefix)
    {
        GameObject[] objects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && objects[i].name.StartsWith(prefix))
            {
                Object.DestroyImmediate(objects[i]);
            }
        }
    }

    private struct RadarScene
    {
        public GameObject Player;
        public PlayerRadarPing Radar;
        public PlayerUpgradeState Upgrades;

        public RadarScene(GameObject player, PlayerRadarPing radar, PlayerUpgradeState upgrades)
        {
            Player = player;
            Radar = radar;
            Upgrades = upgrades;
        }

        public void Destroy()
        {
            Object.DestroyImmediate(Player);
        }
    }
}
#endif
