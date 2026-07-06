using UnityEngine;

public class IntroDialogueGate : MonoBehaviour
{
    public bool requireIntroDialogue;

    public bool IsBlockingStartup => false;

    public void Configure(bool shouldRequireIntroDialogue)
    {
        requireIntroDialogue = shouldRequireIntroDialogue;
    }
}
