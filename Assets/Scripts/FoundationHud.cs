using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FoundationHud : MonoBehaviour
{
    private const float DefaultSafeImpactSpeed = 8f;

    private ResourceInventory inventory;
    private PlayerMovement playerMovement;
    private PlayerScanner scanner;
    private PlayerRadarPing radarPing;
    private ShipHealth playerHealth;
    private ShipCrashDamage playerCrashDamage;
    private StarSystemGenerator generator;
    private Texture2D pixel;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;
    private readonly List<CollisionWarningCandidate> collisionWarningCandidates = new List<CollisionWarningCandidate>();

    [Header("Collision Warnings")]
    public float collisionWarningSpeedMultiplier = 3f;
    public float collisionWarningMaxDistance = 650f;
    public float collisionWarningExtraCorridor = 18f;
    public float collisionWarningMinScale = 0.75f;
    public float collisionWarningMaxScale = 1.55f;
    public int maxCollisionWarnings = 3;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
        labelStyle = new GUIStyle
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };
        smallLabelStyle = new GUIStyle
        {
            fontSize = 12,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<ResourceInventory>();
        }

        if (scanner == null && inventory != null)
        {
            scanner = inventory.GetComponent<PlayerScanner>();
        }

        if (playerMovement == null && inventory != null)
        {
            playerMovement = inventory.GetComponent<PlayerMovement>();
        }

        if (radarPing == null && inventory != null)
        {
            radarPing = inventory.GetComponent<PlayerRadarPing>();
        }

        if (playerHealth == null && inventory != null)
        {
            playerHealth = inventory.GetComponent<ShipHealth>();
        }

        if (playerCrashDamage == null && inventory != null)
        {
            playerCrashDamage = inventory.GetComponent<ShipCrashDamage>();
        }

        if (generator == null)
        {
            generator = FindFirstObjectByType<StarSystemGenerator>();
        }
    }

    private void OnGUI()
    {
        if (inventory == null)
        {
            return;
        }

        string scannerState = scanner == null || scanner.IsReady ? "Ready" : "Charging";
        int seed = generator == null ? 0 : generator.seed;
        float speed = playerMovement == null ? 0f : playerMovement.Speed;
        GUI.Label(
            new Rect(12f, 12f, 560f, 44f),
            "Seed " + seed +
            "\nScanner " + scannerState +
            "  Speed " + speed.ToString("0.0"),
            labelStyle
        );

        DrawHudResources(new Rect(12f, 60f, 560f, 28f));
        DrawRadarSlot(new Rect(12f, 104f, 64f, 64f));
        DrawHealthBar(new Rect(84f, 104f, 220f, 28f));
        DrawDevControls(new Rect(84f, 140f, 180f, 28f));
        DrawRadarPointers();
        DrawCollisionWarnings();
    }

    public static float CalculateHealthFraction(float currentHealth, float maxHealth)
    {
        return maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
    }

    public static bool ShouldShowCollisionWarning(float speed, float threshold)
    {
        return speed >= Mathf.Max(0f, threshold);
    }

    public static float CollisionWarningSpeedThreshold(float safeImpactSpeed, float multiplier)
    {
        return Mathf.Max(0f, safeImpactSpeed) * Mathf.Max(0f, multiplier);
    }

    public static bool IsTrajectoryThreat(Vector2 playerPosition, Vector2 velocity, Vector2 targetPosition, float targetRadius, float maxDistance, float extraCorridor)
    {
        if (velocity.sqrMagnitude < 0.0001f || maxDistance <= 0f)
        {
            return false;
        }

        Vector2 toTarget = targetPosition - playerPosition;
        Vector2 heading = velocity.normalized;
        float forwardDistance = Vector2.Dot(toTarget, heading);
        if (forwardDistance <= 0f || forwardDistance > maxDistance)
        {
            return false;
        }

        float lateralMissDistance = Mathf.Abs((heading.x * toTarget.y) - (heading.y * toTarget.x));
        float dangerCorridor = Mathf.Max(Mathf.Max(0f, targetRadius) + Mathf.Max(0f, extraCorridor), 28f);
        return lateralMissDistance <= dangerCorridor;
    }

    public static bool IsWarningTarget(MapMarkerType markerType)
    {
        return markerType == MapMarkerType.Planet
            || markerType == MapMarkerType.Star
            || markerType == MapMarkerType.Asteroid;
    }

    public static int PrepareCollisionWarningsForRender(List<CollisionWarningCandidate> warnings, int maxWarnings)
    {
        if (warnings == null)
        {
            return 0;
        }

        warnings.Sort((left, right) => left.forwardDistance.CompareTo(right.forwardDistance));
        return Mathf.Min(Mathf.Max(0, maxWarnings), warnings.Count);
    }

    public static float DistanceUntilOnScreen(Vector2 cameraPosition, Vector2 targetPosition, float targetRadius, float cameraHalfWidth, float cameraHalfHeight)
    {
        Vector2 delta = targetPosition - cameraPosition;
        float radius = Mathf.Max(0f, targetRadius);
        float xDistance = Mathf.Max(0f, Mathf.Abs(delta.x) - Mathf.Max(0f, cameraHalfWidth) - radius);
        float yDistance = Mathf.Max(0f, Mathf.Abs(delta.y) - Mathf.Max(0f, cameraHalfHeight) - radius);
        return new Vector2(xDistance, yDistance).magnitude;
    }

    public static float CollisionWarningScaleForDistance(float distanceUntilOnScreen, float maxDistance, float minScale = 0.75f, float maxScale = 1.55f)
    {
        if (maxDistance <= 0f)
        {
            return distanceUntilOnScreen <= 0f ? maxScale : minScale;
        }

        float normalizedDistance = Mathf.Clamp01(Mathf.Max(0f, distanceUntilOnScreen) / maxDistance);
        return Mathf.SmoothStep(maxScale, minScale, normalizedDistance);
    }

    private void DrawHudResources(Rect rect)
    {
        DrawRect(rect, new Color(0f, 0f, 0f, 0.58f));
        ResourceGui.DrawAvailableResources(new Rect(rect.x + 6f, rect.y + 4f, rect.width - 12f, rect.height - 8f), smallLabelStyle);
    }

    private void DrawHealthBar(Rect rect)
    {
        if (playerHealth == null)
        {
            return;
        }

        float healthFraction = CalculateHealthFraction(playerHealth.currentHealth, playerHealth.maxHealth);
        Color fillColor = Color.Lerp(new Color(0.95f, 0.18f, 0.12f, 0.95f), new Color(0.25f, 1f, 0.48f, 0.95f), healthFraction);

        DrawRect(rect, new Color(0f, 0f, 0f, 0.76f));
        DrawRectOutline(rect, new Color(0.75f, 0.95f, 1f, 0.55f), 2f);
        DrawRect(new Rect(rect.x + 4f, rect.y + 4f, (rect.width - 8f) * healthFraction, rect.height - 8f), fillColor);

        GUI.Label(
            rect,
            "HULL " + Mathf.CeilToInt(playerHealth.currentHealth) + " / " + Mathf.CeilToInt(playerHealth.maxHealth),
            smallLabelStyle);
    }

    private void DrawDevControls(Rect rect)
    {
        if (!IsDevMode())
        {
            return;
        }

        DrawRect(rect, new Color(0f, 0f, 0f, 0.72f));
        DrawRectOutline(rect, new Color(1f, 0.4f, 0.24f, 0.8f), 2f);
        GUI.Label(rect, "PIRATES: BASES", smallLabelStyle);
    }

    private static bool IsDevMode()
    {
        GameModeRules rules = GameModeRuntime.ResolveActiveRules(SceneManager.GetActiveScene().name);
        return rules != null && rules.modeId == GameModeId.Dev;
    }

    private void DrawRadarSlot(Rect rect)
    {
        bool ready = radarPing == null || radarPing.IsReady;
        Color frameColor = ready ? new Color(0.2f, 0.95f, 1f, 0.95f) : new Color(0.18f, 0.36f, 0.42f, 0.95f);
        Color fillColor = ready ? new Color(0.04f, 0.24f, 0.3f, 0.92f) : new Color(0.03f, 0.08f, 0.1f, 0.92f);

        DrawRect(rect, new Color(0f, 0f, 0f, 0.76f));
        DrawRectOutline(rect, frameColor, 3f);
        DrawRect(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f), fillColor);

        if (radarPing != null && !ready)
        {
            float cooldownProgress = 1f - Mathf.Clamp01(radarPing.CooldownRemaining / Mathf.Max(0.01f, radarPing.cooldownSeconds));
            Rect fillRect = new Rect(rect.x + 6f, rect.yMax - 6f - ((rect.height - 12f) * cooldownProgress), rect.width - 12f, (rect.height - 12f) * cooldownProgress);
            DrawRect(fillRect, new Color(0.12f, 0.7f, 0.85f, 0.55f));
        }

        GUI.Label(new Rect(rect.x + 6f, rect.y + 4f, 18f, 18f), "1", smallLabelStyle);
        GUI.Label(new Rect(rect.x + 6f, rect.y + 23f, rect.width - 12f, 18f), "RAD", smallLabelStyle);
        GUI.Label(new Rect(rect.x + 6f, rect.y + 41f, rect.width - 12f, 18f), ready ? "RDY" : radarPing.CooldownRemaining.ToString("0.0"), smallLabelStyle);
    }

    private void DrawRadarPointers()
{
    if (radarPing == null || inventory == null)
    {
        return;
    }

    Vector2 planetPosition;
    Vector2 starPosition;

    System.Collections.Generic.IReadOnlyList<Vector2> asteroidPositions = radarPing.GetPointers(MapMarkerType.Asteroid);
    bool hasAsteroid = asteroidPositions.Count > 0;
    bool hasPlanet = radarPing.TryGetPointer(MapMarkerType.Planet, out planetPosition);
    bool hasStar = radarPing.TryGetPointer(MapMarkerType.Star, out starPosition);

    if (!hasAsteroid && !hasPlanet && !hasStar)
    {
        return;
    }

    float separation = 0f;

    if (hasAsteroid)
    {
        for (int i = 0; i < asteroidPositions.Count; i++)
        {
            DrawEdgePointer("ROCK", asteroidPositions[i], new Color(0.95f, 0.78f, 0.42f, 0.95f), separation);
            separation += 24f;
        }
    }

    if (hasPlanet)
    {
        DrawEdgePointer("PLANET", planetPosition, new Color(0.28f, 0.95f, 1f, 0.95f), separation);
        separation += 24f;
    }

    if (hasStar)
    {
        DrawEdgePointer("SUN", starPosition, new Color(1f, 0.76f, 0.22f, 0.95f), separation);
    }
}

