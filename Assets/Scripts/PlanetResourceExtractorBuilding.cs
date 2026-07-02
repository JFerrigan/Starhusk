using UnityEngine;

public abstract class PlanetResourceExtractorBuilding : PlacedBuilding
{
    private static readonly System.Collections.Generic.List<PlanetResourceExtractorBuilding> activeExtractors = new System.Collections.Generic.List<PlanetResourceExtractorBuilding>();

    [SerializeField]
    private ResourceDeposit sourceDeposit;

    [SerializeField]
    private PlanetSurfaceAnchor surfaceAnchor;

    [SerializeField]
    private BuildingStorage storage;

    [SerializeField]
    private string planetName;

    private float nextExtractionTime;

    public ResourceDeposit SourceDeposit => sourceDeposit;
    public BuildingStorage Storage => storage;
    public string PlanetName => planetName;
    public string DisplayName => BuildingCatalog.GetDisplayName(buildingType, buildingTier);
    public static System.Collections.Generic.IReadOnlyList<PlanetResourceExtractorBuilding> ActiveExtractors => activeExtractors;

    protected abstract BuildingType ExtractorType { get; }

    protected virtual BuildingDefinition Definition => BuildingCatalog.GetDefinition(ExtractorType);

    protected override void Awake()
    {
        base.Awake();
        buildingType = ExtractorType;
        buildingTier = BuildingTier.Tier1;

        EnsureReferences();

        if (storage != null)
        {
            storage.Configure(PreferredResourceType, Definition.capacity, storage.CurrentAmount);
        }
    }

    private void OnEnable()
    {
        if (!activeExtractors.Contains(this))
        {
            activeExtractors.Add(this);
        }
    }

    private void OnDisable()
    {
        activeExtractors.Remove(this);
    }

    private void Update()
    {
        if (sourceDeposit == null || storage == null || Time.time < nextExtractionTime)
        {
            return;
        }

        ExtractOnce();
        nextExtractionTime = Time.time + Definition.extractionInterval;
    }

    public void Initialize(ResourceDeposit deposit, Transform planetTransform, Vector2 surfaceNormal)
    {
        EnsureReferences();

        if (storage != null)
        {
            storage.Configure(PreferredResourceType, Definition.capacity, storage.CurrentAmount);
        }

        BindToPlanet(deposit, planetTransform, surfaceNormal);
        nextExtractionTime = Time.time + Definition.extractionInterval;
    }

    public bool CanRelocateTo(ResourceDeposit deposit)
    {
        if (!BuildingCatalog.IsValidPlacementTarget(ExtractorType, deposit))
        {
            return false;
        }

        if (storage == null || storage.IsEmpty)
        {
            return true;
        }

        return storage.ResourceType == deposit.resourceType;
    }

    public bool BindToPlanet(ResourceDeposit deposit, Transform planetTransform, Vector2 surfaceNormal)
    {
        if (deposit == null || planetTransform == null || storage == null || surfaceAnchor == null)
        {
            return false;
        }

        if (!CanRelocateTo(deposit))
        {
            return false;
        }

        sourceDeposit = deposit;
        planetName = planetTransform.name;

        if (storage.IsEmpty)
        {
            storage.SetResourceType(PreferredResourceType);
        }

        surfaceAnchor.Bind(planetTransform, surfaceNormal);
        surfaceAnchor.SnapToSurface();
        LogisticsNode.EnsureForExtractor(this);
        return true;
    }

    public int ExtractOnce()
    {
        if (sourceDeposit == null || storage == null || storage.IsFull || sourceDeposit.IsDepleted)
        {
            return 0;
        }

        if (!storage.CanStore(sourceDeposit.resourceType))
        {
            return 0;
        }

        int amountToExtract = Mathf.Min(Definition.extractAmountPerTick, sourceDeposit.remainingAmount);
        int storedAmount = storage.AddResource(sourceDeposit.resourceType, amountToExtract);

        if (storedAmount <= 0)
        {
            return 0;
        }

        sourceDeposit.remainingAmount -= storedAmount;
        return storedAmount;
    }

    private void EnsureReferences()
    {
        if (surfaceAnchor == null)
        {
            surfaceAnchor = GetComponent<PlanetSurfaceAnchor>();
        }

        if (storage == null)
        {
            storage = GetComponent<BuildingStorage>();
        }
    }

    private ResourceType PreferredResourceType
    {
        get
        {
            if (Definition.requiredResourceType.HasValue)
            {
                return Definition.requiredResourceType.Value;
            }

            return sourceDeposit != null ? sourceDeposit.resourceType : ResourceType.Ore;
        }
    }
}
