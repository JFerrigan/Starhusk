#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class BasicMapControllerTests
{
    [Test]
    public void WorldCenterMapsToRectCenter()
    {
        Rect rect = new Rect(10f, 20f, 100f, 100f);

        Vector2 mapPosition = BasicMapController.WorldToMapPosition(Vector2.zero, rect, 80f);

        Assert.That(mapPosition.x, Is.EqualTo(rect.center.x).Within(0.001f));
        Assert.That(mapPosition.y, Is.EqualTo(rect.center.y).Within(0.001f));
    }

    [Test]
    public void PositiveWorldXMapsToRightEdge()
    {
        Rect rect = new Rect(0f, 0f, 200f, 200f);

        Vector2 mapPosition = BasicMapController.WorldToMapPosition(new Vector2(80f, 0f), rect, 80f);

        Assert.That(mapPosition.x, Is.EqualTo(rect.xMax).Within(0.001f));
        Assert.That(mapPosition.y, Is.EqualTo(rect.center.y).Within(0.001f));
    }

    [Test]
    public void PositiveWorldYMapsTowardTopEdge()
    {
        Rect rect = new Rect(0f, 0f, 200f, 200f);

        Vector2 mapPosition = BasicMapController.WorldToMapPosition(new Vector2(0f, 80f), rect, 80f);

        Assert.That(mapPosition.x, Is.EqualTo(rect.center.x).Within(0.001f));
        Assert.That(mapPosition.y, Is.EqualTo(rect.yMin).Within(0.001f));
    }

    [Test]
    public void MapToWorldPositionInvertsCenterRightAndTopMappings()
    {
        Rect rect = new Rect(10f, 20f, 200f, 200f);
        float radius = 80f;

        Vector2 center = BasicMapController.MapToWorldPosition(rect.center, rect, radius);
        Vector2 right = BasicMapController.MapToWorldPosition(new Vector2(rect.xMax, rect.center.y), rect, radius);
        Vector2 top = BasicMapController.MapToWorldPosition(new Vector2(rect.center.x, rect.yMin), rect, radius);

        Assert.That(center.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(center.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(right.x, Is.EqualTo(radius).Within(0.001f));
        Assert.That(right.y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(top.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(top.y, Is.EqualTo(radius).Within(0.001f));
    }

    [Test]
    public void FullMapClickConversionProducesExpectedWorldDestination()
    {
        Rect rect = new Rect(50f, 60f, 400f, 400f);

        bool converted = BasicMapController.TryGetAutopilotDestinationFromMapClick(
            new Vector2(rect.center.x + 100f, rect.center.y - 50f),
            rect,
            200f,
            out Vector2 destination);

        Assert.IsTrue(converted);
        Assert.That(destination.x, Is.EqualTo(100f).Within(0.001f));
        Assert.That(destination.y, Is.EqualTo(50f).Within(0.001f));
    }

    [Test]
    public void FullMapClickConversionIgnoresHintArea()
    {
        Rect rect = new Rect(50f, 60f, 400f, 400f);

        bool converted = BasicMapController.TryGetAutopilotDestinationFromMapClick(
            new Vector2(rect.x + 20f, rect.y + 20f),
            rect,
            200f,
            out _);

        Assert.IsFalse(converted);
    }

    [Test]
    public void PlayerMarkerIsVisibleWithoutDiscovery()
    {
        GameObject markerObject = new GameObject("Player Marker");

        try
        {
            MapMarker marker = markerObject.AddComponent<MapMarker>();
            marker.markerType = MapMarkerType.Player;
            marker.requireDiscovery = false;

            Assert.IsTrue(marker.IsVisible);
        }
        finally
        {
            Object.DestroyImmediate(markerObject);
        }
    }

    [Test]
    public void DiscoveredMarkerVisibilityFollowsDiscoveryState()
    {
        GameObject markerObject = new GameObject("Planet Marker");

        try
        {
            DiscoveryState discovery = markerObject.AddComponent<DiscoveryState>();
            MapMarker marker = markerObject.AddComponent<MapMarker>();
            marker.requireDiscovery = true;
            marker.discoveryState = discovery;

            discovery.SetDiscovered(false);
            Assert.IsFalse(marker.IsVisible);

            discovery.SetDiscovered(true);
            Assert.IsTrue(marker.IsVisible);
        }
        finally
        {
            Object.DestroyImmediate(markerObject);
        }
    }
}
#endif
