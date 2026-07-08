#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public class CameraFollowTests
{
    [Test]
    public void SpeedZoomUsesBaseSizeWhenStationary()
    {
        Assert.That(CameraFollow.CalculateTargetOrthographicSize(0f, 16f, 34f, 90f), Is.EqualTo(16f).Within(0.001f));
    }

    [Test]
    public void SpeedZoomInterpolatesBetweenBaseAndMax()
    {
        float size = CameraFollow.CalculateTargetOrthographicSize(45f, 16f, 34f, 90f);

        Assert.That(size, Is.GreaterThan(16f));
        Assert.That(size, Is.LessThan(34f));
    }

    [Test]
    public void SpeedZoomClampsAtMaxSize()
    {
        Assert.That(CameraFollow.CalculateTargetOrthographicSize(120f, 16f, 34f, 90f), Is.EqualTo(34f).Within(0.001f));
    }

    [Test]
    public void SpeedZoomTreatsNegativeSpeedAsStationary()
    {
        Assert.That(CameraFollow.CalculateTargetOrthographicSize(-10f, 16f, 34f, 90f), Is.EqualTo(16f).Within(0.001f));
    }
}
#endif
