using System.Collections.Generic;
using UnityEngine;

public class AsteroidCarverHull : MonoBehaviour
{
    public float cutRadius = 1.25f;
    public float cutInterval = 0.08f;

    private readonly Dictionary<CircularDestructibleAsteroid, float> nextCutTimes = new Dictionary<CircularDestructibleAsteroid, float>();

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryCut(collision.collider, Time.time);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCut(other, Time.time);
    }

    public bool TryCut(Collider2D other, float now)
    {
        if (other == null || !IsUnlocked())
        {
            return false;
        }

        CircularDestructibleAsteroid asteroid = other.GetComponentInParent<CircularDestructibleAsteroid>();
        if (asteroid == null)
        {
            return false;
        }

        if (nextCutTimes.TryGetValue(asteroid, out float nextTime) && now < nextTime)
        {
            return false;
        }

        Vector2 direction = GetComponent<Rigidbody2D>() == null
            ? (Vector2)transform.up
            : GetComponent<Rigidbody2D>().linearVelocity;
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = transform.up;
        }

        bool cut = asteroid.ApplyCircularCut(transform.position, cutRadius, direction);
        if (cut)
        {
            nextCutTimes[asteroid] = now + Mathf.Max(0f, cutInterval);
        }

        return cut;
    }

    private bool IsUnlocked()
    {
        PlayerUpgradeState state = GetComponent<PlayerUpgradeState>();
        if (state == null)
        {
            state = PlayerUpgradeState.Current;
        }

        return state != null && state.IsUnlocked(UpgradeId.AsteroidCarverHull);
    }
}
