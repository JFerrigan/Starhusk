using System.Collections.Generic;
using UnityEngine;

public class CircularDestructibleAsteroid : MonoBehaviour
{
    public int gridResolution = 34;
    public float localRadius = 0.5f;
    public float minimumLiveArea = 0.018f;
    public float destroyedMapHideThreshold = 0.75f;
    public int maxLiveFragments = 5;
    public int meshSortingOrder = 10;

    private readonly HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
    private ResourceDeposit deposit;
    private MapMarker mapMarker;
    private PolygonCollider2D polygonCollider;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private SpriteRenderer spriteRenderer;
    private Color tint = Color.white;
    private float cellSize;
    private float initialArea;
    private float lastImpactFlashUntil;

    public float CurrentArea => cells.Count * cellSize * cellSize * LocalScaleArea();
    public int CellCount => cells.Count;
    public float DestroyedFraction => initialArea <= 0f ? 0f : Mathf.Clamp01(1f - (CurrentArea / initialArea));
    public bool ShouldAppearOnMapAndRadar => DestroyedFraction < Mathf.Clamp01(destroyedMapHideThreshold);

    private void Awake()
    {
        deposit = GetComponent<ResourceDeposit>();
        mapMarker = GetComponent<MapMarker>();
        CacheSpriteRenderer();
        EnsureGeneratedShape();
        EnsureInitialArea();
        RebuildGeometry();
        ApplyDiscoveryVisibility();
    }

    private void Update()
    {
        if (meshRenderer == null)
        {
            return;
        }

        if (Time.time < lastImpactFlashUntil)
        {
            SetMeshColor(WithDiscoveryAlpha(Color.Lerp(tint, Color.white, 0.55f)));
        }
        else
        {
            SetMeshColor(WithDiscoveryAlpha(tint));
        }
    }

    public void InitializeFromCells(IEnumerable<Vector2Int> sourceCells, float sourceCellSize, Color sourceTint, IReadOnlyList<ResourceStack> resources, int mineAmount, float sourceInitialArea = 0f)
    {
        cells.Clear();
        cellSize = sourceCellSize;
        tint = sourceTint;
        initialArea = sourceInitialArea;

        foreach (Vector2Int cell in sourceCells)
        {
            cells.Add(cell);
        }

        deposit = GetComponent<ResourceDeposit>();
        mapMarker = GetComponent<MapMarker>();
        if (deposit == null)
        {
            deposit = gameObject.AddComponent<ResourceDeposit>();
        }

        deposit.ConfigureResourcesExact(resources, mineAmount);
        CacheSpriteRenderer();
        EnsureInitialArea();
        RebuildGeometry();
        ApplyDiscoveryVisibility();
    }

    public void SetTint(Color asteroidTint)
    {
        tint = asteroidTint;
        SetMeshColor(WithDiscoveryAlpha(tint));
        ApplyDiscoveryVisibility();
    }

    public bool ApplyCircularCut(Vector2 worldPoint, float worldRadius, Vector2 shotDirection)
    {
        if (cells.Count <= 0)
        {
            return false;
        }

        Vector2 localPoint = transform.InverseTransformPoint(worldPoint);
        float localCutRadius = WorldRadiusToLocal(worldRadius);
        if (!IntersectsCurrentShape(localPoint, localCutRadius))
        {
            return false;
        }

        float previousArea = CurrentArea;
        List<ResourceStack> previousResources = CopyResources(deposit);

        List<Vector2Int> removedCells = new List<Vector2Int>();
        foreach (Vector2Int cell in cells)
        {
            if ((CellCenter(cell) - localPoint).sqrMagnitude <= localCutRadius * localCutRadius)
            {
                removedCells.Add(cell);
            }
        }

        if (removedCells.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < removedCells.Count; i++)
        {
            cells.Remove(removedCells[i]);
        }

        List<List<Vector2Int>> components = FindComponents(cells);
        List<List<Vector2Int>> liveComponents = new List<List<Vector2Int>>();
        float removedArea = previousArea;

        for (int i = 0; i < components.Count; i++)
        {
            float componentArea = AreaForCells(components[i]);
            if (componentArea >= minimumLiveArea && liveComponents.Count < maxLiveFragments)
            {
                liveComponents.Add(components[i]);
                removedArea -= componentArea;
            }
        }

        liveComponents.Sort((a, b) => b.Count.CompareTo(a.Count));
        float liveArea = Mathf.Max(0f, previousArea - removedArea);
        List<ResourceStack> pickupResources = AllocateRemovedResources(previousResources, previousArea, removedArea);
        List<List<ResourceStack>> liveResources = AllocateLiveResources(previousResources, pickupResources, liveComponents, liveArea);

        SpawnPickups(pickupResources, worldPoint, shotDirection);
        lastImpactFlashUntil = Time.time + 0.08f;

        if (liveComponents.Count <= 0)
        {
            UpdateMapAndRadarVisibility();
            Destroy(gameObject);
            return true;
        }

        ApplyComponentToSelf(liveComponents[0], liveResources.Count > 0 ? liveResources[0] : null);

        for (int i = 1; i < liveComponents.Count; i++)
        {
            SpawnChunk(liveComponents[i], liveResources.Count > i ? liveResources[i] : null, worldPoint);
        }

        return true;
    }

