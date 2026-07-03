using System.Collections.Generic;
using UnityEngine;

public struct ActiveDysonBeam
{
    public DysonSatellite dynamicSatellite;
    public DysonSatellite stationarySatellite;
    public Vector2 DynamicPosition;
    public Vector2 ReceiverPosition;

    public ActiveDysonBeam(DysonSatellite dynamicSatellite, DysonSatellite stationarySatellite, Vector2 dynamicPosition, Vector2 receiverPosition)
    {
        this.dynamicSatellite = dynamicSatellite;
        this.stationarySatellite = stationarySatellite;
        DynamicPosition = dynamicPosition;
        ReceiverPosition = receiverPosition;
    }
}

public class DysonBeamNetwork : MonoBehaviour
{
    public float alignmentToleranceDegrees = 35f;
    public float beamWidth = 0.18f;
    public float beamUpdateInterval = 0.05f;
    public bool includeSceneSatellites = true;
    private float nextBeamUpdateTime;
    
    public Color beamColor = new Color(1f, 0.92f, 0.35f, 0.82f);

    private DysonSatellite[] dynamicSatellites;
    private DysonSatellite[] stationarySatellites;
    private LineRenderer[] beams;
    private readonly List<ActiveDysonBeam> activeAlignments = new List<ActiveDysonBeam>();

    public IReadOnlyList<ActiveDysonBeam> ActiveAlignments => activeAlignments;
    public int ActiveBeamCount => activeAlignments.Count;

    public void Initialize(DysonSatellite[] dynamicSatellites, DysonSatellite[] stationarySatellites)
    {
        this.dynamicSatellites = dynamicSatellites;
        this.stationarySatellites = stationarySatellites;
        CreateBeamRenderers(Mathf.Max(1, dynamicSatellites.Length));
    }

    private void LateUpdate()
    {
        if (dynamicSatellites == null || stationarySatellites == null || beams == null)
        {
            return;
        }

        if (Time.time < nextBeamUpdateTime)
        {
            return;
        }

        nextBeamUpdateTime = Time.time + beamUpdateInterval;

        RebuildActiveAlignments();
        EnsureBeamCount(Mathf.Max(1, DynamicSatelliteCount()));
        DrawActiveBeams();

    }

    public static bool AreAligned(Vector2 sunPosition, Vector2 dynamicPosition, Vector2 stationaryPosition, float toleranceDegrees)
    {
        float dynamicAngle = DysonSatellite.AngleFromSun(sunPosition, dynamicPosition);
        float stationaryAngle = DysonSatellite.AngleFromSun(sunPosition, stationaryPosition);
        return Mathf.Abs(Mathf.DeltaAngle(dynamicAngle, stationaryAngle)) <= toleranceDegrees;
    }

    private void CreateBeamRenderers(int count)
{
    beams = new LineRenderer[count];

    Shader shader = Shader.Find("Sprites/Default");
    if (shader == null)
    {
        shader = Shader.Find("Universal Render Pipeline/Unlit");
    }

    Material sharedBeamMaterial = new Material(shader);

    for (int i = 0; i < beams.Length; i++)
    {
        GameObject beamObject = new GameObject("Dyson Reflection Beam " + i);
        beamObject.transform.SetParent(transform, false);

        LineRenderer beam = beamObject.AddComponent<LineRenderer>();
        beam.useWorldSpace = true;
        beam.positionCount = 2;
        beam.widthMultiplier = beamWidth;
        beam.sortingOrder = 450;
        beam.sharedMaterial = sharedBeamMaterial;
        beam.startColor = beamColor;
        beam.endColor = new Color(1f, 1f, 0.78f, 0.62f);
        beam.enabled = false;

        beams[i] = beam;
    }
}

    private void EnsureBeamCount(int count)
    {
        if (beams != null && beams.Length == count)
        {
            return;
        }

        if (beams != null)
        {
            for (int i = 0; i < beams.Length; i++)
            {
                if (beams[i] != null)
                {
                    Destroy(beams[i].gameObject);
                }
            }
        }

        CreateBeamRenderers(count);
    }

    private void RebuildActiveAlignments()
    {
        activeAlignments.Clear();
        RefreshSceneSatelliteCache();
        BuildActiveAlignments(dynamicSatellites, stationarySatellites, alignmentToleranceDegrees, activeAlignments);
    }

