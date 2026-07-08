using System;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    public ShipFaction faction = ShipFaction.Neutral;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public bool destroyOnDeath = true;

    public event Action<ShipHealth> Died;

    public bool IsAlive => currentHealth > 0f;

    private void Awake()
    {
        if (currentHealth <= 0f)
        {
            currentHealth = maxHealth;
        }
    }

    public bool ApplyDamage(float amount, ShipFaction sourceFaction)
    {
        if (!IsAlive || amount <= 0f || (sourceFaction != ShipFaction.Neutral && sourceFaction == faction))
        {
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (currentHealth > 0f)
        {
            return true;
        }

        Died?.Invoke(this);
        if (destroyOnDeath)
        {
            DestroyShip();
        }

        return true;
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
    }

    private void DestroyShip()
    {
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
}
