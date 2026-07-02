using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    public ResourceInventory inventory;
    public TMP_Text oreText;

    private void Update()
    {
        if (inventory == null || oreText == null)
        {
            return;
        }

        oreText.text =
            "Ore: " + inventory.GetAmount(ResourceType.Ore) +
            "  Ice: " + inventory.GetAmount(ResourceType.Ice) +
            "  Silicate: " + inventory.GetAmount(ResourceType.Silicate) +
            "  Copper: " + inventory.GetAmount(ResourceType.Copper) +
            "  Biomass: " + inventory.GetAmount(ResourceType.Biomass);
    }
}
