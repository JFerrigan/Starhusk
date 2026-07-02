using System.Collections.Generic;
using UnityEngine;

public class PlanetLogisticsNetwork : MonoBehaviour
{
    public const float DefaultRegistrationRange = 260f;

    private static readonly List<PlanetLogisticsNetwork> activeNetworks = new List<PlanetLogisticsNetwork>();

    public float registrationRange = DefaultRegistrationRange;
    public float routeAltitude = 14f;
    public float routeProbeRadius = 1.5f;
    public float obstacleClearance = 6f;
    public float rebuildDelay = 0.15f;

    private readonly List<LogisticsNode> nodes = new List<LogisticsNode>();
    private readonly List<LogisticsRouteSegment> routeSegments = new List<LogisticsRouteSegment>();
    private readonly Dictionary<LogisticsNode, Vector2> loopPoints = new Dictionary<LogisticsNode, Vector2>();

    private bool dirty = true;
    private float rebuildTime;
    private int routeVersion;

    public static IReadOnlyList<PlanetLogisticsNetwork> ActiveNetworks => activeNetworks;
    public IReadOnlyList<LogisticsNode> Nodes => nodes;
    public IReadOnlyList<LogisticsRouteSegment> RouteSegments => routeSegments;
    public bool IsDirty => dirty;
    public int RouteVersion => routeVersion;

    private void OnEnable()
    {
        if (!activeNetworks.Contains(this))
        {
            activeNetworks.Add(this);
        }

        MarkDirty();
    }

    private void OnDisable()
    {
        activeNetworks.Remove(this);
    }

    private void Update()
    {
        if (dirty && Time.time >= rebuildTime)
        {
            Rebuild();
        }

        if (HiddenRoutingDisplayController.RoutesVisible)
        {
            DrawHiddenRoutes();
        }
    }

    public void Register(LogisticsNode node)
    {
        if (node == null || nodes.Contains(node))
        {
            return;
        }

        nodes.Add(node);
        MarkDirty();
    }

    public void Unregister(LogisticsNode node)
    {
        if (node == null)
        {
            return;
        }

        if (nodes.Remove(node))
        {
            loopPoints.Remove(node);
            MarkDirty();
        }
    }

    public void MarkDirty()
    {
        dirty = true;
        rebuildTime = Time.time + Mathf.Max(0f, rebuildDelay);
    }

    public void Rebuild()
    {
        dirty = false;
        routeVersion++;
        routeSegments.Clear();
        loopPoints.Clear();
        PruneInvalidNodes();
        nodes.Sort(CompareNodes);

        if (nodes.Count <= 0)
        {
            return;
        }

        float routeRadius = PlanetRouteRadius();
        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode node = nodes[i];
            Vector2 direction = ((Vector2)node.transform.position - (Vector2)transform.position).normalized;
            if (direction.sqrMagnitude < 0.001f)
            {
                float angle = (360f / Mathf.Max(1, nodes.Count)) * i * Mathf.Deg2Rad;
                direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            Vector2 loopPoint = (Vector2)transform.position + direction * routeRadius;
            loopPoints[node] = loopPoint;
            AppendPath(node.transform.position, loopPoint, node.PrimaryCollider, null);
        }

        if (nodes.Count == 1)
        {
            return;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode current = nodes[i];
            LogisticsNode next = nodes[(i + 1) % nodes.Count];
            AppendPath(loopPoints[current], loopPoints[next], null, null);
        }
    }

    public IReadOnlyList<Vector2> GetPath(Vector2 from, Transform destination)
    {
        return GetRoutePath(from, destination);
    }

    public IReadOnlyList<Vector2> GetRoutePath(Vector2 from, Transform destination)
    {
        EnsureFreshRoute();
        List<Vector2> path = new List<Vector2>();
        LogisticsNode destinationNode = FindNode(destination);
        if (destinationNode == null || routeSegments.Count <= 0)
        {
            return path;
        }

        Vector2 startPoint = NearestRoutePoint(from);
        Vector2 destinationPoint = destinationNode.transform.position;
        List<Vector2> graphPath = ShortestGraphPath(startPoint, destinationPoint);
        if (graphPath.Count <= 0)
        {
            return path;
        }

        if (Vector2.Distance(from, graphPath[0]) > 0.1f)
        {
            path.Add(graphPath[0]);
        }

        for (int i = 1; i < graphPath.Count; i++)
        {
            path.Add(graphPath[i]);
        }

        return path;
    }

