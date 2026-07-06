using UnityEngine;

public class PlanetUpgradeBuilding : PlacedBuilding
{
    public UpgradeId upgradeId;

    public void Initialize(BuildingType type, UpgradeId id, Transform planetTransform, Vector2 surfaceNormal)
    {
        buildingType = type;
        buildingTier = BuildingTier.Tier1;
        upgradeId = id;

        PlanetSurfaceAnchor anchor = GetComponent<PlanetSurfaceAnchor>();
        if (anchor != null)
        {
            anchor.Bind(planetTransform, surfaceNormal);
            anchor.SnapToSurface();
        }
    }
}
