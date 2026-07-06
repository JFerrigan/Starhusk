using UnityEngine;

public enum GameModeId
{
    Dev,
    Real
}

[System.Serializable]
public class GameModeRules
{
    public GameModeId modeId;
    public string gameplaySceneName;
    public int startingResourceAmountPerType;
    public bool spawnStartingCompanion;
    public bool requirePower;
    public bool showRepository;
    public int starterAsteroidCount;
    public bool useRandomSeed;
    public int fixedSeed;
    public bool requireIntroDialogue;

    public GameModeRules Clone()
    {
        return (GameModeRules)MemberwiseClone();
    }
}

public static class GameModeCatalog
{
    public const string MainMenuSceneName = "MainMenu";
    public const string DevGameSceneName = "Game_Dev";
    public const string RealGameSceneName = "Game_Real";
    public const int DefaultStartingResourceAmount = 1000;
    public const int DefaultDevSeed = 1107;
    public const int DefaultStarterAsteroidCount = 8;

    public static GameModeRules Create(GameModeId modeId)
    {
        switch (modeId)
        {
            case GameModeId.Real:
                return new GameModeRules
                {
                    modeId = GameModeId.Real,
                    gameplaySceneName = RealGameSceneName,
                    startingResourceAmountPerType = 0,
                    spawnStartingCompanion = false,
                    requirePower = true,
                    showRepository = false,
                    starterAsteroidCount = 0,
                    useRandomSeed = true,
                    fixedSeed = DefaultDevSeed,
                    requireIntroDialogue = true
                };
            default:
                return new GameModeRules
                {
                    modeId = GameModeId.Dev,
                    gameplaySceneName = DevGameSceneName,
                    startingResourceAmountPerType = DefaultStartingResourceAmount,
                    spawnStartingCompanion = true,
                    requirePower = false,
                    showRepository = true,
                    starterAsteroidCount = DefaultStarterAsteroidCount,
                    useRandomSeed = false,
                    fixedSeed = DefaultDevSeed,
                    requireIntroDialogue = false
                };
        }
    }

    public static bool TryResolveSceneName(string sceneName, out GameModeRules rules)
    {
        switch (sceneName)
        {
            case DevGameSceneName:
                rules = Create(GameModeId.Dev);
                return true;
            case RealGameSceneName:
                rules = Create(GameModeId.Real);
                return true;
            default:
                rules = null;
                return false;
        }
    }

    public static string SceneNameFor(GameModeId modeId)
    {
        return Create(modeId).gameplaySceneName;
    }
}

public static class GameModeRuntime
{
    private static GameModeRules pendingRules;

    public static void SelectMode(GameModeId modeId)
    {
        pendingRules = GameModeCatalog.Create(modeId);
    }

    public static GameModeRules ResolveActiveRules(string activeSceneName)
    {
        if (pendingRules != null && pendingRules.gameplaySceneName == activeSceneName)
        {
            return pendingRules.Clone();
        }

        if (GameModeCatalog.TryResolveSceneName(activeSceneName, out GameModeRules rules))
        {
            return rules;
        }

        return GameModeCatalog.Create(GameModeId.Dev);
    }

    public static void ClearSelectionForTests()
    {
        pendingRules = null;
    }
}
