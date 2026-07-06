using UnityEngine;

public class IntroDialogueGate : MonoBehaviour
{
    public bool requireIntroDialogue;
    public string introDialogueId = DialogueLibrary.Act1IntroId;

    private bool hasStartedIntro;

    public bool IsBlockingStartup => false;

    public void Configure(bool shouldRequireIntroDialogue)
    {
        Configure(shouldRequireIntroDialogue, Object.FindFirstObjectByType<DialogueController>());
    }

    public void Configure(bool shouldRequireIntroDialogue, DialogueController dialogueController)
    {
        requireIntroDialogue = shouldRequireIntroDialogue;

        if (!requireIntroDialogue || hasStartedIntro || dialogueController == null)
        {
            return;
        }

        if (dialogueController.StartDialogue(introDialogueId))
        {
            hasStartedIntro = true;
        }
    }
}
