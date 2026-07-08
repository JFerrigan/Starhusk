using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAutopilotController : MonoBehaviour
{
    private const float FallbackThrustForce = 8f;
    private const float FallbackBrakeDeceleration = 28f;
    private const float FallbackSimpleAcceleration = 8f;
    private const float FallbackSimpleStopDeceleration = 28f;
    private const float ArrivalBrakeSafetyMultiplier = 1.15f;

    public float arrivalRadius = 3f;
    public float waypointArrivalRadius = 4f;
    public float slowdownRadius = 18f;
    public float steeringResponsiveness = 6f;
    public float routeProbeRadius = 1.5f;
    public float obstacleClearance = 5f;
    public int maxRouteIterations = 8;

    private readonly List<Vector2> waypoints = new List<Vector2>();
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private Collider2D[] ownColliders;
    private Vector2 destination;
    private int waypointIndex;
    private bool hasDestination;
    private bool routePartial;

    public bool HasDestination => hasDestination;
    public Vector2 Destination => destination;
    public IReadOnlyList<Vector2> Waypoints => waypoints;
    public bool RoutePartial => routePartial;

    private void Awake()
    {
        CacheComponents();
    }

    private void FixedUpdate()
    {
        if (!hasDestination)
        {
            return;
        }

        CacheComponents();

        if (movement != null && movement.HasManualMovementInput)
        {
            CancelAutopilot();
            return;
        }

        Vector2 position = rb.position;
        if (Vector2.Distance(position, destination) <= Mathf.Max(0.1f, arrivalRadius))
        {
            CancelAutopilot();
            return;
        }

        if (waypoints.Count == 0)
        {
            RebuildRoute(position, destination);
        }

        waypointIndex = Mathf.Clamp(waypointIndex, 0, Mathf.Max(0, waypoints.Count - 1));
        while (waypointIndex < waypoints.Count - 1 && Vector2.Distance(position, waypoints[waypointIndex]) <= Mathf.Max(0.1f, waypointArrivalRadius))
        {
            waypointIndex++;
        }

        Vector2 target = waypoints[Mathf.Min(waypointIndex, waypoints.Count - 1)];
        if (SegmentBlocked(position, target, out _) && waypointIndex < waypoints.Count)
        {
            RebuildRoute(position, destination);
            target = waypoints[Mathf.Min(waypointIndex, waypoints.Count - 1)];
        }

        Vector2 toTarget = target - position;
        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float desiredAngle = (Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg) - 90f;
        float nextAngle = Mathf.LerpAngle(rb.rotation, desiredAngle, Mathf.Clamp01(steeringResponsiveness * Time.fixedDeltaTime));
        rb.MoveRotation(nextAngle);

        Vector2 forward = ForwardFromRotation(nextAngle);
        if (GameSettings.MovementControl == MovementControlType.Simple)
        {
            rb.linearVelocity = PlayerMovement.CalculateSimpleVelocity(
                rb.linearVelocity,
                forward,
                ShouldBrakeForArrival(position, SimpleStopDeceleration) ? 0f : 1f,
                SimpleAcceleration,
                SimpleStopDeceleration,
                Time.fixedDeltaTime);
        }
        else if (ShouldBrakeForArrival(position, BrakeDeceleration))
        {
            rb.linearVelocity = Vector2.MoveTowards(
                rb.linearVelocity,
                Vector2.zero,
                BrakeDeceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.AddForce(forward * ThrustForce);
        }
    }

    public void SetDestination(Vector2 worldDestination)
    {
        CacheComponents();
        hasDestination = true;
        RebuildRoute(rb == null ? (Vector2)transform.position : rb.position, worldDestination);
    }

    public void CancelAutopilot()
    {
        hasDestination = false;
        routePartial = false;
        waypointIndex = 0;
        waypoints.Clear();
    }

    private void CacheComponents()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (ownColliders == null || ownColliders.Length == 0)
        {
            ownColliders = GetComponentsInChildren<Collider2D>();
        }
    }

    private void RebuildRoute(Vector2 start, Vector2 requestedDestination)
    {
        destination = ResolveSafeDestination(start, requestedDestination);
        waypoints.Clear();
        waypoints.Add(start);
        waypointIndex = 1;
        routePartial = false;

        Vector2 current = start;
        int iterations = Mathf.Max(1, maxRouteIterations);
        for (int i = 0; i < iterations; i++)
        {
            RaycastHit2D hit;
            if (!SegmentBlocked(current, destination, out hit))
            {
                AddWaypoint(destination);
                return;
            }

            Vector2 waypoint = BuildDetourWaypoint(current, destination, hit.collider, hit.point);
            AddWaypoint(waypoint);
            current = waypoint;
        }

        routePartial = true;
        AddWaypoint(destination);
    }

    private Vector2 ResolveSafeDestination(Vector2 start, Vector2 requestedDestination)
    {
        Vector2 result = requestedDestination;
        float probe = Mathf.Max(0.01f, routeProbeRadius + obstacleClearance);
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(requestedDestination, probe);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider2D blocker = overlaps[i];
            if (ShouldIgnore(blocker))
            {
                continue;
            }

            Vector2 center = blocker.bounds.center;
            Vector2 direction = result - center;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = requestedDestination - start;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.up;
            }

            float safeDistance = BlockingRadius(blocker) + Mathf.Max(0f, obstacleClearance) + Mathf.Max(0f, routeProbeRadius);
            result = center + (direction.normalized * safeDistance);
        }

        return result;
    }

    private bool SegmentBlocked(Vector2 start, Vector2 end, out RaycastHit2D blockingHit)
    {
        blockingHit = default;
        Vector2 delta = end - start;
        float distance = delta.magnitude;
        if (distance <= 0.01f)
        {
            return false;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, Mathf.Max(0.01f, routeProbeRadius), delta.normalized, distance);
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D candidate = hits[i].collider;
            if (ShouldIgnore(candidate))
            {
                continue;
            }

            if (hits[i].distance < closestDistance)
            {
                closestDistance = hits[i].distance;
                blockingHit = hits[i];
            }
        }

        return blockingHit.collider != null;
    }

    private Vector2 BuildDetourWaypoint(Vector2 start, Vector2 end, Collider2D blocker, Vector2 hitPoint)
    {
        Vector2 center = blocker == null ? hitPoint : (Vector2)blocker.bounds.center;
        Vector2 direction = (end - start).sqrMagnitude > 0.0001f ? (end - start).normalized : Vector2.up;
        Vector2 centerToHit = hitPoint - center;
        if (centerToHit.sqrMagnitude < 0.0001f)
        {
            centerToHit = new Vector2(-direction.y, direction.x);
        }

        Vector2 clockwise = new Vector2(centerToHit.y, -centerToHit.x).normalized;
        Vector2 counterClockwise = new Vector2(-centerToHit.y, centerToHit.x).normalized;
        float safeDistance = BlockingRadius(blocker) + Mathf.Max(0f, obstacleClearance) + Mathf.Max(0f, routeProbeRadius);
        Vector2 waypointA = center + ((centerToHit.normalized + clockwise).normalized * safeDistance);
        Vector2 waypointB = center + ((centerToHit.normalized + counterClockwise).normalized * safeDistance);

        return Vector2.Distance(waypointA, end) <= Vector2.Distance(waypointB, end) ? waypointA : waypointB;
    }

    private void AddWaypoint(Vector2 point)
    {
        if (waypoints.Count == 0 || Vector2.Distance(waypoints[waypoints.Count - 1], point) > 0.01f)
        {
            waypoints.Add(point);
        }
    }

    private bool ShouldIgnore(Collider2D candidate)
    {
        if (candidate == null || candidate.isTrigger)
        {
            return true;
        }

        if (ownColliders != null)
        {
            for (int i = 0; i < ownColliders.Length; i++)
            {
                if (candidate == ownColliders[i])
                {
                    return true;
                }
            }
        }

        return candidate.transform.IsChildOf(transform);
    }

    private static float BlockingRadius(Collider2D collider)
    {
        if (collider == null)
        {
            return 0f;
        }

        Vector2 extents = collider.bounds.extents;
        return Mathf.Max(extents.x, extents.y);
    }

    private bool ShouldBrakeForArrival(Vector2 position, float deceleration)
    {
        if (deceleration <= 0.0001f)
        {
            return false;
        }

        float destinationDistance = Vector2.Distance(position, destination);
        float speed = rb == null ? 0f : rb.linearVelocity.magnitude;
        float stoppingDistance = (speed * speed) / (2f * deceleration);
        float brakeBuffer = stoppingDistance * (ArrivalBrakeSafetyMultiplier - 1f);
        float fixedStepTravel = speed * Time.fixedDeltaTime;
        float brakeDistance = stoppingDistance + brakeBuffer + fixedStepTravel + Mathf.Max(0.1f, arrivalRadius);
        return destinationDistance <= brakeDistance;
    }

    private float ThrustForce => movement == null ? FallbackThrustForce : movement.thrustForce;
    private float BrakeDeceleration => movement == null ? FallbackBrakeDeceleration : movement.brakeDeceleration;
    private float SimpleAcceleration => movement == null ? FallbackSimpleAcceleration : movement.simpleAcceleration;
    private float SimpleStopDeceleration => movement == null ? FallbackSimpleStopDeceleration : movement.simpleStopDeceleration;

    private static Vector2 ForwardFromRotation(float rotation)
    {
        float radians = (rotation + 90f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
}