private void DrawCollisionWarnings()
{
    if (inventory == null || playerMovement == null)
    {
        return;
    }

    float safeImpactSpeed = playerCrashDamage == null ? DefaultSafeImpactSpeed : playerCrashDamage.safeImpactSpeed;
    float speedThreshold = CollisionWarningSpeedThreshold(safeImpactSpeed, collisionWarningSpeedMultiplier);
    if (!ShouldShowCollisionWarning(playerMovement.Speed, speedThreshold))
    {
        return;
    }

    collisionWarningCandidates.Clear();
    Vector2 playerPosition = inventory.transform.position;
    Vector2 velocity = playerMovement.Velocity;
    Vector2 heading = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : Vector2.zero;
    MapMarker[] markers = FindObjectsByType<MapMarker>(FindObjectsSortMode.None);

    for (int i = 0; i < markers.Length; i++)
    {
        MapMarker marker = markers[i];
        if (marker == null || marker.transform == inventory.transform)
        {
            continue;
        }

        if (!marker.CanAppearOnMapAndRadar || !IsWarningTarget(marker.markerType))
        {
            continue;
        }

        Vector2 markerPosition = marker.transform.position;
        if (TryWorldToGuiPosition(markerPosition, out Vector2 guiPosition) && IsGuiPointOnScreen(guiPosition, 18f))
        {
            continue;
        }

        float targetRadius = ApproximateCollisionRadius(marker);
        if (!IsTrajectoryThreat(playerPosition, velocity, markerPosition, targetRadius, collisionWarningMaxDistance, collisionWarningExtraCorridor))
        {
            continue;
        }

        float forwardDistance = Vector2.Dot(markerPosition - playerPosition, heading);
        float distanceUntilVisible = DistanceUntilMarkerIsOnScreen(markerPosition, targetRadius);
        collisionWarningCandidates.Add(new CollisionWarningCandidate(marker, markerPosition, forwardDistance, distanceUntilVisible));
    }

    if (collisionWarningCandidates.Count == 0)
    {
        return;
    }

    int warningCount = PrepareCollisionWarningsForRender(collisionWarningCandidates, maxCollisionWarnings);
    float separation = 72f;
    Color warningColor = new Color(1f, 0.08f, 0.04f, 0.95f);

    for (int i = 0; i < warningCount; i++)
    {
        CollisionWarningCandidate warning = collisionWarningCandidates[i];
        float scale = CollisionWarningScaleForDistance(
            warning.distanceUntilOnScreen,
            collisionWarningMaxDistance,
            collisionWarningMinScale,
            collisionWarningMaxScale);
        DrawEdgeWarningPointer(warning.worldPosition, warningColor, separation, scale);
        separation += 24f * Mathf.Max(0.75f, scale);
    }
}

