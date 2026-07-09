using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeState : MonoBehaviour
{
    private static readonly UpgradeId[] OrderedUpgradeIds =
    {
        UpgradeId.Ping3Asteroids,
        UpgradeId.PingAsteroidResourceType,
        UpgradeId.Ping10Asteroids,
        UpgradeId.TripleShotProjectiles,
        UpgradeId.HomingProjectiles,
        UpgradeId.AsteroidAnnihilator,
        UpgradeId.AutopilotUnlock,
        UpgradeId.ImpactShield,
        UpgradeId.AsteroidCarverHull,
        UpgradeId.InfiniteRadarRange,
        UpgradeId.PersistentRadarDiscovery
    };

    private readonly HashSet<UpgradeId> unlockedUpgrades = new HashSet<UpgradeId>();

    public static System.Collections.Generic.IReadOnlyList<UpgradeId> AllUpgradeIds => OrderedUpgradeIds;

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

    public void SetUnlocked(UpgradeId upgradeId, bool unlocked)
    {
        if (unlocked)
        {
            unlockedUpgrades.Add(upgradeId);
        }
        else
        {
            unlockedUpgrades.Remove(upgradeId);
        }
    }

    public void Clear()
    {
        unlockedUpgrades.Clear();
    }
}
