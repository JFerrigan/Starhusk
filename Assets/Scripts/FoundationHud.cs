using UnityEngine;

public class FoundationHud : MonoBehaviour
{
    private ResourceInventory inventory;
    private PlayerScanner scanner;
    private StarSystemGenerator generator;
    private GUIStyle labelStyle;

    private void Awake()
    {
        labelStyle = new GUIStyle
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<ResourceInventory>();
        }

        if (scanner == null && inventory != null)
        {
            scanner = inventory.GetComponent<PlayerScanner>();
        }

        if (generator == null)
        {
            generator = FindFirstObjectByType<StarSystemGenerator>();
        }
    }

    private void OnGUI()
    {
        if (inventory == null)
        {
            return;
        }

        string scannerState = scanner == null || scanner.IsReady ? "Ready" : "Charging";
        int seed = generator == null ? 0 : generator.seed;

        GUI.Label(
            new Rect(12f, 12f, 560f, 80f),
            "Seed " + seed +
            "\nOre " + inventory.GetAmount(ResourceType.Ore) +
            "  Ice " + inventory.GetAmount(ResourceType.Ice) +
            "  Silicate " + inventory.GetAmount(ResourceType.Silicate) +
            "  Copper " + inventory.GetAmount(ResourceType.Copper) +
            "\nScanner " + scannerState,
            labelStyle
        );
    }
}
