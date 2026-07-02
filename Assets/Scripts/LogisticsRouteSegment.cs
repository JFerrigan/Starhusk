using UnityEngine;

public struct LogisticsRouteSegment
{
    public Vector2 start;
    public Vector2 end;

    public LogisticsRouteSegment(Vector2 start, Vector2 end)
    {
        this.start = start;
        this.end = end;
    }
}
