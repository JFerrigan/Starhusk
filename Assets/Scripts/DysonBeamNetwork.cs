using UnityEngine;

public class DysonBeamNetwork : MonoBehaviour
{
    public float alignmentToleranceDegrees = 35f;
    public float beamWidth = 0.18f;
    public float beamUpdateInterval = 0.05f;
    private float nextBeamUpdateTime;
    
    public Color beamColor = new Color(1f, 0.92f, 0.35f, 0.82f);

    private DysonSatellite[] dynamicSatellites;
    private DysonSatellite[] stationarySatellites;
    private LineRenderer[] beams;

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

        int beamIndex = 0;

for (int dynamicIndex = 0; dynamicIndex < dynamicSatellites.Length && beamIndex < beams.Length; dynamicIndex++)
{
    DysonSatellite dynamicSatellite = dynamicSatellites[dynamicIndex];
    if (dynamicSatellite == null)
    {
        continue;
    }

    bool foundBeam = false;
    Vector2 dynamicPosition = dynamicSatellite.transform.position;

    for (int stationaryIndex = 0; stationaryIndex < stationarySatellites.Length; stationaryIndex++)
    {
        DysonSatellite stationarySatellite = stationarySatellites[stationaryIndex];
        if (stationarySatellite == null)
        {
            continue;
        }

        Vector2 stationaryPosition = stationarySatellite.transform.position;

        bool aligned = AreAligned(
            dynamicSatellite.sunPosition,
            dynamicPosition,
            stationaryPosition,
            alignmentToleranceDegrees
        );

        if (aligned)
        {
            SetBeam(beams[beamIndex], dynamicPosition, stationaryPosition, true);
            foundBeam = true;
            break;
        }
    }

    if (!foundBeam)
    {
        beams[beamIndex].enabled = false;
    }

    beamIndex++;
}

for (int i = beamIndex; i < beams.Length; i++)
{
    beams[i].enabled = false;
}

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
}
