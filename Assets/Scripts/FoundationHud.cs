using UnityEngine;

public class FoundationHud : MonoBehaviour
{
    private ResourceInventory inventory;
    private PlayerMovement playerMovement;
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

        if (playerMovement == null && inventory != null)
        {
            playerMovement = inventory.GetComponent<PlayerMovement>();
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
        float speed = playerMovement == null ? 0f : playerMovement.Speed;

        GUI.Label(
            new Rect(12f, 12f, 560f, 100f),
            "Seed " + seed +
            "\nOre " + inventory.GetAmount(ResourceType.Ore) +
            "  Ice " + inventory.GetAmount(ResourceType.Ice) +
            "  Silicate " + inventory.GetAmount(ResourceType.Silicate) +
            "  Copper " + inventory.GetAmount(ResourceType.Copper) +
            "\nScanner " + scannerState +
            "  Speed " + speed.ToString("0.0"),
            labelStyle
        );
    }
}
