using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenuController : MonoBehaviour
{
    private static readonly string[] SectionNames = { "Graphics", "Controls", "Gameplay" };
    private static readonly int[] FrameRateOptions = { 30, 60, 120, 144, 240 };
    private static readonly ControlBinding[] ControlBindings =
    {
        new ControlBinding("Rotate Left", "A / Left Arrow"),
        new ControlBinding("Rotate Right", "D / Right Arrow"),
        new ControlBinding("Forward Thrust", "W / Up Arrow"),
        new ControlBinding("Reverse Thrust", "S / Down Arrow"),
        new ControlBinding("Brake", "Space"),
        new ControlBinding("Fire Mining Laser", "Left Mouse / Left Ctrl"),
        new ControlBinding("Interact / Mine", "E"),
        new ControlBinding("Radar Ping", "1"),
        new ControlBinding("Scan", "F"),
        new ControlBinding("Map", "M"),
        new ControlBinding("Set Autopilot Destination", "Left-click full map"),
        new ControlBinding("Build Menu", "I"),
        new ControlBinding("Settings / Controls", "Esc / Gear"),
        new ControlBinding("Toggle Routes", "T"),
        new ControlBinding("Place Mine", "2"),
        new ControlBinding("Place Condenser", "3"),
        new ControlBinding("Place Harvester", "4"),
        new ControlBinding("Place Dredger", "5"),
        new ControlBinding("Place Collector", "6"),
        new ControlBinding("Place Hub", "7"),
        new ControlBinding("Place Freighter", "8"),
        new ControlBinding("Place Storage", "9"),
        new ControlBinding("Place Satellite Factory", "0"),
        new ControlBinding("Confirm Placement / Select", "Left Mouse"),
        new ControlBinding("Cancel Placement / Drag", "Right Mouse / Esc"),
        new ControlBinding("Dialogue Continue", "Enter"),
        new ControlBinding("Dialogue Choice", "1 - 4")
    };

    private Rect windowRect;
    private bool isOpen;
    private int selectedSection;
    private GUIStyle titleStyle;
    private GUIStyle sectionButtonStyle;
    private GUIStyle labelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle valueStyle;
    private GUIStyle buttonStyle;
    private GUIStyle smallButtonStyle;
    private Vector2 controlsScrollPosition;
    private float styleScale;

    public bool IsOpen => isOpen;
    public static int ControlBindingCount => ControlBindings.Length;
    public static int TabCount => SectionNames.Length;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (CloseBlockingOverlayBeforeSettings())
            {
                return;
            }

            isOpen = !isOpen;
        }
    }

    public static bool CloseBlockingOverlayBeforeSettings()
    {
        BasicMapController map = FindFirstObjectByType<BasicMapController>();
        if (map != null && map.CloseFullMap())
        {
            return true;
        }

        BuildOptionsMenu buildMenu = FindFirstObjectByType<BuildOptionsMenu>();
        return buildMenu != null && buildMenu.CloseMenu();
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
        DrawSettingsConsole();
    }

    private void DrawGearButton()
    {
        float scale = GameUiScale.Current;
        float size = GameUiScale.Size(46f, scale);
        Rect buttonRect = new Rect(Screen.width - size - GameUiScale.Size(16f, scale), Screen.height - size - GameUiScale.Size(16f, scale), size, size);
        bool hover = buttonRect.Contains(Event.current.mousePosition);

        DrawLauncherButtonFrame(buttonRect, hover, scale);
        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
        {
            isOpen = true;
        }

        DrawSettingsIcon(buttonRect, scale);
    }

    private void EnsurePosition()
    {
        float scale = GameUiScale.Current;
        float width = Mathf.Clamp(Screen.width * 0.78f, GameUiScale.Size(560f, scale), Screen.width - GameUiScale.Size(32f, scale));
        float height = Mathf.Clamp(Screen.height * 0.78f, GameUiScale.Size(430f, scale), Screen.height - GameUiScale.Size(32f, scale));
        windowRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
    }

    private void DrawSettingsConsole()
    {
        float scale = GameUiScale.Current;
        PixelUiSprites.Draw(windowRect, PixelUiFrame.ConsolePanel);

        Rect headerRect = new Rect(
            windowRect.x + GameUiScale.Size(10f, scale),
            windowRect.y + GameUiScale.Size(10f, scale),
            windowRect.width - GameUiScale.Size(20f, scale),
            GameUiScale.Size(50f, scale));
        PixelUiSprites.Draw(headerRect, PixelUiFrame.ConsoleHeader);

        Rect titleRect = new Rect(
            headerRect.x + GameUiScale.Size(18f, scale),
            headerRect.y + GameUiScale.Size(8f, scale),
            headerRect.width - GameUiScale.Size(86f, scale),
            headerRect.height - GameUiScale.Size(16f, scale));
        DrawTextBlock(titleRect, PixelUiSprites.ConsoleSurface);
        GUI.Label(titleRect, "SETTINGS", titleStyle);

        if (DrawButton(
            new Rect(headerRect.xMax - GameUiScale.Size(54f, scale), headerRect.y + GameUiScale.Size(9f, scale), GameUiScale.Size(38f, scale), GameUiScale.Size(32f, scale)),
            "X",
            smallButtonStyle))
        {
            isOpen = false;
        }

        Rect bodyRect = new Rect(
            windowRect.x + GameUiScale.Size(16f, scale),
            windowRect.y + GameUiScale.Size(74f, scale),
            windowRect.width - GameUiScale.Size(32f, scale),
            windowRect.height - GameUiScale.Size(94f, scale));

        float sidebarWidth = Mathf.Min(GameUiScale.Size(178f, scale), bodyRect.width * 0.34f);
        float gap = GameUiScale.Size(14f, scale);
        Rect sidebarRect = new Rect(bodyRect.x, bodyRect.y, sidebarWidth, bodyRect.height);
        Rect contentRect = new Rect(sidebarRect.xMax + gap, bodyRect.y, bodyRect.width - sidebarWidth - gap, bodyRect.height);

        PixelUiSprites.Draw(sidebarRect, PixelUiFrame.ConsoleInnerPanel);
        PixelUiSprites.Draw(contentRect, PixelUiFrame.ConsoleInnerPanel);

        DrawSidebar(GameUiScale.Inset(sidebarRect, 12f, 12f), scale);
        DrawSelectedSection(GameUiScale.Inset(contentRect, 18f, 16f));
    }

    private void DrawSidebar(Rect rect, float scale)
    {
        Rect headingRect = new Rect(rect.x, rect.y, rect.width, GameUiScale.Size(28f, scale));
        DrawTextBlock(headingRect, PixelUiSprites.ConsoleInset);
        GUI.Label(headingRect, "SECTIONS", labelStyle);

        float y = rect.y + GameUiScale.Size(42f, scale);
        for (int i = 0; i < SectionNames.Length; i++)
        {
            Rect buttonRect = new Rect(rect.x, y, rect.width, GameUiScale.Size(42f, scale));
            DrawSidebarButton(buttonRect, i);
            y += GameUiScale.Size(52f, scale);
        }
    }

    private void DrawSidebarButton(Rect rect, int index)
    {
        bool selected = index == selectedSection;
        bool hover = rect.Contains(Event.current.mousePosition);
        PixelUiFrame frame = selected ? PixelUiFrame.ConsoleButtonActive : hover ? PixelUiFrame.ConsoleButtonHover : PixelUiFrame.ConsoleButton;

        PixelUiSprites.Draw(rect, frame);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
        {
            selectedSection = index;
        }

        GUI.Label(rect, SectionNames[index].ToUpperInvariant(), sectionButtonStyle);
    }

    private void DrawSelectedSection(Rect rect)
    {
        switch (selectedSection)
        {
            case 0:
                DrawGraphicsSection(rect);
                break;
            case 1:
                DrawControlsSection(rect);
                break;
            case 2:
                DrawGameplaySection(rect);
                break;
        }
    }

    private void DrawSectionHeader(Rect rect, string title, string subtitle)
    {
        float scale = GameUiScale.Current;
        Rect titleRect = new Rect(rect.x, rect.y, rect.width, GameUiScale.Size(28f, scale));
        DrawTextBlock(titleRect, PixelUiSprites.ConsoleInset);
        GUI.Label(titleRect, title.ToUpperInvariant(), labelStyle);

        if (!string.IsNullOrEmpty(subtitle))
        {
            Rect subtitleRect = new Rect(rect.x, rect.y + GameUiScale.Size(32f, scale), rect.width, GameUiScale.Size(40f, scale));
            DrawTextBlock(subtitleRect, PixelUiSprites.ConsoleInset);
            GUI.Label(subtitleRect, subtitle, bodyStyle);
        }
    }

    private void DrawGraphicsSection(Rect rect)
    {
        float scale = GameUiScale.Current;
        DrawSectionHeader(rect, "Graphics", "Display settings apply immediately and are saved by the existing game settings system.");

        float y = rect.y + GameUiScale.Size(86f, scale);
        float rowHeight = GameUiScale.Size(52f, scale);
        float rowGap = GameUiScale.Size(12f, scale);

        DrawStepperRow(
            new Rect(rect.x, y, rect.width, rowHeight),
            "Quality",
            QualityName(GameSettings.QualityLevel),
            () => GameSettings.SetQualityLevel(GameSettings.QualityLevel - 1),
            () => GameSettings.SetQualityLevel(GameSettings.QualityLevel + 1),
            GameSettings.QualityLevel > 0,
            GameSettings.QualityLevel < QualitySettings.names.Length - 1);

        y += rowHeight + rowGap;
        DrawToggleRow(new Rect(rect.x, y, rect.width, rowHeight), "VSync", GameSettings.VSync, GameSettings.SetVSync);

        y += rowHeight + rowGap;
        DrawStepperRow(
            new Rect(rect.x, y, rect.width, rowHeight),
            "Target FPS",
            GameSettings.VSync ? "VSync" : GameSettings.TargetFrameRate.ToString(),
            () => GameSettings.SetTargetFrameRate(PreviousFrameRate(GameSettings.TargetFrameRate)),
            () => GameSettings.SetTargetFrameRate(NextFrameRate(GameSettings.TargetFrameRate)),
            !GameSettings.VSync && GameSettings.TargetFrameRate > FrameRateOptions[0],
            !GameSettings.VSync && GameSettings.TargetFrameRate < FrameRateOptions[FrameRateOptions.Length - 1]);

        y += rowHeight + rowGap;
        DrawToggleRow(new Rect(rect.x, y, rect.width, rowHeight), "Fullscreen", GameSettings.FullScreen, GameSettings.SetFullScreen);
    }

    private void DrawControlsSection(Rect rect)
    {
        float scale = GameUiScale.Current;
        DrawSectionHeader(rect, "Controls", "Reference only. Bindings are unchanged.");

        float listY = GameUiScale.Size(82f, scale);
        float rowHeight = GameUiScale.Size(40f, scale);
        float rowGap = GameUiScale.Size(8f, scale);
        float contentHeight = ControlBindings.Length * (rowHeight + rowGap);
        Rect scrollRect = new Rect(rect.x, rect.y + listY, rect.width, rect.height - listY);
        Rect viewRect = new Rect(0f, 0f, scrollRect.width - GameUiScale.Size(18f, scale), contentHeight);

        controlsScrollPosition = GUI.BeginScrollView(scrollRect, controlsScrollPosition, viewRect);
        float y = 0f;
        for (int i = 0; i < ControlBindings.Length; i++)
        {
            DrawControlBindingRow(new Rect(0f, y, viewRect.width, rowHeight), ControlBindings[i]);
            y += rowHeight + rowGap;
        }

        GUI.EndScrollView();
    }

    private void DrawGameplaySection(Rect rect)
    {
        float scale = GameUiScale.Current;
        DrawSectionHeader(
            rect,
            "Gameplay",
            "Choose how thrust input maps to ship movement. This does not change bindings or pause behavior.");

        float y = rect.y + GameUiScale.Size(94f, scale);
        DrawMovementOption(
            new Rect(rect.x, y, rect.width, GameUiScale.Size(74f, scale)),
            MovementControlType.NewtonianPhysics,
            "Thrust adds momentum. Space brakes.");

        DrawMovementOption(
            new Rect(rect.x, y + GameUiScale.Size(88f, scale), rect.width, GameUiScale.Size(74f, scale)),
            MovementControlType.Simple,
            "Forward accelerates in the current facing direction. Releasing thrust slows to a stop.");
    }

    private void DrawControlBindingRow(Rect rect, ControlBinding binding)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.ConsoleRow);
        float scale = GameUiScale.Current;
        Rect laneRect = new Rect(rect.x + GameUiScale.Size(8f, scale), rect.y + GameUiScale.Size(6f, scale), rect.width - GameUiScale.Size(16f, scale), rect.height - GameUiScale.Size(12f, scale));
        DrawTextBlock(laneRect, PixelUiSprites.ConsoleInset);

        float valueWidth = Mathf.Min(GameUiScale.Size(230f, scale), rect.width * 0.42f);
        GUI.Label(new Rect(rect.x + GameUiScale.Size(14f, scale), rect.y + GameUiScale.Size(7f, scale), rect.width - valueWidth - GameUiScale.Size(28f, scale), rect.height - GameUiScale.Size(14f, scale)), binding.action, bodyStyle);
        DrawValuePill(new Rect(rect.xMax - valueWidth - GameUiScale.Size(12f, scale), rect.y + GameUiScale.Size(7f, scale), valueWidth, rect.height - GameUiScale.Size(14f, scale)), binding.buttons);
    }

    private void DrawMovementOption(Rect rect, MovementControlType movementControl, string description)
    {
        bool selected = GameSettings.MovementControl == movementControl;
        PixelUiSprites.Draw(rect, selected ? PixelUiFrame.ConsoleRowSelected : PixelUiFrame.ConsoleRow);

        float scale = GameUiScale.Current;
        float buttonWidth = GameUiScale.Size(112f, scale);
        Rect laneRect = new Rect(rect.x + GameUiScale.Size(10f, scale), rect.y + GameUiScale.Size(8f, scale), rect.width - buttonWidth - GameUiScale.Size(34f, scale), rect.height - GameUiScale.Size(16f, scale));
        DrawTextBlock(laneRect, PixelUiSprites.ConsoleInset);

        GUI.Label(new Rect(laneRect.x + GameUiScale.Size(8f, scale), laneRect.y + GameUiScale.Size(2f, scale), laneRect.width - GameUiScale.Size(16f, scale), GameUiScale.Size(24f, scale)), GameSettings.MovementControlLabel(movementControl), labelStyle);
        GUI.Label(new Rect(laneRect.x + GameUiScale.Size(8f, scale), laneRect.y + GameUiScale.Size(28f, scale), laneRect.width - GameUiScale.Size(16f, scale), GameUiScale.Size(24f, scale)), description, bodyStyle);

        string label = selected ? "SELECTED" : "SELECT";
        if (DrawButton(new Rect(rect.xMax - buttonWidth - GameUiScale.Size(14f, scale), rect.y + GameUiScale.Size(21f, scale), buttonWidth, GameUiScale.Size(32f, scale)), label, smallButtonStyle))
        {
            GameSettings.SetMovementControl(movementControl);
        }
    }

    private void DrawToggleRow(Rect rect, string label, bool value, System.Action<bool> setter)
    {
        DrawSettingRowFrame(rect);
        float scale = GameUiScale.Current;
        GUI.Label(new Rect(rect.x + GameUiScale.Size(16f, scale), rect.y + GameUiScale.Size(12f, scale), rect.width * 0.42f, GameUiScale.Size(28f, scale)), label, labelStyle);
        DrawValuePill(new Rect(rect.xMax - GameUiScale.Size(204f, scale), rect.y + GameUiScale.Size(10f, scale), GameUiScale.Size(72f, scale), GameUiScale.Size(32f, scale)), value ? "ON" : "OFF");

        if (DrawButton(new Rect(rect.xMax - GameUiScale.Size(118f, scale), rect.y + GameUiScale.Size(10f, scale), GameUiScale.Size(104f, scale), GameUiScale.Size(32f, scale)), value ? "ON" : "OFF", smallButtonStyle))
        {
            setter(!value);
        }
    }

    private void DrawStepperRow(Rect rect, string label, string value, System.Action previous, System.Action next, bool canPrevious, bool canNext)
    {
        DrawSettingRowFrame(rect);
        float scale = GameUiScale.Current;
        GUI.Label(new Rect(rect.x + GameUiScale.Size(16f, scale), rect.y + GameUiScale.Size(12f, scale), rect.width * 0.34f, GameUiScale.Size(28f, scale)), label, labelStyle);
        DrawValuePill(new Rect(rect.xMax - GameUiScale.Size(286f, scale), rect.y + GameUiScale.Size(10f, scale), GameUiScale.Size(150f, scale), GameUiScale.Size(32f, scale)), value);

        bool previousEnabled = GUI.enabled;
        GUI.enabled = canPrevious;
        if (DrawButton(new Rect(rect.xMax - GameUiScale.Size(124f, scale), rect.y + GameUiScale.Size(10f, scale), GameUiScale.Size(48f, scale), GameUiScale.Size(32f, scale)), "<", smallButtonStyle))
        {
            previous();
        }

        GUI.enabled = canNext;
        if (DrawButton(new Rect(rect.xMax - GameUiScale.Size(62f, scale), rect.y + GameUiScale.Size(10f, scale), GameUiScale.Size(48f, scale), GameUiScale.Size(32f, scale)), ">", smallButtonStyle))
        {
            next();
        }

        GUI.enabled = previousEnabled;
    }

    private void DrawSettingRowFrame(Rect rect)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.ConsoleRow);
        float scale = GameUiScale.Current;
        Rect laneRect = new Rect(rect.x + GameUiScale.Size(10f, scale), rect.y + GameUiScale.Size(8f, scale), rect.width - GameUiScale.Size(20f, scale), rect.height - GameUiScale.Size(16f, scale));
        DrawTextBlock(laneRect, PixelUiSprites.ConsoleInset);
    }

    private void DrawValuePill(Rect rect, string value)
    {
        PixelUiSprites.Draw(rect, PixelUiFrame.ConsoleChip);
        DrawTextBlock(new Rect(rect.x + GameUiScale.Size(4f), rect.y + GameUiScale.Size(4f), rect.width - GameUiScale.Size(8f), rect.height - GameUiScale.Size(8f)), PixelUiSprites.ConsoleDeep);
        GUI.Label(rect, value.ToUpperInvariant(), valueStyle);
    }

    private bool DrawButton(Rect rect, string label, GUIStyle style)
    {
        bool hover = rect.Contains(Event.current.mousePosition);
        PixelUiSprites.Draw(rect, hover ? PixelUiFrame.ConsoleButtonHover : PixelUiFrame.ConsoleButton);
        return GUI.Button(rect, label, style);
    }

    private static void DrawSettingsIcon(Rect rect, float scale)
    {
        Vector2 center = rect.center;
        Color color = PixelUiSprites.ConsoleAccent;
        float lineWidth = GameUiScale.Size(2f, scale);
        float spokeOuter = GameUiScale.Size(13f, scale);
        float spokeInner = GameUiScale.Size(8f, scale);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 0.25f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            DrawIconLine(center + (direction * spokeInner), center + (direction * spokeOuter), color, lineWidth);
        }

        DrawIconCircle(center, GameUiScale.Size(7f, scale), color, lineWidth, 20);
        DrawIconCircle(center, GameUiScale.Size(3f, scale), color, lineWidth, 14);
    }

    private static void DrawLauncherButtonFrame(Rect rect, bool hover, float scale)
    {
        Color fill = hover ? PixelUiSprites.ConsoleActive : PixelUiSprites.ConsoleSurface;
        Color edge = hover ? PixelUiSprites.Cyan : PixelUiSprites.ConsoleAccent;
        float border = GameUiScale.Size(2f, scale);
        float inset = GameUiScale.Size(5f, scale);

        DrawTextBlock(rect, new Color(0f, 0f, 0f, 0.48f));
        Rect body = new Rect(rect.x + border, rect.y + border, rect.width - (border * 2f), rect.height - (border * 2f));
        DrawTextBlock(body, fill);

        DrawTextBlock(new Rect(body.x, body.y, body.width, border), edge);
        DrawTextBlock(new Rect(body.x, body.yMax - border, body.width, border), PixelUiSprites.DarkPurple);
        DrawTextBlock(new Rect(body.x, body.y, border, body.height), edge);
        DrawTextBlock(new Rect(body.xMax - border, body.y, border, body.height), PixelUiSprites.DarkPurple);

        DrawTextBlock(new Rect(body.x + inset, body.y + GameUiScale.Size(4f, scale), body.width - (inset * 2f), border), new Color(edge.r, edge.g, edge.b, 0.36f));
    }

    private static void DrawIconCircle(Vector2 center, float radius, Color color, float width, int segments)
    {
        Vector2 previous = center + new Vector2(radius, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;
            Vector2 next = center + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            DrawIconLine(previous, next, color, width);
            previous = next;
        }
    }

    private static void DrawIconLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - (width * 0.5f), delta.magnitude, width), Texture2D.whiteTexture);
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    private static void DrawTextBlock(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
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
        float scale = GameUiScale.Current;
        if (titleStyle != null && Mathf.Approximately(styleScale, scale))
        {
            return;
        }

        styleScale = scale;
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(22f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = PixelUiSprites.ConsoleAccent }
        };

        sectionButtonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(13f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(18f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(14f, scale),
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(13f, scale),
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = PixelUiSprites.ConsoleAccent }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = GameUiScale.Font(17f, scale),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            hover = { textColor = PixelUiSprites.ConsoleAccent },
            active = { textColor = Color.white }
        };

        smallButtonStyle = new GUIStyle(buttonStyle)
        {
            fontSize = GameUiScale.Font(13f, scale)
        };
    }

    private struct ControlBinding
    {
        public readonly string action;
        public readonly string buttons;

        public ControlBinding(string action, string buttons)
        {
            this.action = action;
            this.buttons = buttons;
        }
    }
}
