#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class GameModeBootstrapTests
{
    [SetUp]
    public void SetUp()
    {
        CleanupSceneObjects();
        GameModeRuntime.ClearSelectionForTests();
    }

    [TearDown]
    public void TearDown()
    {
        CleanupSceneObjects();
        GameModeRuntime.ClearSelectionForTests();
    }

    [Test]
    public void DevModeGrantsStartingResources()
    {
        ResourceInventory inventory = new GameObject("Player").AddComponent<ResourceInventory>();

        GameBootstrap.ApplyStartingResources(inventory, GameModeCatalog.Create(GameModeId.Dev));

        Assert.That(inventory.GetAmount(ResourceType.Ore), Is.EqualTo(GameModeCatalog.DefaultStartingResourceAmount));
        Assert.That(inventory.GetAmount(ResourceType.Ice), Is.EqualTo(GameModeCatalog.DefaultStartingResourceAmount));
        Assert.That(inventory.GetAmount(ResourceType.Silicate), Is.EqualTo(GameModeCatalog.DefaultStartingResourceAmount));
        Assert.That(inventory.GetAmount(ResourceType.Copper), Is.EqualTo(GameModeCatalog.DefaultStartingResourceAmount));
        Assert.That(inventory.GetAmount(ResourceType.Biomass), Is.EqualTo(GameModeCatalog.DefaultStartingResourceAmount));
    }

    [Test]
    public void RealModeStartsAtZeroResources()
    {
        ResourceInventory inventory = new GameObject("Player").AddComponent<ResourceInventory>();

        GameBootstrap.ApplyStartingResources(inventory, GameModeCatalog.Create(GameModeId.Real));

        Assert.That(inventory.GetAmount(ResourceType.Ore), Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Ice), Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Silicate), Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Copper), Is.EqualTo(0));
        Assert.That(inventory.GetAmount(ResourceType.Biomass), Is.EqualTo(0));
    }

    [Test]
    public void DevModeSpawnsCompanionAndShowsRepository()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Dev));

        Assert.IsNotNull(Object.FindFirstObjectByType<CompanionAutomaton>());
        Assert.IsNotNull(Object.FindFirstObjectByType<BuildOptionsMenu>());
        Assert.IsNotNull(Object.FindFirstObjectByType<SettingsMenuController>());
    }

    [Test]
    public void RealModeSkipsCompanionAndHidesRepository()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Real));

        Assert.IsNull(Object.FindFirstObjectByType<CompanionAutomaton>());
        Assert.IsNull(Object.FindFirstObjectByType<BuildOptionsMenu>());
    }

    [Test]
    public void DevModeDisablesPowerGatingAndRealModeRequiresIt()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Dev));
        PowerNetworkController devController = Object.FindFirstObjectByType<PowerNetworkController>();
        Assert.IsNotNull(devController);
        Assert.IsFalse(devController.requirePower);

        CleanupSceneObjects();

        RunBootstrap(GameModeCatalog.Create(GameModeId.Real));
        PowerNetworkController realController = Object.FindFirstObjectByType<PowerNetworkController>();
        Assert.IsNotNull(realController);
        Assert.IsTrue(realController.requirePower);
    }

    [Test]
    public void RealModeSetsStarterAsteroidCountToZero()
    {
        StarSystemGenerator generator = new GameObject("Generator").AddComponent<StarSystemGenerator>();

        generator.ApplyGameModeRules(GameModeCatalog.Create(GameModeId.Real));

        Assert.That(generator.starterAsteroidCount, Is.EqualTo(0));
    }

    [Test]
    public void RealModeAssignsDifferentSeedsAcrossFreshRuns()
    {
        GameModeRules rules = GameModeCatalog.Create(GameModeId.Real);
        StarSystemGenerator first = new GameObject("First Generator").AddComponent<StarSystemGenerator>();
        StarSystemGenerator second = new GameObject("Second Generator").AddComponent<StarSystemGenerator>();
        int nextSeed = 5000;

        first.ApplyGameModeRules(rules, () => ++nextSeed);
        second.ApplyGameModeRules(rules, () => ++nextSeed);

        Assert.AreNotEqual(first.seed, second.seed);
    }

    [Test]
    public void DevModeUsesFixedSeedWhenConfigured()
    {
        GameModeRules rules = GameModeCatalog.Create(GameModeId.Dev);

        Assert.That(StarSystemGenerator.ResolveSeed(rules), Is.EqualTo(GameModeCatalog.DefaultDevSeed));
    }

    [Test]
    public void MainMenuButtonsPointAtExpectedScenes()
    {
        Assert.That(PlaceholderMainMenu.PrimaryButtonSceneName, Is.EqualTo(GameModeCatalog.RealGameSceneName));
        Assert.That(PlaceholderMainMenu.DevButtonSceneName, Is.EqualTo(GameModeCatalog.DevGameSceneName));
    }

    [Test]
    public void SettingsControlsPageListsGameplayBindings()
    {
        Assert.That(SettingsMenuController.ControlBindingCount, Is.GreaterThanOrEqualTo(20));
    }

    [Test]
    public void BootstrapAddsCrashDamageToPlayerShip()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Dev));

        ResourceInventory player = Object.FindFirstObjectByType<ResourceInventory>();

        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<ShipHealth>());
        Assert.IsNotNull(player.GetComponent<ShipCrashDamage>());
    }

    [Test]
    public void BootstrapAddsAutopilotToPlayerShip()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Dev));

        ResourceInventory player = Object.FindFirstObjectByType<ResourceInventory>();

        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<PlayerAutopilotController>());
    }

    [Test]
    public void BootstrapRunsOnlyForGameplayScenes()
    {
        Assert.IsFalse(GameBootstrap.ShouldBootstrapScene(GameModeCatalog.MainMenuSceneName));
        Assert.IsTrue(GameBootstrap.ShouldBootstrapScene(GameModeCatalog.DevGameSceneName));
        Assert.IsTrue(GameBootstrap.ShouldBootstrapScene(GameModeCatalog.RealGameSceneName));
    }

    private static void RunBootstrap(GameModeRules rules)
    {
        GameObject bootstrapObject = new GameObject("Bootstrap");
        bootstrapObject.SetActive(false);
        GameBootstrap bootstrap = bootstrapObject.AddComponent<GameBootstrap>();
        bootstrap.RunBootstrap(rules);
    }

    private static void CleanupSceneObjects()
    {
        GameObject[] gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        for (int i = 0; i < gameObjects.Length; i++)
        {
            Object.DestroyImmediate(gameObjects[i]);
        }
    }
}
#endif
