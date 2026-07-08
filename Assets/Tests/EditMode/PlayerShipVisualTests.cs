#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PlayerShipVisualTests
{
    [Test]
    public void SelectSpriteUsesIdleSpriteWhenNotThrusting()
    {
        Sprite idle = CreateSprite();
        Sprite thrust = CreateSprite();

        try
        {
            Assert.That(PlayerShipVisual.SelectSprite(false, idle, thrust, null), Is.SameAs(idle));
        }
        finally
        {
            Object.DestroyImmediate(idle.texture);
            Object.DestroyImmediate(thrust.texture);
            Object.DestroyImmediate(idle);
            Object.DestroyImmediate(thrust);
        }
    }

    [Test]
    public void SelectSpriteUsesThrustSpriteWhenThrusting()
    {
        Sprite idle = CreateSprite();
        Sprite thrust = CreateSprite();

        try
        {
            Assert.That(PlayerShipVisual.SelectSprite(true, idle, thrust, null), Is.SameAs(thrust));
        }
        finally
        {
            Object.DestroyImmediate(idle.texture);
            Object.DestroyImmediate(thrust.texture);
            Object.DestroyImmediate(idle);
            Object.DestroyImmediate(thrust);
        }
    }

    [Test]
    public void SelectSpriteFallsBackToThrustSpriteWhenIdleSpriteIsMissing()
    {
        Sprite thrust = CreateSprite();

        try
        {
            Assert.That(PlayerShipVisual.SelectSprite(false, null, thrust, null), Is.SameAs(thrust));
        }
        finally
        {
            Object.DestroyImmediate(thrust.texture);
            Object.DestroyImmediate(thrust);
        }
    }

    private static Sprite CreateSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
    }
}
#endif
