using System.Collections.Generic;
using UnityEngine;

public class PlaceholderMainMenu : MonoBehaviour
{
    private const string BackgroundResourceName = "MenuBackground";
    private const string FontResourceName = "MenuFont";

    public string gameTitle = "STARHUSK";
    public string tagline = "";

    private bool menuOpen = true;
    private float previousTimeScale = 1f;

    private Texture2D pixel;
    private Texture2D backgroundImage;
    private Font menuFont;

    private GUIStyle titleStyle;
    private GUIStyle taglineStyle;
    private GUIStyle buttonStyle;
    private GUIStyle smallStyle;

    private readonly List<MonoBehaviour> disabledBehaviours = new List<MonoBehaviour>();

    public static bool IsOpen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMenuExists()
    {
        PlaceholderMainMenu existingMenu = FindFirstObjectByType<PlaceholderMainMenu>();
        if (existingMenu != null)
        {
            return;
        }

        GameObject menuObject = new GameObject("Placeholder Main Menu");
        menuObject.AddComponent<PlaceholderMainMenu>();
    }

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        backgroundImage = Resources.Load<Texture2D>(BackgroundResourceName);
        menuFont = Resources.Load<Font>(FontResourceName);

        CreateNonSkinStyles();
        OpenMenu();
    }

    private void OnDestroy()
    {
        RestoreGameplay();
    }

    private void OnGUI()
    {
        if (!menuOpen)
        {
            return;
        }

        GUI.depth = -1000;

        EnsureSkinStyles();
        DrawBackground();

        float panelWidth = Mathf.Clamp(Screen.width * 0.36f, 420f, 620f);
        Rect panelRect = new Rect(0f, 0f, panelWidth, Screen.height);

        DrawLeftPanel(panelRect);
        DrawMenuContent(panelRect);
    }

    private void CreateNonSkinStyles()
    {
        titleStyle = new GUIStyle
        {
            fontSize = 58,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        taglineStyle = new GUIStyle
        {
            fontSize = 17,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(0.72f, 0.9f, 1f, 0.95f) }
        };

        smallStyle = new GUIStyle
        {
            fontSize = 13,
            alignment = TextAnchor.LowerLeft,
            normal = { textColor = new Color(0.72f, 0.84f, 0.92f, 0.72f) }
        };

        ApplyFont(titleStyle);
        ApplyFont(taglineStyle);
        ApplyFont(smallStyle);
    }

    private void EnsureSkinStyles()
    {
        if (buttonStyle != null)
        {
            return;
        }

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 21,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(24, 16, 0, 0)
        };

        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = new Color(0.55f, 0.95f, 1f, 1f);
        buttonStyle.active.textColor = new Color(1f, 0.86f, 0.34f, 1f);

        ApplyFont(buttonStyle);
    }

    private void ApplyFont(GUIStyle style)
    {
        if (style != null && menuFont != null)
        {
            style.font = menuFont;
        }
    }

    private void DrawBackground()
    {
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);

        if (backgroundImage != null)
        {
            GUI.DrawTexture(screenRect, backgroundImage, ScaleMode.ScaleAndCrop);
        }
        else
        {
            DrawRect(screenRect, new Color(0.005f, 0.008f, 0.018f, 1f));
            DrawFallbackStars();
        }

        DrawRect(screenRect, new Color(0f, 0f, 0f, 0.18f));
    }

    private void DrawLeftPanel(Rect panelRect)
    {
        DrawRect(panelRect, new Color(0f, 0f, 0f, 0.62f));

        Rect darkerStrip = new Rect(0f, 0f, panelRect.width * 0.72f, panelRect.height);
        DrawRect(darkerStrip, new Color(0f, 0f, 0f, 0.28f));

        Rect edgeGlow = new Rect(panelRect.xMax - 3f, 0f, 3f, Screen.height);
        DrawRect(edgeGlow, new Color(0.24f, 0.84f, 0.95f, 0.85f));

        Rect thinEdgeGlow = new Rect(panelRect.xMax - 9f, 0f, 1f, Screen.height);
        DrawRect(thinEdgeGlow, new Color(0.24f, 0.84f, 0.95f, 0.28f));
    }

    private void DrawMenuContent(Rect panelRect)
    {
        float leftPadding = 58f;
        float contentWidth = panelRect.width - leftPadding - 42f;

        float titleY = Mathf.Max(70f, Screen.height * 0.16f);

        DrawShadowLabel(
            new Rect(leftPadding, titleY, contentWidth, 76f),
            gameTitle,
            titleStyle,
            3f);

        GUI.Label(
            new Rect(leftPadding + 2f, titleY + 76f, contentWidth, 52f),
            tagline,
            taglineStyle);

        float lineY = titleY + 145f;
        DrawRect(new Rect(leftPadding, lineY, contentWidth * 0.72f, 2f), new Color(0.24f, 0.84f, 0.95f, 0.9f));
        DrawRect(new Rect(leftPadding, lineY + 7f, contentWidth * 0.38f, 1f), new Color(1f, 0.86f, 0.34f, 0.85f));

        float buttonY = lineY + 58f;
        float buttonWidth = Mathf.Min(310f, contentWidth);
        float buttonHeight = 48f;
        float gap = 16f;

        if (DrawMenuButton(new Rect(leftPadding, buttonY, buttonWidth, buttonHeight), "BEGIN"))
        {
            StartGame();
        }

        buttonY += buttonHeight + gap;

        if (DrawMenuButton(new Rect(leftPadding, buttonY, buttonWidth, buttonHeight), "QUIT"))
        {
            QuitGame();
        }

    }

    private bool DrawMenuButton(Rect rect, string label)
    {
        Event current = Event.current;
        bool hovering = rect.Contains(current.mousePosition);

        Color fillColor = hovering
            ? new Color(0.08f, 0.22f, 0.28f, 0.92f)
            : new Color(0.02f, 0.06f, 0.09f, 0.82f);

        Color borderColor = hovering
            ? new Color(0.55f, 0.95f, 1f, 1f)
            : new Color(0.24f, 0.84f, 0.95f, 0.62f);

        DrawRect(rect, fillColor);
        DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), borderColor);
        DrawRectOutline(rect, borderColor, 1f);

        return GUI.Button(rect, label, buttonStyle);
    }

    private void DrawShadowLabel(Rect rect, string text, GUIStyle style, float shadowOffset)
    {
        Color originalColor = style.normal.textColor;

        style.normal.textColor = new Color(0f, 0f, 0f, 0.78f);
        GUI.Label(new Rect(rect.x + shadowOffset, rect.y + shadowOffset, rect.width, rect.height), text, style);

        style.normal.textColor = originalColor;
        GUI.Label(rect, text, style);
    }

    private void OpenMenu()
    {
        menuOpen = true;
        IsOpen = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        FreezeGameplay();
    }

    private void StartGame()
    {
        menuOpen = false;
        IsOpen = false;

        RestoreGameplay();
    }

    private void FreezeGameplay()
    {
        disabledBehaviours.Clear();

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];

            if (behaviour == null)
            {
                continue;
            }

            if (behaviour == this)
            {
                continue;
            }

            if (!behaviour.enabled)
            {
                continue;
            }

            behaviour.enabled = false;
            disabledBehaviours.Add(behaviour);
        }
    }

    private void RestoreGameplay()
    {
        for (int i = 0; i < disabledBehaviours.Count; i++)
        {
            MonoBehaviour behaviour = disabledBehaviours[i];

            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        disabledBehaviours.Clear();

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        IsOpen = false;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DrawFallbackStars()
    {
        float starSize = 2f;
        for (int i = 0; i < 100; i++)
        {
            float x = Mathf.Repeat(i * 97.31f, Screen.width);
            float y = Mathf.Repeat(i * 53.73f, Screen.height);
            float alpha = 0.18f + Mathf.Repeat(i * 0.137f, 0.55f);

            DrawRect(
                new Rect(x, y, starSize, starSize),
                new Color(0.8f, 0.95f, 1f, alpha));
        }
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    private void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}