    public PlanetResourceExtractorBuilding FindBestExtractor(Vector2 from, float scanRange)
    {
        PlanetResourceExtractorBuilding bestExtractor = null;
        int bestAmount = 0;
        float rangeSquared = scanRange * scanRange;

        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode node = nodes[i];
            PlanetResourceExtractorBuilding extractor = node == null ? null : node.Extractor;
            if (extractor == null || extractor.Storage == null || extractor.Storage.CurrentAmount <= 0)
            {
                continue;
            }

            if (Vector2.SqrMagnitude((Vector2)extractor.transform.position - from) > rangeSquared)
            {
                continue;
            }

            if (extractor.Storage.CurrentAmount > bestAmount)
            {
                bestAmount = extractor.Storage.CurrentAmount;
                bestExtractor = extractor;
            }
        }

        return bestExtractor;
    }

    public CollectorHub FindNearestCargoWithCapacity(Vector2 from)
    {
        CollectorHub nearestHub = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode node = nodes[i];
            CollectorHub hub = node == null ? null : node.CargoHub;
            if (hub == null || hub.Storage == null || hub.Storage.IsFull)
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude((Vector2)hub.transform.position - from);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHub = hub;
            }
        }

        return nearestHub;
    }

    public static PlanetLogisticsNetwork FindNearest(Vector2 position, float maxRange = DefaultRegistrationRange)
    {
        PlanetLogisticsNetwork nearest = null;
        float nearestDistance = maxRange * maxRange;

        for (int i = 0; i < activeNetworks.Count; i++)
        {
            PlanetLogisticsNetwork candidate = activeNetworks[i];
            if (candidate == null)
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude((Vector2)candidate.transform.position - position);
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private Vector2 NearestRoutePoint(Vector2 position)
    {
        Vector2 nearest = routeSegments[0].start;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < routeSegments.Count; i++)
        {
            ConsiderRoutePoint(routeSegments[i].start, position, ref nearest, ref nearestDistance);
            ConsiderRoutePoint(routeSegments[i].end, position, ref nearest, ref nearestDistance);
        }

        return nearest;
    }

    private static void ConsiderRoutePoint(Vector2 point, Vector2 position, ref Vector2 nearest, ref float nearestDistance)
    {
        float distance = Vector2.SqrMagnitude(point - position);
        if (distance < nearestDistance)
        {
            nearestDistance = distance;
            nearest = point;
        }
    }

    private List<Vector2> ShortestGraphPath(Vector2 start, Vector2 end)
    {
        List<Vector2> graphPoints = BuildGraphPoints();
        int startIndex = FindPointIndex(graphPoints, start);
        int endIndex = FindPointIndex(graphPoints, end);
        if (startIndex < 0 || endIndex < 0)
        {
            return new List<Vector2>();
        }

        float[] distances = new float[graphPoints.Count];
        int[] previous = new int[graphPoints.Count];
        bool[] visited = new bool[graphPoints.Count];

        for (int i = 0; i < graphPoints.Count; i++)
        {
            distances[i] = float.MaxValue;
            previous[i] = -1;
        }

        distances[startIndex] = 0f;

        for (int step = 0; step < graphPoints.Count; step++)
        {
            int current = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < graphPoints.Count; i++)
            {
                if (!visited[i] && distances[i] < bestDistance)
                {
                    bestDistance = distances[i];
                    current = i;
                }
            }

            if (current < 0 || current == endIndex)
            {
                break;
            }

            visited[current] = true;
            for (int i = 0; i < routeSegments.Count; i++)
            {
                int startSegmentIndex = FindPointIndex(graphPoints, routeSegments[i].start);
                int endSegmentIndex = FindPointIndex(graphPoints, routeSegments[i].end);
                if (startSegmentIndex < 0 || endSegmentIndex < 0)
                {
                    continue;
                }

                RelaxEdge(current, startSegmentIndex, endSegmentIndex, graphPoints, distances, previous);
                RelaxEdge(current, endSegmentIndex, startSegmentIndex, graphPoints, distances, previous);
            }
        }

        if (startIndex != endIndex && previous[endIndex] < 0)
        {
            return new List<Vector2>();
        }

        List<Vector2> path = new List<Vector2>();
        int pathIndex = endIndex;
        path.Add(graphPoints[pathIndex]);
        while (pathIndex != startIndex)
        {
            pathIndex = previous[pathIndex];
            if (pathIndex < 0)
            {
                return new List<Vector2>();
            }

            path.Add(graphPoints[pathIndex]);
        }

        path.Reverse();
        return path;
    }

    private static void RelaxEdge(int current, int from, int to, IReadOnlyList<Vector2> graphPoints, float[] distances, int[] previous)
    {
        if (current != from || distances[from] >= float.MaxValue)
        {
            return;
        }

        float candidateDistance = distances[from] + Vector2.Distance(graphPoints[from], graphPoints[to]);
        if (candidateDistance < distances[to])
        {
            distances[to] = candidateDistance;
            previous[to] = from;
        }
    }

    private List<Vector2> BuildGraphPoints()
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < routeSegments.Count; i++)
        {
            AddUniquePoint(points, routeSegments[i].start);
            AddUniquePoint(points, routeSegments[i].end);
        }

        return points;
    }

    private static void AddUniquePoint(List<Vector2> points, Vector2 point)
    {
        if (FindPointIndex(points, point) < 0)
        {
            points.Add(point);
        }
    }

    private static int FindPointIndex(IReadOnlyList<Vector2> points, Vector2 point)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (Vector2.Distance(points[i], point) <= 0.05f)
            {
                return i;
            }
        }

        return -1;
    }

    private LogisticsNode FindNode(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode node = nodes[i];
            if (node != null && (node.transform == target || target.IsChildOf(node.transform)))
            {
                return node;
            }
        }

        return null;
    }

    private LogisticsNode NearestNode(Vector2 position)
    {
        LogisticsNode nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < nodes.Count; i++)
        {
            LogisticsNode node = nodes[i];
            if (node == null || !loopPoints.ContainsKey(node))
            {
                continue;
            }

            float distance = Vector2.SqrMagnitude(loopPoints[node] - position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = node;
            }
        }

        return nearest;
    }

    private void EnsureFreshRoute()
    {
        if (dirty)
        {
            Rebuild();
        }
    }

    private void PruneInvalidNodes()
    {
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i] == null || !nodes[i].isActiveAndEnabled)
            {
                nodes.RemoveAt(i);
            }
        }
    }

    private void AppendPath(Vector2 start, Vector2 end, Collider2D sourceCollider, Collider2D targetCollider)
    {
        List<Vector2> path = LogisticsRoutePlanner.BuildSegmentPath(start, end, transform, routeProbeRadius, obstacleClearance, sourceCollider, targetCollider);
        for (int i = 0; i < path.Count - 1; i++)
        {
            routeSegments.Add(new LogisticsRouteSegment(path[i], path[i + 1]));
        }
    }

    private float PlanetRouteRadius()
    {
        return PlanetSurfaceAnchor.SurfaceRadiusFor(transform) + Mathf.Max(0f, routeAltitude);
    }

    private int CompareNodes(LogisticsNode left, LogisticsNode right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        Vector2 center = transform.position;
        Vector2 leftDelta = (Vector2)left.transform.position - center;
        Vector2 rightDelta = (Vector2)right.transform.position - center;
        float leftAngle = Mathf.Atan2(leftDelta.y, leftDelta.x);
        float rightAngle = Mathf.Atan2(rightDelta.y, rightDelta.x);
        int angleCompare = leftAngle.CompareTo(rightAngle);
        if (angleCompare != 0)
        {
            return angleCompare;
        }

        int distanceCompare = leftDelta.sqrMagnitude.CompareTo(rightDelta.sqrMagnitude);
        if (distanceCompare != 0)
        {
            return distanceCompare;
        }

        return string.CompareOrdinal(left.name, right.name);
    }

    private void DrawHiddenRoutes()
    {
        Color routeColor = new Color(0.15f, 0.95f, 1f, 0.9f);
        for (int i = 0; i < routeSegments.Count; i++)
        {
            Debug.DrawLine(routeSegments[i].start, routeSegments[i].end, routeColor);
        }
    }
}
