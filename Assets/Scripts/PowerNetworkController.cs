using System.Collections.Generic;
using UnityEngine;

public class PowerNetworkController : MonoBehaviour
{
    public const int PowerPerActiveBeam = 100;

    public bool requirePower = false; // Keep off for now while prototyping.
public float rebuildInterval = 0.25f;
public float sourceRange = PowerRelay.DefaultRange;
public Color powerLinkColor = new Color(1f, 0.9f, 0.22f, 0.9f);
public Color consumerLinkColor = new Color(0.42f, 1f, 0.92f, 0.75f);
    private readonly List<PowerNode> nodes = new List<PowerNode>();
    private readonly List<PowerComponent> components = new List<PowerComponent>();
    private readonly List<PowerLink> visibleLinks = new List<PowerLink>();
    private float nextRebuildTime;

    public static PowerNetworkController Instance { get; private set; }
    public int TotalGeneration { get; private set; }
    public int TotalDemand { get; private set; }
    public int PoweredDemand { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RebuildNow();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Time.time >= nextRebuildTime)
        {
            RebuildNow();
        }

        DrawVisibleLinks();
    }

    public void RebuildNow()
    {
        nextRebuildTime = Time.time + Mathf.Max(0.05f, rebuildInterval);
        nodes.Clear();
        components.Clear();
        visibleLinks.Clear();
        TotalGeneration = 0;
        TotalDemand = 0;
        PoweredDemand = 0;

        BuildSourceNodes();
        BuildRelayNodes();
        BuildComponents();
        AssignRelayStates();
        AssignConsumers();
    }

    private void BuildSourceNodes()
    {
        IReadOnlyList<ActiveDysonBeam> alignments = DysonBeamNetwork.FindActiveAlignmentsInScene();
        for (int i = 0; i < alignments.Count; i++)
        {
            ActiveDysonBeam alignment = alignments[i];
            AddNode(new PowerNode(PowerNodeType.Source, alignment.ReceiverPosition, Mathf.Max(0f, sourceRange), null, PowerPerActiveBeam));
            TotalGeneration += PowerPerActiveBeam;
        }
    }

    private void BuildRelayNodes()
    {
        PowerRelay[] relays = FindObjectsByType<PowerRelay>(FindObjectsSortMode.None);
        System.Array.Sort(relays, CompareComponents);

        for (int i = 0; i < relays.Length; i++)
        {
            PowerRelay relay = relays[i];
            if (relay == null)
            {
                continue;
            }

            AddNode(new PowerNode(PowerNodeType.Relay, relay.transform.position, relay.Range, relay, 0));
        }
    }

    private void AddNode(PowerNode node)
    {
        node.index = nodes.Count;
        nodes.Add(node);
    }

    private void BuildComponents()
    {
        bool[] visited = new bool[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            if (visited[i] || nodes[i].generation <= 0 && nodes[i].type != PowerNodeType.Relay)
            {
                continue;
            }

            PowerComponent component = new PowerComponent();
            Queue<int> queue = new Queue<int>();
            visited[i] = true;
            queue.Enqueue(i);

            while (queue.Count > 0)
            {
                int nodeIndex = queue.Dequeue();
                PowerNode node = nodes[nodeIndex];
                component.nodeIndices.Add(nodeIndex);
                component.generation += node.generation;

                for (int otherIndex = 0; otherIndex < nodes.Count; otherIndex++)
                {
                    if (visited[otherIndex] || otherIndex == nodeIndex)
                    {
                        continue;
                    }

                    PowerNode other = nodes[otherIndex];
                    if (!NodesCanConnect(node, other))
                    {
                        continue;
                    }

                    visited[otherIndex] = true;
                    queue.Enqueue(otherIndex);
                    visibleLinks.Add(new PowerLink(node.position, other.position, powerLinkColor));
                }
            }

            if (component.generation > 0)
            {
                components.Add(component);
            }
        }
    }

    private static bool NodesCanConnect(PowerNode first, PowerNode second)
    {
        float range = Mathf.Min(first.range, second.range);
        if (range <= 0f)
        {
            return false;
        }

        return Vector2.SqrMagnitude(first.position - second.position) <= range * range;
    }

    private void AssignRelayStates()
    {
        PowerRelay[] relays = FindObjectsByType<PowerRelay>(FindObjectsSortMode.None);
        for (int i = 0; i < relays.Length; i++)
        {
            if (relays[i] != null)
            {
                relays[i].SetConnected(false);
            }
        }

        for (int componentIndex = 0; componentIndex < components.Count; componentIndex++)
        {
            PowerComponent component = components[componentIndex];
            for (int i = 0; i < component.nodeIndices.Count; i++)
            {
                PowerNode node = nodes[component.nodeIndices[i]];
                if (node.relay != null)
                {
                    node.relay.SetConnected(true);
                }
            }
        }
    }

   private void AssignConsumers()
{
    List<PowerConsumerEntry> consumers = FindConsumers();

    for (int i = 0; i < consumers.Count; i++)
    {
        int demand = Mathf.Max(0, consumers[i].consumer.PowerDemand);
        TotalDemand += demand;

        if (!requirePower)
        {
            consumers[i].consumer.SetPowered(true);
            PoweredDemand += demand;
        }
        else
        {
            consumers[i].consumer.SetPowered(false);
        }
    }

    if (!requirePower)
    {
        return;
    }

    consumers.Sort(CompareConsumerEntries);

    for (int componentIndex = 0; componentIndex < components.Count; componentIndex++)
    {
        PowerComponent component = components[componentIndex];
        int remainingPower = component.generation;

        for (int consumerIndex = 0; consumerIndex < consumers.Count; consumerIndex++)
        {
            PowerConsumerEntry entry = consumers[consumerIndex];
            if (entry.assigned || !IsConsumerInComponentRange(entry.position, component, out Vector2 linkStart))
            {
                continue;
            }

            int demand = Mathf.Max(0, entry.consumer.PowerDemand);
            if (demand > remainingPower)
            {
                continue;
            }

            remainingPower -= demand;
            PoweredDemand += demand;
            entry.consumer.SetPowered(true);
            entry.assigned = true;
            consumers[consumerIndex] = entry;
            visibleLinks.Add(new PowerLink(linkStart, entry.position, consumerLinkColor));
        }
    }
}

    private List<PowerConsumerEntry> FindConsumers()
    {
        List<PowerConsumerEntry> consumers = new List<PowerConsumerEntry>();
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour is IPowerConsumer consumer)
            {
                consumers.Add(new PowerConsumerEntry(consumer, behaviour, behaviour.transform.position));
            }
        }

        return consumers;
    }

    private bool IsConsumerInComponentRange(Vector2 consumerPosition, PowerComponent component, out Vector2 linkStart)
    {
        linkStart = Vector2.zero;
        float bestDistanceSquared = float.MaxValue;
        bool found = false;

        for (int i = 0; i < component.nodeIndices.Count; i++)
        {
            PowerNode node = nodes[component.nodeIndices[i]];
            float range = Mathf.Max(0f, node.range);
            float distanceSquared = Vector2.SqrMagnitude(consumerPosition - node.position);
            if (distanceSquared > range * range || distanceSquared >= bestDistanceSquared)
            {
                continue;
            }

            bestDistanceSquared = distanceSquared;
            linkStart = node.position;
            found = true;
        }

        return found;
    }

    private void DrawVisibleLinks()
    {
        if (!HiddenRoutingDisplayController.RoutesVisible)
        {
            return;
        }

        for (int i = 0; i < visibleLinks.Count; i++)
        {
            PowerLink link = visibleLinks[i];
            Debug.DrawLine(link.start, link.end, link.color);
        }
    }

    private static int CompareConsumerEntries(PowerConsumerEntry first, PowerConsumerEntry second)
    {
        return CompareComponents(first.behaviour, second.behaviour);
    }

    private static int CompareComponents(Component first, Component second)
    {
        string firstName = StableName(first);
        string secondName = StableName(second);
        int nameCompare = string.CompareOrdinal(firstName, secondName);
        if (nameCompare != 0)
        {
            return nameCompare;
        }

        int firstId = first == null ? 0 : first.GetInstanceID();
        int secondId = second == null ? 0 : second.GetInstanceID();
        return firstId.CompareTo(secondId);
    }

    private static string StableName(Component component)
    {
        if (component == null)
        {
            return string.Empty;
        }

        ObjectIdentity identity = component.GetComponent<ObjectIdentity>();
        return identity == null ? component.name : identity.HoverName;
    }

    private enum PowerNodeType
    {
        Source,
        Relay
    }

    private struct PowerNode
    {
        public int index;
        public PowerNodeType type;
        public Vector2 position;
        public float range;
        public PowerRelay relay;
        public int generation;

        public PowerNode(PowerNodeType type, Vector2 position, float range, PowerRelay relay, int generation)
        {
            index = -1;
            this.type = type;
            this.position = position;
            this.range = range;
            this.relay = relay;
            this.generation = generation;
        }
    }

    private class PowerComponent
    {
        public readonly List<int> nodeIndices = new List<int>();
        public int generation;
    }

    private struct PowerConsumerEntry
    {
        public IPowerConsumer consumer;
        public MonoBehaviour behaviour;
        public Vector2 position;
        public bool assigned;

        public PowerConsumerEntry(IPowerConsumer consumer, MonoBehaviour behaviour, Vector2 position)
        {
            this.consumer = consumer;
            this.behaviour = behaviour;
            this.position = position;
            assigned = false;
        }
    }

    private struct PowerLink
    {
        public Vector2 start;
        public Vector2 end;
        public Color color;

        public PowerLink(Vector2 start, Vector2 end, Color color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }
    }
}
