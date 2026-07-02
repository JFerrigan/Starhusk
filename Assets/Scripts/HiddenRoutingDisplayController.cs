using UnityEngine;
using UnityEngine.InputSystem;

public class HiddenRoutingDisplayController : MonoBehaviour
{
    public Key toggleKey = Key.T;

    public static bool RoutesVisible { get; private set; }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            RoutesVisible = !RoutesVisible;
        }
    }

    public static void SetRoutesVisible(bool visible)
    {
        RoutesVisible = visible;
    }
}