private float DistanceUntilMarkerIsOnScreen(Vector2 markerPosition, float targetRadius)
{
    Camera camera = Camera.main;
    if (camera == null || !camera.orthographic)
    {
        return Vector2.Distance(inventory.transform.position, markerPosition);
    }

    float halfHeight = camera.orthographicSize;
    float halfWidth = halfHeight * camera.aspect;
    return DistanceUntilOnScreen(camera.transform.position, markerPosition, targetRadius, halfWidth, halfHeight);
}

private static float ApproximateCollisionRadius(MapMarker marker)
{
    if (marker == null)
    {
        return 0f;
    }

    CircleCollider2D collider = marker.GetComponent<CircleCollider2D>();
    if (collider == null)
    {
        collider = marker.GetComponentInChildren<CircleCollider2D>();
    }

    if (collider == null)
    {
        return 0f;
    }

    Vector3 scale = collider.transform.lossyScale;
    float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
    return Mathf.Max(0f, collider.radius * largestScale);
}

   private void DrawEdgePointer(string label, Vector2 targetPosition, Color color, float separation)
{
    DrawEdgePointer(label, null, targetPosition, color, separation);
}

   private void DrawEdgePointer(string label, string detail, Vector2 targetPosition, Color color, float separation)
{
    Vector2 targetGuiPosition;
    Vector2 direction;

    if (TryWorldToGuiPosition(targetPosition, out targetGuiPosition))
    {
        if (IsGuiPointOnScreen(targetGuiPosition, 18f))
        {
            DrawOnScreenPointer(label, detail, targetGuiPosition, color);
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        direction = targetGuiPosition - screenCenter;
    }
    else
    {
        Vector2 worldDelta = targetPosition - (Vector2)inventory.transform.position;
        direction = new Vector2(worldDelta.x, -worldDelta.y);
    }

    if (direction.sqrMagnitude < 0.001f)
    {
        return;
    }

    direction.Normalize();

    Vector2 center = EdgePointerPosition(direction, Screen.width, Screen.height, 34f + separation);
    Vector2 tip = center + (direction * 20f);
    Vector2 left = tip - (direction * 8f) + new Vector2(-direction.y, direction.x) * 5f;
    Vector2 right = tip - (direction * 8f) - new Vector2(-direction.y, direction.x) * 5f;
    Rect labelRect = new Rect(center.x - 34f, center.y + 13f, 68f, string.IsNullOrEmpty(detail) ? 18f : 32f);

    Rect frameRect = new Rect(center.x - 28f, center.y - 18f, 56f, string.IsNullOrEmpty(detail) ? 52f : 66f);
    DrawRect(frameRect, new Color(0f, 0f, 0f, 0.52f));
    DrawRectOutline(frameRect, color, 2f);
    DrawLine(center - (direction * 10f), tip, color, 4f);
    DrawLine(tip, left, color, 4f);
    DrawLine(tip, right, color, 4f);
    GUI.Label(labelRect, string.IsNullOrEmpty(detail) ? label : label + "\n" + detail, smallLabelStyle);
}

private void DrawEdgeWarningPointer(Vector2 targetPosition, Color color, float separation, float scale)
{
    Vector2 targetGuiPosition;
    Vector2 direction;

    if (TryWorldToGuiPosition(targetPosition, out targetGuiPosition))
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        direction = targetGuiPosition - screenCenter;
    }
    else
    {
        Vector2 worldDelta = targetPosition - (Vector2)inventory.transform.position;
        direction = new Vector2(worldDelta.x, -worldDelta.y);
    }

    if (direction.sqrMagnitude < 0.001f)
    {
        return;
    }

    direction.Normalize();
    scale = Mathf.Max(0.01f, scale);

    Vector2 center = EdgePointerPosition(direction, Screen.width, Screen.height, 34f + separation);
    Vector2 tip = center + (direction * 20f * scale);
    Vector2 wingBase = tip - (direction * 8f * scale);
    Vector2 perpendicular = new Vector2(-direction.y, direction.x);
    Vector2 left = wingBase + (perpendicular * 5f * scale);
    Vector2 right = wingBase - (perpendicular * 5f * scale);
    Rect frameRect = new Rect(center.x - (28f * scale), center.y - (18f * scale), 56f * scale, 52f * scale);
    float lineWidth = Mathf.Max(1f, 4f * scale);

    DrawRect(frameRect, new Color(0f, 0f, 0f, 0.52f));
    DrawRectOutline(frameRect, color, Mathf.Max(1f, 2f * scale));
    DrawLine(center - (direction * 10f * scale), tip, color, lineWidth);
    DrawLine(tip, left, color, lineWidth);
    DrawLine(tip, right, color, lineWidth);
}

