using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeState : MonoBehaviour
{
    private readonly HashSet<UpgradeId> unlockedUpgrades = new HashSet<UpgradeId>();

    public static PlayerUpgradeState Current
    {
        get
        {
            PlayerUpgradeState state = FindFirstObjectByType<PlayerUpgradeState>();
            if (state != null)
            {
                return state;
            }

            ResourceInventory inventory = FindFirstObjectByType<ResourceInventory>();
            if (inventory != null)
            {
                return inventory.gameObject.AddComponent<PlayerUpgradeState>();
            }

            return null;
        }
    }

    public bool IsUnlocked(UpgradeId upgradeId)
    {
        return unlockedUpgrades.Contains(upgradeId);
    }

    public bool Unlock(UpgradeId upgradeId)
    {
        return unlockedUpgrades.Add(upgradeId);
    }

    public void Clear()
    {
        unlockedUpgrades.Clear();
    }
}