    public bool DestroyEntireAsteroid(Vector2 impactPoint, Vector2 direction)
    {
        if (cells.Count <= 0)
        {
            return false;
        }

        SpawnPickups(CopyResources(deposit), impactPoint, direction);
        cells.Clear();
        UpdateMapAndRadarVisibility();

        if (mapMarker == null)
        {
            mapMarker = GetComponent<MapMarker>();
        }

        if (mapMarker != null)
        {
            mapMarker.hiddenFromMapAndRadar = true;
        }

        DestroyAsteroidObject(gameObject);
        return true;
    }

    private void EnsureGeneratedShape()
    {
        gridResolution = Mathf.Clamp(gridResolution, 12, 80);

        if (cellSize <= 0f)
        {
            cellSize = (localRadius * 2f) / gridResolution;
        }

        if (cells.Count > 0)
        {
            return;
        }

        for (int y = -gridResolution; y <= gridResolution; y++)
        {
            for (int x = -gridResolution; x <= gridResolution; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector2 center = CellCenter(cell);
                if (center.magnitude <= localRadius)
                {
                    cells.Add(cell);
                }
            }
        }
    }

    private void RebuildGeometry()
    {
        if (!EnsureMeshComponents())
        {
            return;
        }

        RemoveSolidCircleColliders();

        List<Vector3> vertices = new List<Vector3>(cells.Count * 4);
        List<int> triangles = new List<int>(cells.Count * 6);

        foreach (Vector2Int cell in cells)
        {
            Vector2 center = CellCenter(cell);
            float half = cellSize * 0.5f;
            int start = vertices.Count;

            vertices.Add(new Vector3(center.x - half, center.y - half, 0f));
            vertices.Add(new Vector3(center.x + half, center.y - half, 0f));
            vertices.Add(new Vector3(center.x + half, center.y + half, 0f));
            vertices.Add(new Vector3(center.x - half, center.y + half, 0f));

            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Destructible Asteroid Mesh";
            meshFilter.sharedMesh = mesh;
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();

        RebuildCollider();
        UpdateMapAndRadarVisibility();
        ApplyDiscoveryVisibility();
    }

    private void EnsureInitialArea()
    {
        if (initialArea <= 0f)
        {
            initialArea = CurrentArea;
        }
    }

    private void UpdateMapAndRadarVisibility()
    {
        if (mapMarker == null)
        {
            mapMarker = GetComponent<MapMarker>();
        }

        if (mapMarker != null)
        {
            mapMarker.hiddenFromMapAndRadar = !ShouldAppearOnMapAndRadar;
        }
    }

    private void RebuildCollider()
    {
        if (polygonCollider == null)
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
        }

        if (polygonCollider == null)
        {
            polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        }

        if (cells.Count <= 0)
        {
            polygonCollider.pathCount = 1;
            polygonCollider.SetPath(0, new Vector2[0]);
            return;
        }

        List<Vector2[]> paths = BuildColliderPaths();
        polygonCollider.pathCount = Mathf.Max(1, paths.Count);

        if (paths.Count <= 0)
        {
            polygonCollider.SetPath(0, new Vector2[0]);
            return;
        }

        for (int i = 0; i < paths.Count; i++)
        {
            polygonCollider.SetPath(i, paths[i]);
        }
    }

