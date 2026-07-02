using UnityEngine;

public class LogisticsNode : MonoBehaviour
{
    public LogisticsNodeType nodeType = LogisticsNodeType.Extractor;

    [SerializeField]
    private Transform assignedPlanet;

    [SerializeField]
    private PlanetLogisticsNetwork network;

    [SerializeField]
    private PlanetResourceExtractorBuilding extractor;

    [SerializeField]
    private CollectorHub cargoHub;

    private Collider2D primaryCollider;

    public Transform AssignedPlanet => assignedPlanet;
    public PlanetLogisticsNetwork Network => network;
    public PlanetResourceExtractorBuilding Extractor => extractor;
    public CollectorHub CargoHub => cargoHub;
    public Collider2D PrimaryCollider => primaryCollider;

    private void Awake()
    {
        RefreshReferences();
    }

    private void OnEnable()
    {
        RefreshAssignment();
    }

    private void OnDisable()
    {
        Unregister();
    }

    public void ConfigureExtractor(PlanetResourceExtractorBuilding building)
    {
        extractor = building;
        cargoHub = null;
        nodeType = LogisticsNodeType.Extractor;
        RefreshReferences();
        RefreshAssignment();
    }

    public void ConfigureCargo(CollectorHub hub)
    {
        cargoHub = hub;
        extractor = null;
        nodeType = LogisticsNodeType.Cargo;
        RefreshReferences();
        RefreshAssignment();
    }

    public void RefreshAssignment()
    {
        RefreshReferences();
        PlanetLogisticsNetwork nextNetwork = ResolveNetwork();
        if (nextNetwork == network)
        {
            if (network != null)
            {
                network.MarkDirty();
            }

            return;
        }

        Unregister();
        network = nextNetwork;
        assignedPlanet = network == null ? null : network.transform;

        if (network != null)
        {
            network.Register(this);
        }
    }

    public static LogisticsNode EnsureForExtractor(PlanetResourceExtractorBuilding building)
    {
        if (building == null)
        {
            return null;
        }

        LogisticsNode node = building.GetComponent<LogisticsNode>();
        if (node == null)
        {
            node = building.gameObject.AddComponent<LogisticsNode>();
        }

        node.ConfigureExtractor(building);
        return node;
    }

    public static LogisticsNode EnsureForCargo(CollectorHub hub)
    {
        if (hub == null)
        {
            return null;
        }

        LogisticsNode node = hub.GetComponent<LogisticsNode>();
        if (node == null)
        {
            node = hub.gameObject.AddComponent<LogisticsNode>();
        }

        node.ConfigureCargo(hub);
        return node;
    }

    private void RefreshReferences()
    {
        if (extractor == null)
        {
            extractor = GetComponent<PlanetResourceExtractorBuilding>();
        }

        if (cargoHub == null)
        {
            cargoHub = GetComponent<CollectorHub>();
        }

        primaryCollider = GetComponent<Collider2D>();
    }

    private PlanetLogisticsNetwork ResolveNetwork()
    {
        if (nodeType == LogisticsNodeType.Extractor && extractor != null && extractor.SourceDeposit != null)
        {
            PlanetLogisticsNetwork anchoredNetwork = extractor.SourceDeposit.GetComponent<PlanetLogisticsNetwork>();
            if (anchoredNetwork != null)
            {
                return anchoredNetwork;
            }
        }

        return PlanetLogisticsNetwork.FindNearest(transform.position);
    }

    private void Unregister()
    {
        if (network != null)
        {
            network.Unregister(this);
            network = null;
            assignedPlanet = null;
        }
    }
}
