using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    private readonly HashSet<string> flags = new HashSet<string>();
    private readonly List<DialogueChoice> visibleChoices = new List<DialogueChoice>();

    private DialogueDefinition activeDefinition;
    private DialogueNode activeNode;
    private GUIStyle speakerStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;
    private float styleScale;
    private Vector2 textScrollPosition;
    private Vector2 choiceScrollPosition;

    public bool IsDialogueOpen => activeDefinition != null && activeNode != null;
    public string CurrentNodeId => activeNode == null ? null : activeNode.id;
    public IReadOnlyList<DialogueChoice> VisibleChoices => visibleChoices;

    public bool StartDialogue(string dialogueId)
    {
        if (!DialogueLibrary.TryGet(dialogueId, out DialogueDefinition definition))
        {
            Debug.LogWarning("Dialogue not found: " + dialogueId);
            return false;
        }

        return StartDialogue(definition);
    }

    public bool StartDialogue(DialogueDefinition definition)
    {
        if (definition == null)
        {
            CloseDialogue();
            return false;
        }

        DialogueNode startNode = definition.FindNode(definition.startNodeId);
        if (startNode == null)
        {
            Debug.LogWarning("Dialogue " + definition.id + " has no start node: " + definition.startNodeId);
            CloseDialogue();
            return false;
        }

        activeDefinition = definition;
        SetActiveNode(startNode);
        return true;
    }

    public bool HasFlag(string flag)
    {
        return !string.IsNullOrEmpty(flag) && flags.Contains(flag);
    }

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
        {
            flags.Add(flag);
            RefreshVisibleChoices();
        }
    }

    public void Continue()
    {
        if (!IsDialogueOpen || visibleChoices.Count > 0)
        {
            return;
        }

        GoToNode(activeNode.nextNodeId);
    }

    public void SelectChoice(int visibleChoiceIndex)
    {
        if (!IsDialogueOpen || visibleChoiceIndex < 0 || visibleChoiceIndex >= visibleChoices.Count)
        {
            return;
        }

        DialogueChoice choice = visibleChoices[visibleChoiceIndex];
        if (choice.flagsToSet != null)
        {
            for (int i = 0; i < choice.flagsToSet.Count; i++)
            {
                SetFlag(choice.flagsToSet[i]);
            }
        }

        GoToNode(choice.nextNodeId);
    }

    public void CloseDialogue()
    {
        activeDefinition = null;
        activeNode = null;
        visibleChoices.Clear();
    }

    private void OnGUI()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        EnsureStyles();
        HandleKeyboardInput(Event.current);
        if (!IsDialogueOpen)
        {
            return;
        }

        float scale = GameUiScale.Current;
        float margin = Mathf.Clamp(Screen.width * 0.035f, GameUiScale.Size(12f, scale), GameUiScale.Size(32f, scale));
        float resourceBarTop = Screen.height - GameUiScale.Size(32f, scale) - GameUiScale.Size(14f, scale);
        float bottomGap = GameUiScale.Size(10f, scale);
        float availableHeight = Mathf.Max(GameUiScale.Size(96f, scale), resourceBarTop - bottomGap - margin);
        float panelHeight = Mathf.Clamp(Screen.height * 0.26f, GameUiScale.Size(96f, scale), availableHeight);
        Rect panelRect = new Rect(margin, resourceBarTop - bottomGap - panelHeight, Screen.width - (margin * 2f), panelHeight);
        PixelUiSprites.Draw(panelRect, PixelUiFrame.Panel);

        float inset = GameUiScale.Size(14f, scale);
        float portraitSize = Mathf.Min(GameUiScale.Size(104f, scale), panelRect.height - (inset * 2f));
        Rect portraitRect = new Rect(panelRect.x + inset, panelRect.y + inset, portraitSize, portraitSize);
        GUI.DrawTexture(portraitRect, DialoguePortraits.GetPortrait(activeNode.portraitId), ScaleMode.ScaleToFit, true);

        float textX = portraitRect.xMax + GameUiScale.Size(18f, scale);
        Rect speakerRect = new Rect(textX, panelRect.y + GameUiScale.Size(18f, scale), panelRect.xMax - textX - inset, GameUiScale.Size(34f, scale));
        GUI.Label(speakerRect, string.IsNullOrEmpty(activeNode.speakerName) ? "Unknown" : activeNode.speakerName, speakerStyle);

        float choiceRowHeight = GameUiScale.Size(38f, scale);
        float choiceAreaHeight = visibleChoices.Count > 0 ? Mathf.Min(GameUiScale.Size(146f, scale), choiceRowHeight * visibleChoices.Count) : GameUiScale.Size(46f, scale);
        Rect textRect = new Rect(textX, speakerRect.yMax + GameUiScale.Size(4f, scale), panelRect.xMax - textX - inset, panelRect.height - GameUiScale.Size(58f, scale) - choiceAreaHeight);
        DrawDialogueText(textRect, scale);

        if (visibleChoices.Count > 0)
        {
            DrawChoices(new Rect(textX, panelRect.yMax - choiceAreaHeight - inset + GameUiScale.Size(6f, scale), panelRect.xMax - textX - inset, choiceAreaHeight), scale);
        }
        else
        {
            Rect continueRect = new Rect(panelRect.xMax - GameUiScale.Size(196f, scale), panelRect.yMax - GameUiScale.Size(56f, scale), GameUiScale.Size(170f, scale), GameUiScale.Size(38f, scale));
            string buttonLabel = string.IsNullOrEmpty(activeNode.nextNodeId) ? "Enter: Close" : "Enter: Continue";
            if (GUI.Button(continueRect, buttonLabel, buttonStyle))
            {
                Continue();
                Event.current.Use();
            }
        }
    }

    private void DrawDialogueText(Rect area, float scale)
    {
        float viewWidth = Mathf.Max(1f, area.width - GameUiScale.Size(18f, scale));
        float textHeight = Mathf.Max(area.height, bodyStyle.CalcHeight(new GUIContent(activeNode.text), viewWidth));
        Rect viewRect = new Rect(0f, 0f, viewWidth, textHeight);

        textScrollPosition = GUI.BeginScrollView(area, textScrollPosition, viewRect);
        GUI.Label(viewRect, activeNode.text, bodyStyle);
        GUI.EndScrollView();
    }

    private void DrawChoices(Rect area, float scale)
    {
        float rowHeight = GameUiScale.Size(38f, scale);
        float buttonHeight = GameUiScale.Size(34f, scale);
        Rect viewRect = new Rect(0f, 0f, Mathf.Max(1f, area.width - GameUiScale.Size(18f, scale)), visibleChoices.Count * rowHeight);
        choiceScrollPosition = GUI.BeginScrollView(area, choiceScrollPosition, viewRect);

        for (int i = 0; i < visibleChoices.Count && i < 4; i++)
        {
            Rect buttonRect = new Rect(0f, i * rowHeight, viewRect.width, buttonHeight);
            string label = (i + 1) + ". " + visibleChoices[i].label;
            if (GUI.Button(buttonRect, label, buttonStyle))
            {
                SelectChoice(i);
                Event.current.Use();
            }
        }

        GUI.EndScrollView();
    }

    private void HandleKeyboardInput(Event currentEvent)
    {
        if (currentEvent == null || currentEvent.type != EventType.KeyDown)
        {
            return;
        }

        if (visibleChoices.Count > 0)
        {
            int selectedIndex = KeyCodeToChoiceIndex(currentEvent.keyCode);
            if (selectedIndex >= 0 && selectedIndex < visibleChoices.Count && selectedIndex < 4)
            {
                SelectChoice(selectedIndex);
                currentEvent.Use();
            }

            return;
        }

        if (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter)
        {
            Continue();
            currentEvent.Use();
        }
    }

    private static int KeyCodeToChoiceIndex(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Alpha1:
            case KeyCode.Keypad1:
                return 0;
            case KeyCode.Alpha2:
            case KeyCode.Keypad2:
                return 1;
            case KeyCode.Alpha3:
            case KeyCode.Keypad3:
                return 2;
            case KeyCode.Alpha4:
            case KeyCode.Keypad4:
                return 3;
            default:
                return -1;
        }
    }

    private void GoToNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            CloseDialogue();
            return;
        }

        DialogueNode nextNode = activeDefinition == null ? null : activeDefinition.FindNode(nodeId);
        if (nextNode == null)
        {
            Debug.LogWarning("Dialogue node not found: " + nodeId);
            CloseDialogue();
            return;
        }

        SetActiveNode(nextNode);
    }

    private void SetActiveNode(DialogueNode node)
    {
        activeNode = node;
        textScrollPosition = Vector2.zero;
        choiceScrollPosition = Vector2.zero;
        RefreshVisibleChoices();
    }

    private void RefreshVisibleChoices()
    {
        visibleChoices.Clear();

        if (activeNode == null || activeNode.choices == null)
        {
            return;
        }

        for (int i = 0; i < activeNode.choices.Count && visibleChoices.Count < 4; i++)
        {
            DialogueChoice choice = activeNode.choices[i];
            if (choice != null && AreRequiredFlagsMet(choice))
            {
                visibleChoices.Add(choice);
            }
        }
    }

    private bool AreRequiredFlagsMet(DialogueChoice choice)
    {
        if (choice.requiredFlags == null)
        {
            return true;
        }

        for (int i = 0; i < choice.requiredFlags.Count; i++)
        {
            string requiredFlag = choice.requiredFlags[i];
            if (!string.IsNullOrEmpty(requiredFlag) && !flags.Contains(requiredFlag))
            {
                return false;
            }
        }

        return true;
    }

    private void EnsureStyles()
    {
        float scale = GameUiScale.Current;
        if (speakerStyle != null && Mathf.Approximately(styleScale, scale))
        {
            return;
        }

        styleScale = scale;
        speakerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(26f, scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = PixelUiSprites.Gold }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = GameUiScale.Font(22f, scale),
            wordWrap = true,
            richText = false,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = GameUiScale.Font(20f, scale),
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(GameUiScale.Font(14f, scale), GameUiScale.Font(14f, scale), GameUiScale.Font(5f, scale), GameUiScale.Font(5f, scale)),
            normal =
            {
                background = PixelUiSprites.TextureFor(PixelUiFrame.Button),
                textColor = Color.white
            },
            hover =
            {
                background = PixelUiSprites.TextureFor(PixelUiFrame.ButtonHover),
                textColor = PixelUiSprites.Gold
            },
            active =
            {
                background = PixelUiSprites.TextureFor(PixelUiFrame.ButtonHover),
                textColor = Color.white
            }
        };
    }
}
