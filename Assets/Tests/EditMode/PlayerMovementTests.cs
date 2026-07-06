#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PlayerMovementTests
{
    [Test]
    public void SimpleControlsStopWhenThrustReleased()
    {
        Vector2 velocity = PlayerMovement.CalculateSimpleVelocity(Vector2.right * 10f, Vector2.up, 0f, 100f, 100f, 1f);

        Assert.That(velocity, Is.EqualTo(Vector2.zero));
    }

    [Test]
    public void SimpleControlsRedirectCurrentSpeedAndAccelerateAlongFacingDirection()
    {
        Vector2 velocity = PlayerMovement.CalculateSimpleVelocity(Vector2.right * 10f, Vector2.up, 1f, 8f, 100f, 1f);

        Assert.That(velocity.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(velocity.y, Is.EqualTo(18f).Within(0.001f));
    }

    [Test]
    public void SimpleControlsDoNotCapVelocity()
    {
        Vector2 velocity = PlayerMovement.CalculateSimpleVelocity(Vector2.up * 100f, Vector2.up, 1f, 8f, 100f, 1f);

        Assert.That(velocity.x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(velocity.y, Is.EqualTo(108f).Within(0.001f));
    }
}
#endif
