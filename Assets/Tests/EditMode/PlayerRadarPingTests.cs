#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public class PlayerRadarPingTests
{
    [Test]
    public void PulseProgressClampsToDuration()
    {
        Assert.That(PlayerRadarPing.CalculatePulseProgress(2f, 2f, 0.5f), Is.EqualTo(0f).Within(0.001f));
        Assert.That(PlayerRadarPing.CalculatePulseProgress(2.25f, 2f, 0.5f), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(PlayerRadarPing.CalculatePulseProgress(3f, 2f, 0.5f), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void CooldownReadinessUsesLastPingTime()
    {
        Assert.IsFalse(PlayerRadarPing.IsReadyAtTime(5f, 2f, 4f));
        Assert.IsTrue(PlayerRadarPing.IsReadyAtTime(6f, 2f, 4f));
    }

    [Test]
    public void ContactsExpireAfterContactWindow()
    {
        Assert.IsTrue(PlayerRadarPing.IsContactActive(7f, 7f));
        Assert.IsFalse(PlayerRadarPing.IsContactActive(7.01f, 7f));
    }

    [Test]
    public void RadarDefaultsKeepGuidanceAndCooldownForFifteenSeconds()
    {
        PlayerRadarPing radar = new GameObject("Radar").AddComponent<PlayerRadarPing>();

        try
        {
            Assert.That(radar.cooldownSeconds, Is.EqualTo(15f).Within(0.001f));
            Assert.That(radar.contactDuration, Is.EqualTo(15f).Within(0.001f));
            Assert.That(radar.pulseDuration, Is.LessThan(1f));
        }
        finally
        {
            Object.DestroyImmediate(radar.gameObject);
        }
    }

    [Test]
    public void EdgePointerPositionUsesNearestScreenEdge()
    {
        Vector2 right = FoundationHud.EdgePointerPosition(Vector2.right, 200f, 100f, 10f);
        Vector2 up = FoundationHud.EdgePointerPosition(Vector2.up, 200f, 100f, 10f);

        Assert.That(right.x, Is.EqualTo(190f).Within(0.001f));
        Assert.That(right.y, Is.EqualTo(50f).Within(0.001f));
        Assert.That(up.x, Is.EqualTo(100f).Within(0.001f));
        Assert.That(up.y, Is.EqualTo(90f).Within(0.001f));
    }
}
#endif
