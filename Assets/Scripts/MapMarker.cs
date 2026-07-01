using UnityEngine;

public enum MapMarkerType
{
    Player,
    Star,
    Planet,
    Asteroid
}

public class MapMarker : MonoBehaviour
{
    public MapMarkerType markerType = MapMarkerType.Asteroid;
    public Color markerColor = Color.white;
    public float iconScale = 1f;
    public bool requireDiscovery = true;
    public DiscoveryState discoveryState;

    public bool IsVisible
    {
        get
        {
            return !requireDiscovery || discoveryState == null || discoveryState.discovered;
        }
    }

    private void Awake()
    {
        if (discoveryState == null)
        {
            discoveryState = GetComponent<DiscoveryState>();
        }
    }
}
