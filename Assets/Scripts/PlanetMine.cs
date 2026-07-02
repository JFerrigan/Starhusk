public class PlanetMine : PlanetResourceExtractorBuilding
{
    public const int Tier1Capacity = 250;
    public const int Tier1ExtractAmount = 5;
    public const float Tier1ExtractInterval = 1f;

    protected override BuildingType ExtractorType => BuildingType.Mine;
}
