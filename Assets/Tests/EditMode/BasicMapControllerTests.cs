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