    private List<Vector2[]> BuildColliderPaths()
    {
        HashSet<BoundaryEdge> remainingEdges = BuildBoundaryEdges();
        List<Vector2[]> paths = new List<Vector2[]>();

        while (remainingEdges.Count > 0)
        {
            BoundaryEdge edge = default;
            foreach (BoundaryEdge candidate in remainingEdges)
            {
                edge = candidate;
                break;
            }

            Vector2Int start = edge.start;
            List<Vector2> path = new List<Vector2>();
            int safety = remainingEdges.Count + 1;

            while (safety > 0 && remainingEdges.Remove(edge))
            {
                if (path.Count <= 0)
                {
                    path.Add(GridPointToLocal(edge.start));
                }

                path.Add(GridPointToLocal(edge.end));

                if (edge.end == start)
                {
                    break;
                }

                if (!TryGetNextBoundaryEdge(edge.end, remainingEdges, out edge))
                {
                    break;
                }

                safety--;
            }

            RemoveDuplicateClosingPoint(path);
            SimplifyStraightPathSegments(path);

            if (path.Count >= 3)
            {
                paths.Add(path.ToArray());
            }
        }

        return paths;
    }

    private HashSet<BoundaryEdge> BuildBoundaryEdges()
    {
        HashSet<BoundaryEdge> edges = new HashSet<BoundaryEdge>();

        foreach (Vector2Int cell in cells)
        {
            Vector2Int bottomLeft = new Vector2Int(cell.x, cell.y);
            Vector2Int bottomRight = new Vector2Int(cell.x + 1, cell.y);
            Vector2Int topRight = new Vector2Int(cell.x + 1, cell.y + 1);
            Vector2Int topLeft = new Vector2Int(cell.x, cell.y + 1);

            if (!cells.Contains(cell + Vector2Int.down))
            {
                edges.Add(new BoundaryEdge(bottomLeft, bottomRight));
            }

            if (!cells.Contains(cell + Vector2Int.right))
            {
                edges.Add(new BoundaryEdge(bottomRight, topRight));
            }

            if (!cells.Contains(cell + Vector2Int.up))
            {
                edges.Add(new BoundaryEdge(topRight, topLeft));
            }

            if (!cells.Contains(cell + Vector2Int.left))
            {
                edges.Add(new BoundaryEdge(topLeft, bottomLeft));
            }
        }

        return edges;
    }

    private static bool TryGetNextBoundaryEdge(Vector2Int start, HashSet<BoundaryEdge> remainingEdges, out BoundaryEdge next)
    {
        foreach (BoundaryEdge edge in remainingEdges)
        {
            if (edge.start == start)
            {
                next = edge;
                return true;
            }
        }

        next = default;
        return false;
    }

    private Vector2 GridPointToLocal(Vector2Int point)
    {
        return new Vector2(point.x * cellSize, point.y * cellSize);
    }

    private static void RemoveDuplicateClosingPoint(List<Vector2> path)
    {
        if (path.Count > 1 && (path[0] - path[path.Count - 1]).sqrMagnitude <= 0.000001f)
        {
            path.RemoveAt(path.Count - 1);
        }
    }

    private static void SimplifyStraightPathSegments(List<Vector2> path)
    {
        if (path.Count < 4)
        {
            return;
        }

        for (int i = path.Count - 1; i >= 0; i--)
        {
            Vector2 previous = path[(i - 1 + path.Count) % path.Count];
            Vector2 current = path[i];
            Vector2 next = path[(i + 1) % path.Count];
            Vector2 a = (current - previous).normalized;
            Vector2 b = (next - current).normalized;

            if (Mathf.Abs((a.x * b.y) - (a.y * b.x)) <= 0.0001f && Vector2.Dot(a, b) > 0.999f)
            {
                path.RemoveAt(i);
            }
        }
    }

    private struct BoundaryEdge
    {
        public readonly Vector2Int start;
        public readonly Vector2Int end;

        public BoundaryEdge(Vector2Int start, Vector2Int end)
        {
            this.start = start;
            this.end = end;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BoundaryEdge))
            {
                return false;
            }

