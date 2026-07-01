using NUnit.Framework;
using UnityEngine;

public class ProceduralSpaceBackgroundTests
{
    [Test]
    public void SameSeedProducesSameStarTexture()
    {
        Texture2D first = ProceduralSpaceBackground.GenerateStarTexture(64, 21, 0.01f, 0.2f, 1f);
        Texture2D second = ProceduralSpaceBackground.GenerateStarTexture(64, 21, 0.01f, 0.2f, 1f);

        try
        {
            Color[] firstPixels = first.GetPixels();
            Color[] secondPixels = second.GetPixels();

            Assert.AreEqual(firstPixels.Length, secondPixels.Length);

            for (int i = 0; i < firstPixels.Length; i++)
            {
                Assert.That(firstPixels[i].r, Is.EqualTo(secondPixels[i].r).Within(0.001f));
                Assert.That(firstPixels[i].a, Is.EqualTo(secondPixels[i].a).Within(0.001f));
            }
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }
    }

    [Test]
    public void StarTextureKeepsStarsSparseAndTiny()
    {
        Texture2D texture = ProceduralSpaceBackground.GenerateStarTexture(128, 42, 0.006f, 0.2f, 1f);

        try
        {
            int litPixels = 0;
            foreach (Color pixel in texture.GetPixels())
            {
                if (pixel.a > 0.01f)
                {
                    litPixels++;
                }
            }

            Assert.That(litPixels, Is.GreaterThan(0));
            Assert.That(litPixels, Is.LessThan(texture.width * texture.height * 0.02f));
        }
        finally
        {
            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void NebulaTextureHasVisibleColorVariation()
    {
        Texture2D texture = ProceduralSpaceBackground.GenerateNebulaTexture(64, 1107);

        try
        {
            Color[] pixels = texture.GetPixels();
            float minBlue = float.MaxValue;
            float maxBlue = float.MinValue;

            foreach (Color pixel in pixels)
            {
                minBlue = Mathf.Min(minBlue, pixel.b);
                maxBlue = Mathf.Max(maxBlue, pixel.b);
            }

            Assert.That(maxBlue - minBlue, Is.GreaterThan(0.03f));
        }
        finally
        {
            Object.DestroyImmediate(texture);
        }
    }
}
