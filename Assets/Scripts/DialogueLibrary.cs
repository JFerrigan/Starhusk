using System.Collections.Generic;

public static class DialogueLibrary
{
    public const string Act1IntroId = "act1_intro";

    public static bool TryGet(string dialogueId, out DialogueDefinition definition)
    {
        if (dialogueId == Act1IntroId)
        {
            definition = CreateAct1Intro();
            return true;
        }

        definition = null;
        return false;
    }

    public static DialogueDefinition CreateAct1Intro()
    {
        const string councilId = "council";
        const string councilName = "The Council";
        const string councilPortrait = "council";

        return new DialogueDefinition(
            Act1IntroId,
            "welcome",
            new[]
            {
                new DialogueNode(
                    "welcome",
                    councilId,
                    councilName,
                    councilPortrait,
                    "Welcome, Architect. Your purpose is the expansion and preservation of the human race. We gave you life.",
                    "guidance"),
                new DialogueNode(
                    "guidance",
                    councilId,
                    councilName,
                    councilPortrait,
                    "We are the Council. We exist for your guidance. Seek us when the dark between stars becomes too wide.",
                    "movement"),
                new DialogueNode(
                    "movement",
                    councilId,
                    councilName,
                    councilPortrait,
                    "You are free to explore the universe at your will. Use the arrow keys to activate thrusters, and Spacebar to apply gravity brakes.",
                    "mining"),
                new DialogueNode(
                    "mining",
                    councilId,
                    councilName,
                    councilPortrait,
                    "Mine nearby asteroids with Left Click. Open your inventory with I when you need to inspect what you have gathered.",
                    "building"),
                new DialogueNode(
                    "building",
                    councilId,
                    councilName,
                    councilPortrait,
                    "Gather resources. Construct. Automate. Each structure should make the next step less fragile.",
                    "dyson"),
                new DialogueNode(
                    "dyson",
                    councilId,
                    councilName,
                    councilPortrait,
                    "Your first milestone is to create a Dyson Sphere. Begin small, then make the star itself answer.",
                    "good_luck"),
                new DialogueNode(
                    "good_luck",
                    councilId,
                    councilName,
                    councilPortrait,
                    "Good luck, Architect.")
            });
    }

    public static DialogueDefinition CreateForTests(string id, string startNodeId, IEnumerable<DialogueNode> nodes)
    {
        return new DialogueDefinition(id, startNodeId, nodes);
    }
}