private bool TryWorldToGuiPosition(Vector2 worldPosition, out Vector2 guiPosition)
{
    guiPosition = Vector2.zero;

    Camera camera = Camera.main;
    if (camera == null)
    {
        return false;
    }

    Vector3 screenPosition = camera.WorldToScreenPoint(new Vector3(worldPosition.x, worldPosition.y, 0f));
    if (screenPosition.z < 0f)
    {
        return false;
    }

    guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
    return true;
}

private static bool IsGuiPointOnScreen(Vector2 guiPosition, float margin)
{
    return guiPosition.x >= margin &&
           guiPosition.x <= Screen.width - margin &&
           guiPosition.y >= margin &&
           guiPosition.y <= Screen.height - margin;
}

private void DrawOnScreenPointer(string label, Vector2 center, Color color)
{
    DrawOnScreenPointer(label, null, center, color);
}

private void DrawOnScreenPointer(string label, string detail, Vector2 center, Color color)
{
    float size = 28f;
    Rect frameRect = new Rect(center.x - (size * 0.5f), center.y - (size * 0.5f), size, size);
    Rect labelRect = new Rect(center.x - 34f, center.y - 48f, 68f, string.IsNullOrEmpty(detail) ? 18f : 32f);

    DrawRect(new Rect(center.x - 20f, center.y - 20f, 40f, 40f), new Color(0f, 0f, 0f, 0.38f));
    DrawRectOutline(frameRect, color, 2f);

    DrawLine(new Vector2(center.x - 16f, center.y), new Vector2(center.x - 6f, center.y), color, 3f);
    DrawLine(new Vector2(center.x + 6f, center.y), new Vector2(center.x + 16f, center.y), color, 3f);
    DrawLine(new Vector2(center.x, center.y - 16f), new Vector2(center.x, center.y - 6f), color, 3f);
    DrawLine(new Vector2(center.x, center.y + 6f), new Vector2(center.x, center.y + 16f), color, 3f);

    GUI.Label(labelRect, string.IsNullOrEmpty(detail) ? label : label + "\n" + detail, smallLabelStyle);
}

    public static Vector2 EdgePointerPosition(Vector2 direction, float screenWidth, float screenHeight, float margin)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return new Vector2(screenWidth * 0.5f, margin);
        }

        Vector2 normalized = direction.normalized;
        Vector2 center = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
        float halfWidth = Mathf.Max(1f, (screenWidth * 0.5f) - margin);
        float halfHeight = Mathf.Max(1f, (screenHeight * 0.5f) - margin);
        float scaleX = Mathf.Abs(normalized.x) > 0.001f ? halfWidth / Mathf.Abs(normalized.x) : float.MaxValue;
        float scaleY = Mathf.Abs(normalized.y) > 0.001f ? halfHeight / Mathf.Abs(normalized.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        return center + (normalized * scale);
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pixel);
        GUI.color = previous;
    }

    private void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float width)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;

        Vector2 delta = end - start;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - (width * 0.5f), delta.magnitude, width), pixel);

        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    public struct CollisionWarningCandidate
    {
        public readonly MapMarker marker;
        public readonly Vector2 worldPosition;
        public readonly float forwardDistance;
        public readonly float distanceUntilOnScreen;

        public CollisionWarningCandidate(MapMarker marker, Vector2 worldPosition, float forwardDistance)
            : this(marker, worldPosition, forwardDistance, forwardDistance)
        {
        }

        public CollisionWarningCandidate(MapMarker marker, Vector2 worldPosition, float forwardDistance, float distanceUntilOnScreen)
        {
            this.marker = marker;
            this.worldPosition = worldPosition;
            this.forwardDistance = forwardDistance;
            this.distanceUntilOnScreen = distanceUntilOnScreen;
        }
    }
}
