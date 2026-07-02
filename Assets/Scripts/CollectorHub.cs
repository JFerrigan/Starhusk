using System.Collections.Generic;
using UnityEngine;

public class CollectorHub : MonoBehaviour
{
    public const int DefaultCapacity = 5000;

    private static readonly List<CollectorHub> activeHubs = new List<CollectorHub>();

    [SerializeField]
    private ResourceStorage storage;

    public ResourceStorage Storage => storage;
    public static IReadOnlyList<CollectorHub> ActiveHubs => activeHubs;

    private void Awake()
    {
        storage = GetComponent<ResourceStorage>();
        if (storage == null)
        {
            storage = gameObject.AddComponent<ResourceStorage>();
        }

        storage.Configure(DefaultCapacity);
        LogisticsNode.EnsureForCargo(this);
    }

    private void OnEnable()
    {
        if (!activeHubs.Contains(this))
        {
            activeHubs.Add(this);
        }
    }

    private void OnDisable()
    {
        activeHubs.Remove(this);
    }
}