            BoundaryEdge other = (BoundaryEdge)obj;
            return start == other.start && end == other.end;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (start.GetHashCode() * 397) ^ end.GetHashCode();
            }
        }
    }

    private void ApplyComponentToSelf(List<Vector2Int> component, IReadOnlyList<ResourceStack> resources)
    {
        Vector2 centroid = Centroid(component);
        Vector3 worldCentroid = transform.TransformPoint(centroid);

        cells.Clear();
        for (int i = 0; i < component.Count; i++)
        {
            cells.Add(RecenterCell(component[i], centroid));
        }

        transform.position = new Vector3(worldCentroid.x, worldCentroid.y, transform.position.z);

        if (deposit == null)
        {
            deposit = GetComponent<ResourceDeposit>();
        }

        if (deposit != null)
        {
            deposit.ConfigureResourcesExact(resources, deposit.mineAmountPerInteraction);
        }

        RebuildGeometry();
    }

    private void SpawnChunk(List<Vector2Int> component, IReadOnlyList<ResourceStack> resources, Vector2 impactPoint)
    {
        Vector2 centroid = Centroid(component);
        Vector3 worldCentroid = transform.TransformPoint(centroid);
        List<Vector2Int> recenteredCells = new List<Vector2Int>(component.Count);

        for (int i = 0; i < component.Count; i++)
        {
            recenteredCells.Add(RecenterCell(component[i], centroid));
        }

        GameObject chunkObject = new GameObject(gameObject.name + " Fragment");
        chunkObject.transform.SetParent(transform.parent);
        chunkObject.transform.position = new Vector3(worldCentroid.x, worldCentroid.y, transform.position.z);
        chunkObject.transform.rotation = transform.rotation;
        chunkObject.transform.localScale = transform.localScale;

        ObjectNamer.AssignIdentity(chunkObject, chunkObject.name, ObjectIdentityCategory.Celestial);

        DiscoveryState sourceDiscovery = GetComponent<DiscoveryState>();
        DiscoveryState chunkDiscovery = chunkObject.AddComponent<DiscoveryState>();
        chunkDiscovery.SetDiscovered(sourceDiscovery == null || sourceDiscovery.discovered);
        chunkDiscovery.passiveRevealRadius = sourceDiscovery == null ? 8f : sourceDiscovery.passiveRevealRadius;

        MapMarker sourceMarker = GetComponent<MapMarker>();
        MapMarker marker = chunkObject.AddComponent<MapMarker>();
        marker.markerType = MapMarkerType.Asteroid;
        marker.markerColor = tint;
        marker.iconScale = sourceMarker == null ? 0.8f : sourceMarker.iconScale;
        marker.requireDiscovery = sourceMarker == null || sourceMarker.requireDiscovery;
        marker.discoveryState = chunkDiscovery;

        Rigidbody2D rb = chunkObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0.15f;
        rb.angularDamping = 0.25f;
        Vector2 outward = ((Vector2)worldCentroid - impactPoint).sqrMagnitude > 0.001f
            ? ((Vector2)worldCentroid - impactPoint).normalized
            : Random.insideUnitCircle.normalized;
        rb.linearVelocity = outward * Random.Range(0.5f, 1.8f);
        rb.angularVelocity = Random.Range(-18f, 18f);

        ResourceDeposit chunkDeposit = chunkObject.AddComponent<ResourceDeposit>();
        chunkDeposit.destroyWhenDepleted = false;
        chunkDeposit.ConfigureResourcesExact(resources, deposit == null ? 12 : deposit.mineAmountPerInteraction);

        CircularDestructibleAsteroid chunk = chunkObject.AddComponent<CircularDestructibleAsteroid>();
        chunk.gridResolution = gridResolution;
        chunk.localRadius = localRadius;
        chunk.minimumLiveArea = minimumLiveArea;
        chunk.destroyedMapHideThreshold = destroyedMapHideThreshold;
        chunk.maxLiveFragments = maxLiveFragments;
        chunk.InitializeFromCells(recenteredCells, cellSize, tint, resources, deposit == null ? 12 : deposit.mineAmountPerInteraction, initialArea);
    }

    private void SpawnPickups(IReadOnlyList<ResourceStack> resources, Vector2 impactPoint, Vector2 shotDirection)
    {
        if (resources == null)
        {
            return;
        }

        Vector2 baseDirection = shotDirection.sqrMagnitude > 0.001f ? shotDirection.normalized : Vector2.up;

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];
            int amountRemaining = stack.amount;

            while (amountRemaining > 0)
            {
                int pickupAmount = Mathf.Min(amountRemaining, Mathf.Max(1, Mathf.CeilToInt(stack.amount / 5f)));
                amountRemaining -= pickupAmount;

                GameObject pickupObject = new GameObject(stack.type + " Pickup");
                pickupObject.transform.position = new Vector3(impactPoint.x, impactPoint.y, transform.position.z);

                Vector2 spread = Quaternion.Euler(0f, 0f, Random.Range(-75f, 75f)) * baseDirection;
                Vector2 velocity = (spread.normalized * Random.Range(1.2f, 4.2f)) + Random.insideUnitCircle * 0.5f;

                ResourcePickup pickup = pickupObject.AddComponent<ResourcePickup>();
                pickup.Initialize(stack.type, pickupAmount, velocity, ResourceVisuals.ColorFor(stack.type));
            }
        }
    }

    private List<List<Vector2Int>> FindComponents(HashSet<Vector2Int> sourceCells)
    {
        List<List<Vector2Int>> components = new List<List<Vector2Int>>();
        HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>(sourceCells);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        while (unvisited.Count > 0)
        {
            Vector2Int start = default;
            foreach (Vector2Int cell in unvisited)
            {
                start = cell;
                break;
            }

            List<Vector2Int> component = new List<Vector2Int>();
            unvisited.Remove(start);
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                component.Add(current);

                TryVisit(current + Vector2Int.up, unvisited, queue);
                TryVisit(current + Vector2Int.down, unvisited, queue);
                TryVisit(current + Vector2Int.left, unvisited, queue);
                TryVisit(current + Vector2Int.right, unvisited, queue);
            }

            components.Add(component);
        }

        return components;
    }

    private static void TryVisit(Vector2Int cell, HashSet<Vector2Int> unvisited, Queue<Vector2Int> queue)
    {
        if (unvisited.Remove(cell))
        {
            queue.Enqueue(cell);
        }
    }

    private bool IntersectsCurrentShape(Vector2 localPoint, float localCutRadius)
    {
        float radiusSquared = localCutRadius * localCutRadius;
        foreach (Vector2Int cell in cells)
        {
            if ((CellCenter(cell) - localPoint).sqrMagnitude <= radiusSquared)
            {
                return true;
            }
        }

        return false;
    }

    private List<ResourceStack> AllocateRemovedResources(IReadOnlyList<ResourceStack> previousResources, float previousArea, float removedArea)
    {
        List<ResourceStack> removed = new List<ResourceStack>();
        if (previousResources == null || previousArea <= 0f || removedArea <= 0f)
        {
            return removed;
        }

        float removedRatio = Mathf.Clamp01(removedArea / previousArea);

        for (int i = 0; i < previousResources.Count; i++)
        {
            ResourceStack stack = previousResources[i];
            int amount = Mathf.Clamp(Mathf.RoundToInt(stack.amount * removedRatio), 0, stack.amount);
            if (amount > 0)
            {
                removed.Add(new ResourceStack(stack.type, amount));
            }
        }

        return removed;
    }

    private List<List<ResourceStack>> AllocateLiveResources(IReadOnlyList<ResourceStack> previousResources, IReadOnlyList<ResourceStack> removedResources, List<List<Vector2Int>> liveComponents, float liveArea)
    {
        List<List<ResourceStack>> allocations = new List<List<ResourceStack>>();
        for (int i = 0; i < liveComponents.Count; i++)
        {
            allocations.Add(new List<ResourceStack>());
        }

        if (previousResources == null || liveComponents.Count <= 0)
        {
            return allocations;
        }

        for (int resourceIndex = 0; resourceIndex < previousResources.Count; resourceIndex++)
        {
            ResourceStack stack = previousResources[resourceIndex];
            int removedAmount = AmountOf(removedResources, stack.type);
            int remainingAmount = Mathf.Max(0, stack.amount - removedAmount);
            if (remainingAmount <= 0)
            {
                continue;
            }

            int assigned = 0;
            float bestRemainder = -1f;
            int bestIndex = 0;

            for (int componentIndex = 0; componentIndex < liveComponents.Count; componentIndex++)
            {
                float componentArea = AreaForCells(liveComponents[componentIndex]);
                float exact = liveArea <= 0f ? 0f : remainingAmount * (componentArea / liveArea);
                int amount = Mathf.FloorToInt(exact);
                assigned += amount;

                if (amount > 0)
                {
                    allocations[componentIndex].Add(new ResourceStack(stack.type, amount));
                }

                float remainder = exact - amount;
                if (remainder > bestRemainder)
                {
                    bestRemainder = remainder;
                    bestIndex = componentIndex;
                }
            }

            int leftovers = remainingAmount - assigned;
            if (leftovers > 0 && allocations.Count > 0)
            {
                AddOrMerge(allocations[bestIndex], stack.type, leftovers);
            }
        }

        return allocations;
    }

    private bool EnsureMeshComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshFilter == null)
        {
            return false;
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (meshRenderer == null)
        {
            return false;
        }

        if (meshRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            if (shader != null)
            {
                meshRenderer.sharedMaterial = new Material(shader);
            }
        }

        SetMeshColor(tint);
        meshRenderer.sortingOrder = meshSortingOrder;
        if (spriteRenderer != null)
        {
            DestroyComponent(spriteRenderer);
            spriteRenderer = null;
        }
        return true;
    }

    private void CacheSpriteRenderer()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            tint = spriteRenderer.color;
        }
    }

    private void ApplyDiscoveryVisibility()
    {
        DiscoveryState discovery = GetComponent<DiscoveryState>();
        if (discovery != null)
        {
            discovery.SetDiscovered(discovery.discovered);
        }
    }

    private Color WithDiscoveryAlpha(Color color)
    {
        DiscoveryState discovery = GetComponent<DiscoveryState>();
        color.a = discovery == null || discovery.discovered ? tint.a : tint.a * 0.18f;
        return color;
    }

    private void SetMeshColor(Color color)
    {
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            return;
        }

        meshRenderer.sharedMaterial.color = color;
    }

    private void RemoveSolidCircleColliders()
    {
        CircleCollider2D[] circleColliders = GetComponents<CircleCollider2D>();
        for (int i = 0; i < circleColliders.Length; i++)
        {
            if (circleColliders[i] != null && !circleColliders[i].isTrigger)
            {
                DestroyComponent(circleColliders[i]);
            }
        }
    }

    private static void DestroyComponent(Component component)
    {
        if (Application.isPlaying)
        {
            Destroy(component);
        }
        else
        {
            DestroyImmediate(component);
        }
    }

    private static void DestroyAsteroidObject(GameObject asteroidObject)
    {
        if (Application.isPlaying)
        {
            Destroy(asteroidObject);
        }
        else
        {
            DestroyImmediate(asteroidObject);
        }
    }

    private Vector2 CellCenter(Vector2Int cell)
    {
        return new Vector2((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize);
    }

    private Vector2Int RecenterCell(Vector2Int cell, Vector2 centroid)
    {
        Vector2 localCenter = CellCenter(cell) - centroid;
        return new Vector2Int(
            Mathf.RoundToInt((localCenter.x / cellSize) - 0.5f),
            Mathf.RoundToInt((localCenter.y / cellSize) - 0.5f)
        );
    }

    private Vector2 Centroid(IReadOnlyList<Vector2Int> component)
    {
        if (component == null || component.Count <= 0)
        {
            return Vector2.zero;
        }

        Vector2 sum = Vector2.zero;
        for (int i = 0; i < component.Count; i++)
        {
            sum += CellCenter(component[i]);
        }

        return sum / component.Count;
    }

    private float AreaForCells(IReadOnlyList<Vector2Int> sourceCells)
    {
        return sourceCells == null ? 0f : sourceCells.Count * cellSize * cellSize * LocalScaleArea();
    }

    private float LocalScaleArea()
    {
        Vector3 scale = transform.lossyScale;
        return Mathf.Abs(scale.x * scale.y);
    }

    private float WorldRadiusToLocal(float worldRadius)
    {
        Vector3 scale = transform.lossyScale;
        float averageScale = Mathf.Max(0.001f, (Mathf.Abs(scale.x) + Mathf.Abs(scale.y)) * 0.5f);
        return worldRadius / averageScale;
    }

    private static List<ResourceStack> CopyResources(ResourceDeposit resourceDeposit)
    {
        List<ResourceStack> copy = new List<ResourceStack>();
        if (resourceDeposit == null)
        {
            return copy;
        }

        IReadOnlyList<ResourceStack> resources = resourceDeposit.Resources;
        for (int i = 0; i < resources.Count; i++)
        {
            ResourceStack stack = resources[i];
            if (stack.amount > 0)
            {
                copy.Add(stack);
            }
        }

        return copy;
    }

    private static int AmountOf(IReadOnlyList<ResourceStack> resources, ResourceType type)
    {
        if (resources == null)
        {
            return 0;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].type == type)
            {
                return resources[i].amount;
            }
        }

        return 0;
    }

    private static void AddOrMerge(List<ResourceStack> resources, ResourceType type, int amount)
    {
        if (resources == null || amount <= 0)
        {
            return;
        }

        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].type != type)
            {
                continue;
            }

            ResourceStack stack = resources[i];
            stack.amount += amount;
            resources[i] = stack;
            return;
        }

        resources.Add(new ResourceStack(type, amount));
    }
}
