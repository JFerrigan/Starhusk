using System.Collections.Generic;
using UnityEngine;

public static class LogisticsRoutePlanner
{
    public static List<Vector2> BuildSegmentPath(
        Vector2 start,
        Vector2 end,
        Transform planet,
        float probeRadius,
        float clearance,
        Collider2D sourceCollider,
        Collider2D targetCollider)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(start);

        RaycastHit2D blockingHit;
        if (FindBlockingHit(start, end, planet, probeRadius, sourceCollider, targetCollider, out blockingHit))
        {
            Vector2 direction = (end - start).normalized;
            Vector2 midpoint = blockingHit.point;
            Vector2 centerToHit = planet == null
                ? new Vector2(-direction.y, direction.x)
                : (midpoint - (Vector2)planet.position).normalized;

            if (centerToHit.sqrMagnitude < 0.001f)
            {
                centerToHit = new Vector2(-direction.y, direction.x);
            }

            float blockerRadius = BlockingRadius(blockingHit.collider);
            points.Add(midpoint + centerToHit.normalized * (blockerRadius + clearance));
        }

        points.Add(end);
        return points;
    }

    public static bool SegmentBlocked(
        Vector2 start,
        Vector2 end,
        Transform planet,
        float probeRadius,
        Collider2D sourceCollider,
        Collider2D targetCollider)
    {
        RaycastHit2D hit;
        return FindBlockingHit(start, end, planet, probeRadius, sourceCollider, targetCollider, out hit);
    }

    private static bool FindBlockingHit(
        Vector2 start,
        Vector2 end,
        Transform planet,
        float probeRadius,
        Collider2D sourceCollider,
        Collider2D targetCollider,
        out RaycastHit2D blockingHit)
    {
        blockingHit = default;
        Vector2 delta = end - start;
        float distance = delta.magnitude;
        if (distance <= 0.01f)
        {
            return false;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, probeRadius, delta.normalized, distance);
        float closestDistance = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D candidate = hits[i].collider;
            if (ShouldIgnore(candidate, planet, sourceCollider, targetCollider))
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

    private static bool ShouldIgnore(Collider2D candidate, Transform planet, Collider2D sourceCollider, Collider2D targetCollider)
    {
        if (candidate == null || candidate.isTrigger)
        {
            return true;
        }

        if (candidate == sourceCollider || candidate == targetCollider)
        {
            return true;
        }

        return planet != null && candidate.transform.IsChildOf(planet);
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
}
