using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ProceduralSpaceBackground : MonoBehaviour
{
    [Header("Generation")]
    public int seed = 1107;
    public int textureSize = 1024;
    public int nebulaTextureSize = 512;
    public float starDensity = 0.0028f;

    [Header("Motion")]
    public float nebulaParallax = 0.03f;
    public float farStarParallax = 0.08f;
    public float nearStarParallax = 0.16f;
    public float speedStretch = 0.06f;
    public float textureScrollWorldScale = 90f;
    public float layerMotionMultiplier = 0.08f;

    private Camera attachedCamera;
    private BackgroundLayer nebulaLayer;
    private BackgroundLayer farStarLayer;
    private BackgroundLayer nearStarLayer;
    private Vector3 previousCameraPosition;
    private Vector2 travelVelocity;

    private struct BackgroundLayer
    {
        public Transform transform;
        public MeshRenderer renderer;
        public Material material;
        public float parallax;
        public float localDepth;
        public Vector2 offset;
    }

    private void Awake()
    {
        attachedCamera = GetComponent<Camera>();
        attachedCamera.backgroundColor = new Color(0.005f, 0.007f, 0.018f);
        previousCameraPosition = transform.position;

        BuildLayers();
        ResizeLayersToCamera();
    }

    private void LateUpdate()
    {
        ResizeLayersToCamera();
        UpdateTravelVelocity();
        ScrollLayer(ref nebulaLayer);
        ScrollLayer(ref farStarLayer);
        ScrollLayer(ref nearStarLayer);
    }

    private void OnDestroy()
    {
        DestroyLayerResources(nebulaLayer);
        DestroyLayerResources(farStarLayer);
        DestroyLayerResources(nearStarLayer);
    }

    private void BuildLayers()
    {
        Texture2D nebulaTexture = GenerateNebulaTexture(Mathf.Max(64, nebulaTextureSize), seed);
        Texture2D farStarTexture = GenerateStarTexture(Mathf.Max(128, textureSize), seed + 17, starDensity, 0.25f, 0.78f);
        Texture2D nearStarTexture = GenerateStarTexture(Mathf.Max(128, textureSize), seed + 73, starDensity * 0.42f, 0.35f, 1f);

        nebulaLayer = CreateLayer("Procedural Nebula", nebulaTexture, nebulaParallax, 18f, new Color(1f, 1f, 1f, 0.92f), FilterMode.Bilinear);
        farStarLayer = CreateLayer("Procedural Far Stars", farStarTexture, farStarParallax, 17f, Color.white, FilterMode.Point);
        nearStarLayer = CreateLayer("Procedural Near Stars", nearStarTexture, nearStarParallax, 16f, Color.white, FilterMode.Point);
    }

    private BackgroundLayer CreateLayer(string layerName, Texture2D texture, float parallax, float localDepth, Color tint, FilterMode filterMode)
    {
        GameObject layerObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        layerObject.name = layerName;
        layerObject.transform.SetParent(transform, false);
        layerObject.transform.localPosition = new Vector3(0f, 0f, localDepth);
        layerObject.transform.localRotation = Quaternion.identity;

        Collider collider = layerObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Material material = new Material(FindBackgroundShader());
        material.name = layerName + " Material";
        ApplyMaterialTexture(material, texture);
        ApplyMaterialColor(material, tint);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = filterMode;

        MeshRenderer renderer = layerObject.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return new BackgroundLayer
        {
            transform = layerObject.transform,
            renderer = renderer,
            material = material,
            parallax = parallax,
            localDepth = localDepth,
            offset = Vector2.zero
        };
    }

    private void ResizeLayersToCamera()
    {
        if (attachedCamera == null || !attachedCamera.orthographic)
        {
            return;
        }

        float height = attachedCamera.orthographicSize * 2.4f;
        float width = height * attachedCamera.aspect;
        Vector3 scale = new Vector3(width, height, 1f);

        ApplyLayerScale(nebulaLayer, scale);
        ApplyLayerScale(farStarLayer, scale);
        ApplyLayerScale(nearStarLayer, scale);
    }

    private static void ApplyLayerScale(BackgroundLayer layer, Vector3 scale)
    {
        if (layer.transform != null)
        {
            layer.transform.localScale = scale;
        }
    }

    private void UpdateTravelVelocity()
    {
        Vector3 cameraPosition = transform.position;
        Vector3 delta = cameraPosition - previousCameraPosition;
        previousCameraPosition = cameraPosition;

        if (Time.deltaTime <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 currentVelocity = new Vector2(delta.x, delta.y) / Time.deltaTime;
        travelVelocity = Vector2.Lerp(travelVelocity, currentVelocity, 1f - Mathf.Exp(-8f * Time.deltaTime));
    }

    private void ScrollLayer(ref BackgroundLayer layer)
    {
        if (layer.material == null)
        {
            return;
        }

        Vector2 cameraPosition = transform.position;
        Vector2 velocityBias = travelVelocity * speedStretch * layer.parallax;
        layer.offset = ((cameraPosition * layer.parallax) + velocityBias) / Mathf.Max(1f, textureScrollWorldScale);
        ApplyMaterialOffset(layer.material, layer.offset);

        Vector3 localPosition = layer.transform.localPosition;
        Vector2 localDrift = cameraPosition * layer.parallax * layerMotionMultiplier;
        layer.transform.localPosition = new Vector3(
            localDrift.x,
            localDrift.y,
            localPosition.z
        );
    }

    public static Texture2D GenerateNebulaTexture(int size, int seed)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Procedural Nebula Texture";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        float seedX = (seed % 997) * 0.137f;
        float seedY = (seed % 577) * 0.211f;
        Color deepSpace = new Color(0.006f, 0.008f, 0.026f, 1f);
        Color blueViolet = new Color(0.06f, 0.11f, 0.32f, 1f);
        Color purple = new Color(0.17f, 0.06f, 0.28f, 1f);
        Color coldBlue = new Color(0.02f, 0.2f, 0.38f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;
                float v = (float)y / size;
                float broad = FractalPerlin(u * 2.1f + seedX, v * 2.1f + seedY, 4, 0.5f);
                float detail = FractalPerlin(u * 7.5f + seedY, v * 7.5f + seedX, 3, 0.58f);
                float veil = Mathf.SmoothStep(0.28f, 0.92f, broad);
                float wisps = Mathf.SmoothStep(0.42f, 0.86f, detail) * 0.55f;

                Color color = Color.Lerp(deepSpace, blueViolet, veil * 0.62f);
                color = Color.Lerp(color, purple, Mathf.Clamp01((detail - 0.48f) * 1.9f));
                color = Color.Lerp(color, coldBlue, wisps * Mathf.Clamp01(1f - veil * 0.35f));
                color *= 0.55f + (veil * 0.4f) + (wisps * 0.2f);
                color.a = 1f;

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        return texture;
    }

    public static Texture2D GenerateStarTexture(int size, int seed, float density, float minBrightness, float maxBrightness)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Procedural Star Texture";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clear;
        }

        System.Random random = new System.Random(seed);
        int candidateCount = Mathf.RoundToInt(size * size * density);
        float noiseOffsetX = (seed % 4093) * 0.017f;
        float noiseOffsetY = (seed % 3541) * 0.019f;

        for (int i = 0; i < candidateCount; i++)
        {
            int x = random.Next(0, size);
            int y = random.Next(0, size);
            float noise = Mathf.PerlinNoise((x * 0.031f) + noiseOffsetX, (y * 0.031f) + noiseOffsetY);

            if (noise < 0.38f)
            {
                continue;
            }

            float randomBrightness = minBrightness + ((float)random.NextDouble() * (maxBrightness - minBrightness));
            float brightness = Mathf.Clamp01(randomBrightness * Mathf.Lerp(0.68f, 1.18f, noise));
            int radius = random.NextDouble() > 0.9 ? 1 : 0;

            PlotStar(pixels, size, x, y, radius, brightness);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    private static void PlotStar(Color[] pixels, int size, int centerX, int centerY, int radius, float brightness)
    {
        Color core = new Color(brightness, brightness, Mathf.Min(1f, brightness * 1.08f), 1f);
        SetWrappedPixel(pixels, size, centerX, centerY, core);

        if (radius <= 0)
        {
            return;
        }

        Color edge = new Color(brightness * 0.34f, brightness * 0.34f, brightness * 0.42f, 0.5f);
        SetWrappedPixel(pixels, size, centerX + 1, centerY, edge);
        SetWrappedPixel(pixels, size, centerX - 1, centerY, edge);
        SetWrappedPixel(pixels, size, centerX, centerY + 1, edge);
        SetWrappedPixel(pixels, size, centerX, centerY - 1, edge);
    }

    private static void SetWrappedPixel(Color[] pixels, int size, int x, int y, Color color)
    {
        int wrappedX = (x + size) % size;
        int wrappedY = (y + size) % size;
        pixels[(wrappedY * size) + wrappedX] = color;
    }

    private static float FractalPerlin(float x, float y, int octaves, float persistence)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float value = 0f;
        float totalAmplitude = 0f;

        for (int i = 0; i < octaves; i++)
        {
            value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            totalAmplitude += amplitude;
            amplitude *= persistence;
            frequency *= 2f;
        }

        return value / totalAmplitude;
    }

    private static Shader FindBackgroundShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Unlit/Texture");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            return shader;
        }

        return Shader.Find("Standard");
    }

    private static void ApplyMaterialTexture(Material material, Texture2D texture)
    {
        material.mainTexture = texture;

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }
    }

    private static void ApplyMaterialColor(Material material, Color tint)
    {
        material.color = tint;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", tint);
        }
    }

    private static void ApplyMaterialOffset(Material material, Vector2 offset)
    {
        material.mainTextureOffset = offset;

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTextureOffset("_BaseMap", offset);
        }
    }

    private static void DestroyLayerResources(BackgroundLayer layer)
    {
        if (Application.isPlaying)
        {
            Texture texture = null;
            if (layer.material != null)
            {
                texture = layer.material.mainTexture;
            }
            else if (layer.renderer != null && layer.renderer.sharedMaterial != null)
            {
                texture = layer.renderer.sharedMaterial.mainTexture;
            }

            if (layer.material != null)
            {
                Destroy(layer.material);
            }

            if (texture != null)
            {
                Destroy(texture);
            }
        }
    }
}
