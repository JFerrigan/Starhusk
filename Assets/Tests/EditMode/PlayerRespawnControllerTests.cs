#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PlayerRespawnControllerTests
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
    public void LethalDamageRespawnsAtHomeWithFullHealthAndNoVelocity()
    {
        PlayerRespawnController respawn = CreatePlayer(out ShipHealth health, out Rigidbody2D rb, out ResourceInventory inventory);
        respawn.SetHomeSpawn(new Vector2(24f, -18f));
        rb.linearVelocity = new Vector2(12f, -7f);
        rb.angularVelocity = 45f;
        health.currentHealth = 10f;

        health.ApplyDamage(99f, ShipFaction.Pirate);

        Assert.That(health.currentHealth, Is.EqualTo(health.maxHealth).Within(0.001f));
        Assert.That(Vector2.Distance(respawn.transform.position, new Vector2(24f, -18f)), Is.LessThan(0.001f));
        Assert.That(rb.linearVelocity, Is.EqualTo(Vector2.zero));
        Assert.That(rb.angularVelocity, Is.EqualTo(0f).Within(0.001f));
        Assert.IsNotNull(inventory);
    }

    [Test]
    public void RespawnClearsAllInventoryResourcesAndLegacyOre()
    {
        CreatePlayer(out ShipHealth health, out _, out ResourceInventory inventory);
        inventory.AddResource(ResourceType.Ore, 7);
        inventory.AddResource(ResourceType.Copper, 3);
        health.currentHealth = 5f;

        health.ApplyDamage(10f, ShipFaction.Pirate);

        Assert.That(inventory.GetResources().Count, Is.EqualTo(0));
        Assert.That(inventory.ore, Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Ore), Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Copper), Is.EqualTo(0));
    }

    [Test]
    public void RespawnFallsBackToDefaultHomeWhenNoHomeSpawnIsSet()
    {
        PlayerRespawnController respawn = CreatePlayer(out ShipHealth health, out _, out _);
        respawn.transform.position = new Vector3(20f, 20f, -3f);
        health.currentHealth = 5f;

        health.ApplyDamage(10f, ShipFaction.Pirate);

        Assert.That(respawn.transform.position.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(respawn.transform.position.y, Is.EqualTo(-10f).Within(0.001f));
        Assert.That(respawn.transform.position.z, Is.EqualTo(-3f).Within(0.001f));
    }

    [Test]
    public void DamageWhileAlreadyDeadDoesNotRunRespawnAgain()
    {
        PlayerRespawnController respawn = CreatePlayer(out ShipHealth health, out _, out ResourceInventory inventory);
        respawn.SetHomeSpawn(new Vector2(5f, 6f));
        health.currentHealth = 1f;

        health.ApplyDamage(2f, ShipFaction.Pirate);
        respawn.transform.position = new Vector3(100f, 100f, 0f);
        inventory.AddResource(ResourceType.Ore, 4);
        health.currentHealth = 0f;

        bool damaged = health.ApplyDamage(2f, ShipFaction.Pirate);

        Assert.IsFalse(damaged);
        Assert.That(Vector2.Distance(respawn.transform.position, new Vector2(100f, 100f)), Is.LessThan(0.001f));
        Assert.That(inventory.GetAmount(ResourceType.Ore), Is.EqualTo(4));
    }

    private static PlayerRespawnController CreatePlayer(out ShipHealth health, out Rigidbody2D rb, out ResourceInventory inventory)
    {
        GameObject player = new GameObject("Respawn Test Player");
        rb = player.AddComponent<Rigidbody2D>();
        inventory = player.AddComponent<ResourceInventory>();
        health = player.AddComponent<ShipHealth>();
        health.faction = ShipFaction.Player;
        health.maxHealth = 100f;
        health.currentHealth = 100f;
        health.destroyOnDeath = false;
        return player.AddComponent<PlayerRespawnController>();
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
