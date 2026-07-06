using UnityEngine;

public static class GameSettings
{
    private const string MovementControlKey = "settings.movementControl";
    private const string QualityLevelKey = "settings.qualityLevel";
    private const string VSyncKey = "settings.vSync";
    private const string TargetFrameRateKey = "settings.targetFrameRate";
    private const string FullScreenKey = "settings.fullScreen";

    public static MovementControlType MovementControl { get; private set; } = MovementControlType.NewtonianPhysics;
    public static int QualityLevel { get; private set; }
    public static bool VSync { get; private set; } = true;
    public static int TargetFrameRate { get; private set; } = 60;
    public static bool FullScreen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForRuntime()
    {
        MovementControl = MovementControlType.NewtonianPhysics;
        QualityLevel = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        VSync = QualitySettings.vSyncCount > 0;
        TargetFrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
        FullScreen = Screen.fullScreen;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadAtStartup()
    {
        Load();
        ApplyGraphicsSettings();
    }

    public static void Load()
    {
        MovementControl = (MovementControlType)Mathf.Clamp(
            PlayerPrefs.GetInt(MovementControlKey, (int)MovementControlType.NewtonianPhysics),
            0,
            1);

        int maxQuality = Mathf.Max(0, QualitySettings.names.Length - 1);
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel()), 0, maxQuality);
        VSync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
        TargetFrameRate = Mathf.Clamp(PlayerPrefs.GetInt(TargetFrameRateKey, 60), 30, 240);
        FullScreen = PlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) == 1;
    }

    public static void SetMovementControl(MovementControlType movementControl)
    {
        MovementControl = movementControl;
        PlayerPrefs.SetInt(MovementControlKey, (int)MovementControl);
        PlayerPrefs.Save();
    }

    public static void SetQualityLevel(int qualityLevel)
    {
        QualityLevel = Mathf.Clamp(qualityLevel, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
        PlayerPrefs.SetInt(QualityLevelKey, QualityLevel);
        ApplyGraphicsSettings();
        PlayerPrefs.Save();
    }

    public static void SetVSync(bool enabled)
    {
        VSync = enabled;
        PlayerPrefs.SetInt(VSyncKey, VSync ? 1 : 0);
        ApplyGraphicsSettings();
        PlayerPrefs.Save();
    }

    public static void SetTargetFrameRate(int targetFrameRate)
    {
        TargetFrameRate = Mathf.Clamp(targetFrameRate, 30, 240);
        PlayerPrefs.SetInt(TargetFrameRateKey, TargetFrameRate);
        ApplyGraphicsSettings();
        PlayerPrefs.Save();
    }

    public static void SetFullScreen(bool fullScreen)
    {
        FullScreen = fullScreen;
        PlayerPrefs.SetInt(FullScreenKey, FullScreen ? 1 : 0);
        ApplyGraphicsSettings();
        PlayerPrefs.Save();
    }

    public static void ApplyGraphicsSettings()
    {
        if (QualitySettings.names.Length > 0)
        {
            QualitySettings.SetQualityLevel(QualityLevel, true);
        }

        QualitySettings.vSyncCount = VSync ? 1 : 0;
        Application.targetFrameRate = VSync ? -1 : TargetFrameRate;
        Screen.fullScreen = FullScreen;
    }

    public static string MovementControlLabel(MovementControlType movementControl)
    {
        switch (movementControl)
        {
            case MovementControlType.Simple:
                return "Simple Controls";
            case MovementControlType.NewtonianPhysics:
            default:
                return "Newtonian Physics";
        }
    }
}