    private void RefreshSceneSatelliteCache()
    {
        if (!includeSceneSatellites)
        {
            return;
        }

        DysonSatellite[] satellites = FindObjectsByType<DysonSatellite>(FindObjectsSortMode.None);
        System.Array.Sort(satellites, CompareSatellites);

        List<DysonSatellite> dynamicList = new List<DysonSatellite>();
        List<DysonSatellite> stationaryList = new List<DysonSatellite>();

        for (int i = 0; i < satellites.Length; i++)
        {
            DysonSatellite satellite = satellites[i];
            if (satellite == null)
            {
                continue;
            }

            if (satellite.IsDynamic)
            {
                dynamicList.Add(satellite);
            }
            else
            {
                stationaryList.Add(satellite);
            }
        }

        dynamicSatellites = dynamicList.ToArray();
        stationarySatellites = stationaryList.ToArray();
    }

    private int DynamicSatelliteCount()
    {
        return dynamicSatellites == null ? 0 : dynamicSatellites.Length;
    }

    private void DrawActiveBeams()
    {
        int beamIndex = 0;
        for (; beamIndex < activeAlignments.Count && beamIndex < beams.Length; beamIndex++)
        {
            ActiveDysonBeam alignment = activeAlignments[beamIndex];
            SetBeam(beams[beamIndex], alignment.DynamicPosition, alignment.ReceiverPosition, true);
        }

        for (int i = beamIndex; i < beams.Length; i++)
        {
            beams[i].enabled = false;
        }
    }

    private static void SetBeam(LineRenderer beam, Vector2 start, Vector2 end, bool enabled)
    {
        beam.enabled = enabled;
        if (!enabled)
        {
            return;
        }

        beam.SetPosition(0, start);
        beam.SetPosition(1, end);
    }

    public static IReadOnlyList<ActiveDysonBeam> FindActiveAlignmentsInScene(float toleranceDegrees = 35f)
    {
        DysonSatellite[] satellites = FindObjectsByType<DysonSatellite>(FindObjectsSortMode.None);
        System.Array.Sort(satellites, CompareSatellites);

        List<DysonSatellite> dynamicList = new List<DysonSatellite>();
        List<DysonSatellite> stationaryList = new List<DysonSatellite>();

        for (int i = 0; i < satellites.Length; i++)
        {
            DysonSatellite satellite = satellites[i];
            if (satellite == null)
            {
                continue;
            }

            if (satellite.IsDynamic)
            {
                dynamicList.Add(satellite);
            }
            else
            {
                stationaryList.Add(satellite);
            }
        }

        List<ActiveDysonBeam> alignments = new List<ActiveDysonBeam>();
        BuildActiveAlignments(dynamicList.ToArray(), stationaryList.ToArray(), toleranceDegrees, alignments);
        return alignments;
    }

    private static void BuildActiveAlignments(DysonSatellite[] dynamicSatellites, DysonSatellite[] stationarySatellites, float toleranceDegrees, List<ActiveDysonBeam> results)
    {
        if (dynamicSatellites == null || stationarySatellites == null)
        {
            return;
        }

        for (int dynamicIndex = 0; dynamicIndex < dynamicSatellites.Length; dynamicIndex++)
        {
            DysonSatellite dynamicSatellite = dynamicSatellites[dynamicIndex];
            if (dynamicSatellite == null)
            {
                continue;
            }

            Vector2 dynamicPosition = dynamicSatellite.transform.position;

            for (int stationaryIndex = 0; stationaryIndex < stationarySatellites.Length; stationaryIndex++)
            {
                DysonSatellite stationarySatellite = stationarySatellites[stationaryIndex];
                if (stationarySatellite == null)
                {
                    continue;
                }

                Vector2 stationaryPosition = stationarySatellite.transform.position;
                if (!AreAligned(dynamicSatellite.sunPosition, dynamicPosition, stationaryPosition, toleranceDegrees))
                {
                    continue;
                }

                results.Add(new ActiveDysonBeam(dynamicSatellite, stationarySatellite, dynamicPosition, stationaryPosition));
                break;
            }
        }
    }

    private static int CompareSatellites(DysonSatellite first, DysonSatellite second)
    {
        if (first == second)
        {
            return 0;
        }

        if (first == null)
        {
            return -1;
        }

        if (second == null)
        {
            return 1;
        }

        int nameCompare = string.CompareOrdinal(first.name, second.name);
        if (nameCompare != 0)
        {
            return nameCompare;
        }

        return first.GetInstanceID().CompareTo(second.GetInstanceID());
    }
}
