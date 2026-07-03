#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public class PlanetGravityWellTests
{
    [Test]
    public void GravityIsZeroOutsideInfluenceRadius()
    {
        float acceleration = PlanetGravityWell.CalculateAcceleration(50f, 10f, 40f, 1.35f);

        Assert.That(acceleration, Is.EqualTo(0f));
    }

    [Test]
    public void GravityFallsOffTowardInfluenceEdge()
    {
        float nearSurface = PlanetGravityWell.CalculateAcceleration(12f, 10f, 40f, 1.35f);
        float nearEdge = PlanetGravityWell.CalculateAcceleration(35f, 10f, 40f, 1.35f);

        Assert.That(nearSurface, Is.GreaterThan(nearEdge));
        Assert.That(nearSurface, Is.LessThanOrEqualTo(1.35f));
        Assert.That(nearEdge, Is.GreaterThan(0f));
    }
}
#endif
