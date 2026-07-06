using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenuController : MonoBehaviour
{
    private static readonly string[] TabNames = { "Controls", "Settings" };
    private static readonly int[] FrameRateOptions = { 30, 60, 120, 144, 240 };

    private Rect windowRect;
    private bool isOpen;
    private int selectedTab;
    private GUIStyle titleStyle;
    private GUIStyle tabStyle;
    private GUIStyle labelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle smallButtonStyle;

    public bool IsOpen => isOpen;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }
    }

    private void OnGUI()
    {
        GUI.depth = -1100;
        EnsureStyles();

        if (!isOpen)
        {
            DrawGearButton();
            return;
        }

        EnsurePosition();
        PixelUiSprites.Draw(windowRect, PixelUiFrame.Panel);
        PixelUiSprites.Draw(new Rect(windowRect.x + 8f, windowRect.y + 8f, windowRect.width - 16f, 48f), PixelUiFrame.Header);

        GUI.Label(new Rect(windowRect.x + 24f, windowRect.y + 18f, windowRect.width - 96f, 28f), "SETTINGS [ESC]", titleStyle);
        if (DrawButton(new Rect(windowRect.xMax - 68f, windowRect.y + 16f, 44f, 32f), "X", smallButtonStyle))
        {
            isOpen = false;
        }

        DrawTabs(new Rect(windowRect.x + 24f, windowRect.y + 76f, windowRect.width - 48f, 42f));
        Rect contentRect = new Rect(windowRect.x + 24f, windowRect.y + 134f, windowRect.width - 48f, windowRect.height - 158f);
        PixelUiSprites.Draw(contentRect, PixelUiFrame.InnerPanel);

        Rect padded = new Rect(contentRect.x + 18f, contentRect.y + 18f, contentRect.width - 36f, contentRect.height - 36f);
        if (selectedTab == 0)
        {
            DrawControlsPage(padded);
        }
        else
        {
            DrawSettingsPage(padded);
        }
    }

    private void DrawGearButton()
    {
        float size = 46f;
        Rect buttonRect = new Rect(Screen.width - size - 16f, Screen.height - size - 16f, size, size);
        bool hover = buttonRect.Contains(Event.current.mousePosition);

        PixelUiSprites.Draw(buttonRect, hover ? PixelUiFrame.ButtonHover : PixelUiFrame.Button);
        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
        {
            isOpen = true;
        }

        GUI.Label(buttonRect, "⚙", titleStyle);
    }

    private void EnsurePosition()
    {
        float width = Mathf.Clamp(Screen.width * 0.58f, 520f, 780f);
        float height = Mathf.Clamp(Screen.height * 0.62f, 420f, 620f);
        windowRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
    }

    private void DrawTabs(Rect rect)
    {
        float tabWidth = rect.width / TabNames.Length;
        for (int i = 0; i < TabNames.Length; i++)
        {
            Rect tabRect = new Rect(rect.x + (tabWidth * i), rect.y, tabWidth - 4f, rect.height);
            bool hover = tabRect.Contains(Event.current.mousePosition);
            PixelUiFrame frame = i == selectedTab ? PixelUiFrame.TabActive : hover ? PixelUiFrame.TabHover : PixelUiFrame.Tab;

            PixelUiSprites.Draw(tabRect, frame);
            if (GUI.Button(tabRect, GUIContent.none, GUIStyle.none))
            {
                selectedTab = i;
            }

            GUI.Label(tabRect, TabNames[i].ToUpperInvariant(), tabStyle);
        }
    }

    private void DrawControlsPage(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 30f), "Movement Control Type", labelStyle);
        GUI.Label(
            new Rect(rect.x, rect.y + 34f, rect.width, 72f),
            "Newtonian Physics keeps free momentum. Simple Controls still accelerate without a speed cap, but keep movement aligned with the direction the ship is facing.",
            bodyStyle);

        float y = rect.y + 122f;
        DrawMovementOption(
            new Rect(rect.x, y, rect.width, 58f),
            MovementControlType.NewtonianPhysics,
            "Thrust adds momentum. Space brakes.");

        DrawMovementOption(
            new Rect(rect.x, y + 68f, rect.width, 58f),
            MovementControlType.Simple,
            "Forward accelerates in the current facing direction. Releasing thrust slows to a stop.");
    }

    private void DrawMovementOption(Rect rect, MovementControlType movementControl, string description)
    {
        bool selected = GameSettings.MovementControl == movementControl;
        PixelUiSprites.Draw(rect, selected ? PixelUiFrame.CardUnlocked : PixelUiFrame.Card);

        string label = selected ? "SELECTED" : "SELECT";
        GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width - 150f, 22f), GameSettings.MovementControlLabel(movementControl), labelStyle);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 30f, rect.width - 150f, 22f), description, bodyStyle);

        if (DrawButton(new Rect(rect.xMax - 124f, rect.y + 13f, 104f, 32f), label, smallButtonStyle))
        {
            GameSettings.SetMovementControl(movementControl);
        }
    }

    private void DrawSettingsPage(Rect rect)
    {
        float y = rect.y;
        GUI.Label(new Rect(rect.x, y, rect.width, 30f), "Graphics", labelStyle);
        y += 46f;

        DrawStepper(
            new Rect(rect.x, y, rect.width, 42f),
            "Quality",
            QualityName(GameSettings.QualityLevel),
            () => GameSettings.SetQualityLevel(GameSettings.QualityLevel - 1),
            () => GameSettings.SetQualityLevel(GameSettings.QualityLevel + 1),
            GameSettings.QualityLevel > 0,
            GameSettings.QualityLevel < QualitySettings.names.Length - 1);

        y += 54f;
        DrawToggle(new Rect(rect.x, y, rect.width, 42f), "VSync", GameSettings.VSync, GameSettings.SetVSync);

        y += 54f;
        DrawStepper(
            new Rect(rect.x, y, rect.width, 42f),
            "Target FPS",
            GameSettings.VSync ? "VSync" : GameSettings.TargetFrameRate.ToString(),
            () => GameSettings.SetTargetFrameRate(PreviousFrameRate(GameSettings.TargetFrameRate)),
            () => GameSettings.SetTargetFrameRate(NextFrameRate(GameSettings.TargetFrameRate)),
            !GameSettings.VSync && GameSettings.TargetFrameRate > FrameRateOptions[0],
            !GameSettings.VSync && GameSettings.TargetFrameRate < FrameRateOptions[FrameRateOptions.Length - 1]);

        y += 54f;
        DrawToggle(new Rect(rect.x, y, rect.width, 42f), "Fullscreen", GameSettings.FullScreen, GameSettings.SetFullScreen);
    }

    private void DrawToggle(Rect rect, string label, bool value, System.Action<bool> setter)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.Card);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width - 140f, 26f), label, labelStyle);

        if (DrawButton(new Rect(rect.xMax - 124f, rect.y + 5f, 104f, 32f), value ? "ON" : "OFF", smallButtonStyle))
        {
            setter(!value);
        }
    }

    private void DrawStepper(Rect rect, string label, string value, System.Action previous, System.Action next, bool canPrevious, bool canNext)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.Card);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, rect.width * 0.36f, 26f), label, labelStyle);
        GUI.Label(new Rect(rect.x + rect.width * 0.42f, rect.y + 8f, rect.width * 0.28f, 26f), value, bodyStyle);

        GUI.enabled = canPrevious;
        if (DrawButton(new Rect(rect.xMax - 124f, rect.y + 5f, 46f, 32f), "<", smallButtonStyle))
        {
            previous();
        }

        GUI.enabled = canNext;
        if (DrawButton(new Rect(rect.xMax - 66f, rect.y + 5f, 46f, 32f), ">", smallButtonStyle))
        {
            next();
        }

        GUI.enabled = true;
    }

    private bool DrawButton(Rect rect, string label, GUIStyle style)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        PixelUiSprites.Draw(rect, hover ? PixelUiFrame.ButtonHover : PixelUiFrame.Button);
        return GUI.Button(rect, label, style);
    }

    private static string QualityName(int qualityLevel)
    {
        if (QualitySettings.names.Length <= 0)
        {
            return "Default";
        }

        int index = Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1);
        return QualitySettings.names[index];
    }

    private static int PreviousFrameRate(int current)
    {
        for (int i = FrameRateOptions.Length - 1; i >= 0; i--)
        {
            if (FrameRateOptions[i] < current)
            {
                return FrameRateOptions[i];
            }
        }

        return FrameRateOptions[0];
    }

    private static int NextFrameRate(int current)
    {
        for (int i = 0; i < FrameRateOptions.Length; i++)
        {
            if (FrameRateOptions[i] > current)
            {
                return FrameRateOptions[i];
            }
        }

        return FrameRateOptions[FrameRateOptions.Length - 1];
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = PixelUiSprites.Gold }
        };

        tabStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            hover = { textColor = PixelUiSprites.Gold },
            active = { textColor = Color.white }
        };

        smallButtonStyle = new GUIStyle(buttonStyle)
        {
            fontSize = 13
        };
    }
}
