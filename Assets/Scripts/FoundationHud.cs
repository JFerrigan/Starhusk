using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class FoundationHud : MonoBehaviour
{
    private const float DefaultSafeImpactSpeed = 8f;

    private ResourceInventory inventory;
    private PlayerMovement playerMovement;
    private PlayerRadarPing radarPing;
    private ShipHealth playerHealth;
    private ShipCrashDamage playerCrashDamage;
    private PlayerUpgradeState playerUpgradeState;
    private Texture2D pixel;
    private GUIStyle labelStyle;
    private GUIStyle smallLabelStyle;
    private float styleScale;
    private readonly List<CollisionWarningCandidate> collisionWarningCandidates = new List<CollisionWarningCandidate>();

    [Header("Collision Warnings")]
    public float collisionWarningSpeedMultiplier = 4.5f;
    public float collisionWarningMaxDistance = 650f;
    public float collisionWarningDangerDistance = 160f;
    public float collisionWarningExtraCorridor = 18f;
    public float collisionWarningMinScale = 0.75f;
    public float collisionWarningMaxScale = 1.55f;
    public int maxCollisionWarnings = 3;

    private void Awake()
    {
        pixel = Texture2D.whiteTexture;
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<ResourceInventory>();
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

        if (playerUpgradeState == null && inventory != null)
        {
            playerUpgradeState = inventory.GetComponent<PlayerUpgradeState>();
            if (playerUpgradeState == null)
            {
                playerUpgradeState = inventory.gameObject.AddComponent<PlayerUpgradeState>();
            }
        }

    }

    private void OnGUI()
    {
        if (inventory == null)
        {
            return;
        }

        EnsureStyles();
        float scale = GameUiScale.Current;

        float resourceWidth = Mathf.Min(GameUiScale.Size(760f, scale), Screen.width - GameUiScale.Size(24f, scale));
        float resourceHeight = GameUiScale.Size(32f, scale);
        DrawHudResources(new Rect((Screen.width - resourceWidth) * 0.5f, Screen.height - resourceHeight - GameUiScale.Size(14f, scale), resourceWidth, resourceHeight), scale);
        DrawHealthBar(new Rect(GameUiScale.Size(12f, scale), GameUiScale.Size(12f, scale), GameUiScale.Size(220f, scale), GameUiScale.Size(30f, scale)), scale);
        DrawDevControls(new Rect(GameUiScale.Size(12f, scale), GameUiScale.Size(50f, scale), GameUiScale.Size(260f, scale), GameUiScale.Size(286f, scale)), scale);
        DrawRadarPointers();
        DrawCollisionWarnings();
    }

    private void EnsureStyles()
    {
        float scale = GameUiScale.Current;
        if (labelStyle != null && Mathf.Approximately(styleScale, scale))
        {
            return;
        }

        styleScale = scale;
        labelStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(16f, scale),
            normal = { textColor = Color.white }
        };
        smallLabelStyle = new GUIStyle
        {
            fontSize = GameUiScale.Font(12f, scale),
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };
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

    public static Color CollisionWarningColorForDistance(float distanceUntilOnScreen, float dangerDistance)
    {
        Color caution = new Color(1f, 0.72f, 0.16f, 0.95f);
        Color danger = new Color(1f, 0.08f, 0.04f, 0.95f);
        if (dangerDistance <= 0f)
        {
            return distanceUntilOnScreen <= 0f ? danger : caution;
        }

        float dangerAmount = 1f - Mathf.Clamp01(Mathf.Max(0f, distanceUntilOnScreen) / dangerDistance);
        return Color.Lerp(caution, danger, dangerAmount);
    }

    private void DrawHudResources(Rect rect, float scale)
    {
        DrawRect(rect, new Color(0f, 0f, 0f, 0.58f));
        ResourceGui.DrawAvailableResources(
            new Rect(rect.x + GameUiScale.Size(6f, scale), rect.y + GameUiScale.Size(4f, scale), rect.width - GameUiScale.Size(12f, scale), rect.height - GameUiScale.Size(8f, scale)),
            smallLabelStyle,
            GameUiScale.Size(16f, scale),
            GameUiScale.Size(64f, scale),
            GameUiScale.Size(8f, scale));
    }

    private void DrawHealthBar(Rect rect, float scale)
    {
        if (playerHealth == null)
        {
            return;
        }

        float healthFraction = CalculateHealthFraction(playerHealth.currentHealth, playerHealth.maxHealth);
        Color fillColor = Color.Lerp(new Color(0.95f, 0.18f, 0.12f, 0.95f), new Color(0.25f, 1f, 0.48f, 0.95f), healthFraction);

        DrawRect(rect, new Color(0f, 0f, 0f, 0.76f));
        DrawRectOutline(rect, new Color(0.75f, 0.95f, 1f, 0.55f), GameUiScale.Size(2f, scale));
        DrawRect(new Rect(rect.x + GameUiScale.Size(4f, scale), rect.y + GameUiScale.Size(4f, scale), (rect.width - GameUiScale.Size(8f, scale)) * healthFraction, rect.height - GameUiScale.Size(8f, scale)), fillColor);

        GUI.Label(
            rect,
            "HULL " + Mathf.CeilToInt(playerHealth.currentHealth) + " / " + Mathf.CeilToInt(playerHealth.maxHealth),
            smallLabelStyle);
    }

    private void DrawDevControls(Rect rect, float scale)
    {
        if (!IsDevMode() || playerUpgradeState == null)
        {
            return;
        }

        DrawRect(rect, new Color(0f, 0f, 0f, 0.72f));
        DrawRectOutline(rect, new Color(1f, 0.4f, 0.24f, 0.8f), GameUiScale.Size(2f, scale));
        GUI.Label(new Rect(rect.x, rect.y + GameUiScale.Size(4f, scale), rect.width, GameUiScale.Size(18f, scale)), "DEV UPGRADES", smallLabelStyle);

        IReadOnlyList<UpgradeId> upgradeIds = PlayerUpgradeState.AllUpgradeIds;
        float rowHeight = GameUiScale.Size(22f, scale);
        for (int i = 0; i < upgradeIds.Count; i++)
        {
            UpgradeId upgradeId = upgradeIds[i];
            Rect rowRect = new Rect(
                rect.x + GameUiScale.Size(8f, scale),
                rect.y + GameUiScale.Size(28f, scale) + (rowHeight * i),
                rect.width - GameUiScale.Size(16f, scale),
                rowHeight);

            bool unlocked = playerUpgradeState.IsUnlocked(upgradeId);
            bool nextUnlocked = GUI.Toggle(rowRect, unlocked, UpgradeLabel(upgradeId), smallLabelStyle);
            if (nextUnlocked != unlocked)
            {
                playerUpgradeState.SetUnlocked(upgradeId, nextUnlocked);
            }
        }
    }

    private static string UpgradeLabel(UpgradeId upgradeId)
    {
        switch (upgradeId)
        {
            case UpgradeId.Ping3Asteroids:
                return "Ping 3 Asteroids";
            case UpgradeId.PingAsteroidResourceType:
                return "Ping Resource Type";
            case UpgradeId.Ping10Asteroids:
                return "Ping 10 Asteroids";
            case UpgradeId.TripleShotProjectiles:
                return "Triple Shot";
            case UpgradeId.HomingProjectiles:
                return "Homing Projectiles";
            case UpgradeId.AsteroidAnnihilator:
                return "Asteroid Annihilator";
            case UpgradeId.AutopilotUnlock:
                return "Autopilot";
            case UpgradeId.ImpactShield:
                return "Impact Shield";
            case UpgradeId.AsteroidCarverHull:
                return "Asteroid Carver Hull";
            case UpgradeId.InfiniteRadarRange:
                return "Infinite Radar";
            case UpgradeId.PersistentRadarDiscovery:
                return "Persistent Radar";
            default:
                return upgradeId.ToString();
        }
    }

    private static bool IsDevMode()
    {
        GameModeRules rules = GameModeRuntime.ResolveActiveRules(SceneManager.GetActiveScene().name);
        return rules != null && rules.modeId == GameModeId.Dev;
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
            separation += GameUiScale.Size(24f);
        }
    }

    if (hasPlanet)
    {
        DrawEdgePointer("PLANET", planetPosition, new Color(0.28f, 0.95f, 1f, 0.95f), separation);
        separation += GameUiScale.Size(24f);
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
        if (TryWorldToGuiPosition(markerPosition, out Vector2 guiPosition) && IsGuiPointOnScreen(guiPosition, GameUiScale.Size(18f)))
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
    float separation = GameUiScale.Size(72f);

    for (int i = 0; i < warningCount; i++)
    {
        CollisionWarningCandidate warning = collisionWarningCandidates[i];
        float scale = CollisionWarningScaleForDistance(
            warning.distanceUntilOnScreen,
            collisionWarningMaxDistance,
            collisionWarningMinScale,
            collisionWarningMaxScale);
        Color warningColor = CollisionWarningColorForDistance(warning.distanceUntilOnScreen, collisionWarningDangerDistance);
        DrawEdgeWarningPointer(warning.worldPosition, warningColor, separation, scale);
        separation += GameUiScale.Size(24f) * Mathf.Max(0.75f, scale);
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
        if (IsGuiPointOnScreen(targetGuiPosition, GameUiScale.Size(18f)))
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

    float uiScale = GameUiScale.Current;
    Vector2 center = EdgePointerPosition(direction, Screen.width, Screen.height, GameUiScale.Size(34f, uiScale) + separation);
    Vector2 tip = center + (direction * GameUiScale.Size(20f, uiScale));
    Vector2 left = tip - (direction * GameUiScale.Size(8f, uiScale)) + new Vector2(-direction.y, direction.x) * GameUiScale.Size(5f, uiScale);
    Vector2 right = tip - (direction * GameUiScale.Size(8f, uiScale)) - new Vector2(-direction.y, direction.x) * GameUiScale.Size(5f, uiScale);
    Rect labelRect = new Rect(center.x - GameUiScale.Size(34f, uiScale), center.y + GameUiScale.Size(13f, uiScale), GameUiScale.Size(68f, uiScale), string.IsNullOrEmpty(detail) ? GameUiScale.Size(18f, uiScale) : GameUiScale.Size(32f, uiScale));

    Rect frameRect = new Rect(center.x - GameUiScale.Size(28f, uiScale), center.y - GameUiScale.Size(18f, uiScale), GameUiScale.Size(56f, uiScale), string.IsNullOrEmpty(detail) ? GameUiScale.Size(52f, uiScale) : GameUiScale.Size(66f, uiScale));
    DrawRect(frameRect, new Color(0f, 0f, 0f, 0.52f));
    DrawRectOutline(frameRect, color, GameUiScale.Size(2f, uiScale));
    DrawLine(center - (direction * GameUiScale.Size(10f, uiScale)), tip, color, GameUiScale.Size(4f, uiScale));
    DrawLine(tip, left, color, GameUiScale.Size(4f, uiScale));
    DrawLine(tip, right, color, GameUiScale.Size(4f, uiScale));
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
    float uiScale = GameUiScale.Current;
    scale = Mathf.Max(0.01f, scale) * uiScale;

    Vector2 center = EdgePointerPosition(direction, Screen.width, Screen.height, GameUiScale.Size(34f, uiScale) + separation);
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
    float uiScale = GameUiScale.Current;
    float size = GameUiScale.Size(28f, uiScale);
    Rect frameRect = new Rect(center.x - (size * 0.5f), center.y - (size * 0.5f), size, size);
    Rect labelRect = new Rect(center.x - GameUiScale.Size(34f, uiScale), center.y - GameUiScale.Size(48f, uiScale), GameUiScale.Size(68f, uiScale), string.IsNullOrEmpty(detail) ? GameUiScale.Size(18f, uiScale) : GameUiScale.Size(32f, uiScale));

    DrawRect(new Rect(center.x - GameUiScale.Size(20f, uiScale), center.y - GameUiScale.Size(20f, uiScale), GameUiScale.Size(40f, uiScale), GameUiScale.Size(40f, uiScale)), new Color(0f, 0f, 0f, 0.38f));
    DrawRectOutline(frameRect, color, GameUiScale.Size(2f, uiScale));

    DrawLine(new Vector2(center.x - GameUiScale.Size(16f, uiScale), center.y), new Vector2(center.x - GameUiScale.Size(6f, uiScale), center.y), color, GameUiScale.Size(3f, uiScale));
    DrawLine(new Vector2(center.x + GameUiScale.Size(6f, uiScale), center.y), new Vector2(center.x + GameUiScale.Size(16f, uiScale), center.y), color, GameUiScale.Size(3f, uiScale));
    DrawLine(new Vector2(center.x, center.y - GameUiScale.Size(16f, uiScale)), new Vector2(center.x, center.y - GameUiScale.Size(6f, uiScale)), color, GameUiScale.Size(3f, uiScale));
    DrawLine(new Vector2(center.x, center.y + GameUiScale.Size(6f, uiScale)), new Vector2(center.x, center.y + GameUiScale.Size(16f, uiScale)), color, GameUiScale.Size(3f, uiScale));

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
