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

        float margin = Mathf.Clamp(Screen.width * 0.04f, 18f, 52f);
        float panelHeight = Mathf.Clamp(Screen.height * 0.34f, 190f, 292f);
        Rect panelRect = new Rect(margin, Screen.height - panelHeight - margin, Screen.width - (margin * 2f), panelHeight);
        PixelUiSprites.Draw(panelRect, PixelUiFrame.Panel);

        float inset = 16f;
        Rect portraitRect = new Rect(panelRect.x + inset, panelRect.y + inset, Mathf.Min(104f, panelRect.height - (inset * 2f)), Mathf.Min(104f, panelRect.height - (inset * 2f)));
        GUI.DrawTexture(portraitRect, DialoguePortraits.GetPortrait(activeNode.portraitId), ScaleMode.ScaleToFit, true);

        float textX = portraitRect.xMax + 18f;
        Rect speakerRect = new Rect(textX, panelRect.y + 18f, panelRect.xMax - textX - inset, 34f);
        GUI.Label(speakerRect, string.IsNullOrEmpty(activeNode.speakerName) ? "Unknown" : activeNode.speakerName, speakerStyle);

        float choiceAreaHeight = visibleChoices.Count > 0 ? Mathf.Min(146f, 38f * visibleChoices.Count) : 46f;
        Rect textRect = new Rect(textX, speakerRect.yMax + 4f, panelRect.xMax - textX - inset, panelRect.height - 58f - choiceAreaHeight);
        GUI.Label(textRect, activeNode.text, bodyStyle);

        if (visibleChoices.Count > 0)
        {
            DrawChoices(new Rect(textX, panelRect.yMax - choiceAreaHeight - inset + 6f, panelRect.xMax - textX - inset, choiceAreaHeight));
        }
        else
        {
            Rect continueRect = new Rect(panelRect.xMax - 196f, panelRect.yMax - 56f, 170f, 38f);
            string buttonLabel = string.IsNullOrEmpty(activeNode.nextNodeId) ? "Enter: Close" : "Enter: Continue";
            if (GUI.Button(continueRect, buttonLabel, buttonStyle))
            {
                Continue();
                Event.current.Use();
            }
        }
    }

    private void DrawChoices(Rect area)
    {
        for (int i = 0; i < visibleChoices.Count && i < 4; i++)
        {
            Rect buttonRect = new Rect(area.x, area.y + (i * 38f), area.width, 34f);
            string label = (i + 1) + ". " + visibleChoices[i].label;
            if (GUI.Button(buttonRect, label, buttonStyle))
            {
                SelectChoice(i);
                Event.current.Use();
            }
        }
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
        if (speakerStyle != null)
        {
            return;
        }

        speakerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            normal = { textColor = PixelUiSprites.Gold }
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            wordWrap = true,
            richText = false,
            normal = { textColor = new Color(0.9f, 0.88f, 1f, 1f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(14, 14, 5, 5),
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
