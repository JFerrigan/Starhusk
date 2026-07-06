#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class DialogueSystemTests
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
    public void IntroGateStartsConfiguredIntroOnlyWhenRequired()
    {
        RunBootstrap(GameModeCatalog.Create(GameModeId.Dev));

        DialogueController devController = Object.FindFirstObjectByType<DialogueController>();
        IntroDialogueGate devGate = Object.FindFirstObjectByType<IntroDialogueGate>();

        Assert.IsNotNull(devController);
        Assert.IsNotNull(devGate);
        Assert.IsFalse(devGate.requireIntroDialogue);
        Assert.IsFalse(devController.IsDialogueOpen);

        CleanupSceneObjects();

        RunBootstrap(GameModeCatalog.Create(GameModeId.Real));

        DialogueController realController = Object.FindFirstObjectByType<DialogueController>();
        IntroDialogueGate realGate = Object.FindFirstObjectByType<IntroDialogueGate>();

        Assert.IsNotNull(realController);
        Assert.IsNotNull(realGate);
        Assert.IsTrue(realGate.requireIntroDialogue);
        Assert.IsTrue(realController.IsDialogueOpen);
        Assert.That(realController.CurrentNodeId, Is.EqualTo("welcome"));
    }

    [Test]
    public void ControllerStartsAdvancesAndClosesLinearDialogue()
    {
        DialogueController controller = CreateController();
        DialogueDefinition definition = DialogueLibrary.CreateForTests(
            "linear",
            "one",
            new[]
            {
                new DialogueNode("one", "speaker", "Speaker", "unknown", "First.", "two"),
                new DialogueNode("two", "speaker", "Speaker", "unknown", "Second.")
            });

        Assert.IsTrue(controller.StartDialogue(definition));
        Assert.IsTrue(controller.IsDialogueOpen);
        Assert.That(controller.CurrentNodeId, Is.EqualTo("one"));

        controller.Continue();

        Assert.That(controller.CurrentNodeId, Is.EqualTo("two"));

        controller.Continue();

        Assert.IsFalse(controller.IsDialogueOpen);
    }

    [Test]
    public void ChoiceSelectionJumpsToExpectedNode()
    {
        DialogueController controller = CreateController();
        DialogueDefinition definition = DialogueLibrary.CreateForTests(
            "choice",
            "root",
            new[]
            {
                new DialogueNode(
                    "root",
                    "speaker",
                    "Speaker",
                    "unknown",
                    "Choose.",
                    choices: new[]
                    {
                        new DialogueChoice("Left", "left"),
                        new DialogueChoice("Right", "right")
                    }),
                new DialogueNode("left", "speaker", "Speaker", "unknown", "Left."),
                new DialogueNode("right", "speaker", "Speaker", "unknown", "Right.")
            });

        controller.StartDialogue(definition);
        controller.SelectChoice(1);

        Assert.That(controller.CurrentNodeId, Is.EqualTo("right"));
    }

    [Test]
    public void ChoiceFlagsAreStoredAndControlVisibleChoices()
    {
        DialogueController controller = CreateController();
        DialogueDefinition definition = DialogueLibrary.CreateForTests(
            "flags",
            "root",
            new[]
            {
                new DialogueNode(
                    "root",
                    "speaker",
                    "Speaker",
                    "unknown",
                    "Unlock?",
                    choices: new[]
                    {
                        new DialogueChoice("Unlock", "root", flagsToSet: new[] { "knows_path" }),
                        new DialogueChoice("Secret", "secret", requiredFlags: new[] { "knows_path" })
                    }),
                new DialogueNode("secret", "speaker", "Speaker", "unknown", "Secret.")
            });

        controller.StartDialogue(definition);

        Assert.That(controller.VisibleChoices.Count, Is.EqualTo(1));
        Assert.That(controller.VisibleChoices[0].label, Is.EqualTo("Unlock"));

        controller.SelectChoice(0);

        Assert.IsTrue(controller.HasFlag("knows_path"));
        Assert.That(controller.CurrentNodeId, Is.EqualTo("root"));
        Assert.That(controller.VisibleChoices.Count, Is.EqualTo(2));

        controller.SelectChoice(1);

        Assert.That(controller.CurrentNodeId, Is.EqualTo("secret"));
    }

    [Test]
    public void GeneratedPortraitTexturesUsePointFiltering()
    {
        Texture2D council = DialoguePortraits.GetPortrait("council");
        Texture2D unknown = DialoguePortraits.GetPortrait("missing");

        Assert.IsNotNull(council);
        Assert.IsNotNull(unknown);
        Assert.That(council.filterMode, Is.EqualTo(FilterMode.Point));
        Assert.That(unknown.filterMode, Is.EqualTo(FilterMode.Point));
    }

    private static DialogueController CreateController()
    {
        return new GameObject("DialogueController").AddComponent<DialogueController>();
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
