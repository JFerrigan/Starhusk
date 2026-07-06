using System;
using System.Collections.Generic;

[Serializable]
public class DialogueDefinition
{
    public string id;
    public string startNodeId;
    public List<DialogueNode> nodes = new List<DialogueNode>();

    public DialogueDefinition()
    {
    }

    public DialogueDefinition(string id, string startNodeId, IEnumerable<DialogueNode> nodes)
    {
        this.id = id;
        this.startNodeId = startNodeId;
        this.nodes = nodes == null ? new List<DialogueNode>() : new List<DialogueNode>(nodes);
    }

    public DialogueNode FindNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId) || nodes == null)
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            DialogueNode node = nodes[i];
            if (node != null && node.id == nodeId)
            {
                return node;
            }
        }

        return null;
    }
}

[Serializable]
public class DialogueNode
{
    public string id;
    public string speakerId;
    public string speakerName;
    public string portraitId;
    public string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public string nextNodeId;

    public DialogueNode()
    {
    }

    public DialogueNode(string id, string speakerId, string speakerName, string portraitId, string text, string nextNodeId = null, IEnumerable<DialogueChoice> choices = null)
    {
        this.id = id;
        this.speakerId = speakerId;
        this.speakerName = speakerName;
        this.portraitId = portraitId;
        this.text = text;
        this.nextNodeId = nextNodeId;
        this.choices = choices == null ? new List<DialogueChoice>() : new List<DialogueChoice>(choices);
    }
}

[Serializable]
public class DialogueChoice
{
    public string label;
    public string nextNodeId;
    public List<string> requiredFlags = new List<string>();
    public List<string> flagsToSet = new List<string>();

    public DialogueChoice()
    {
    }

    public DialogueChoice(string label, string nextNodeId, IEnumerable<string> requiredFlags = null, IEnumerable<string> flagsToSet = null)
    {
        this.label = label;
        this.nextNodeId = nextNodeId;
        this.requiredFlags = requiredFlags == null ? new List<string>() : new List<string>(requiredFlags);
        this.flagsToSet = flagsToSet == null ? new List<string>() : new List<string>(flagsToSet);
    }
}
