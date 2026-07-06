#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PixelUiSpritesTests
{
    [Test]
    public void GeneratedUiTexturesUsePointFiltering()
    {
        foreach (PixelUiFrame frame in System.Enum.GetValues(typeof(PixelUiFrame)))
        {
            Texture2D texture = PixelUiSprites.TextureFor(frame);

            Assert.IsNotNull(texture, frame.ToString());
            Assert.That(texture.format, Is.EqualTo(TextureFormat.RGBA32), frame.ToString());
            Assert.That(texture.filterMode, Is.EqualTo(FilterMode.Point), frame.ToString());
            Assert.That(texture.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), frame.ToString());
        }
    }

    [Test]
    public void ResourceIconsExistForEveryResourceType()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            Texture2D icon = ResourceGui.IconFor(type);

            Assert.IsNotNull(icon, type.ToString());
            Assert.That(icon.width, Is.EqualTo(16), type.ToString());
            Assert.That(icon.height, Is.EqualTo(16), type.ToString());
            Assert.That(icon.format, Is.EqualTo(TextureFormat.RGBA32), type.ToString());
            Assert.That(icon.filterMode, Is.EqualTo(FilterMode.Point), type.ToString());
            Assert.That(icon.wrapMode, Is.EqualTo(TextureWrapMode.Clamp), type.ToString());
        }
    }

    [Test]
    public void GridLayoutComputesSaneColumnsAndHeight()
    {
        Assert.That(PixelUiSprites.GridColumnCount(120f, 148f, 12f), Is.EqualTo(1));
        Assert.That(PixelUiSprites.GridColumnCount(620f, 148f, 12f), Is.GreaterThanOrEqualTo(3));

        float narrowHeight = PixelUiSprites.GridContentHeight(5, 120f, 148f, 172f, 12f);
        float wideHeight = PixelUiSprites.GridContentHeight(5, 620f, 148f, 172f, 12f);

        Assert.That(narrowHeight, Is.GreaterThan(wideHeight));
        Assert.That(wideHeight, Is.GreaterThanOrEqualTo(172f));
    }

    [Test]
    public void AutomatonPresentationHelpersReturnSprites()
    {
        foreach (AutomatonBuildOption option in AutomatonPlacementController.AllBuildOptions)
        {
            Assert.IsNotNull(AutomatonPlacementController.SpriteFor(option), option.ToString());
            Assert.That(AutomatonPlacementController.ColorFor(option).a, Is.GreaterThan(0f), option.ToString());
        }
    }
}
#endif
