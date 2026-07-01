using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScanner : MonoBehaviour
{
    public float scanRadius = 12f;
    public float cooldownSeconds = 2.5f;
    public Key scanKey = Key.F;

    private float nextScanTime;

    public bool IsReady => Time.time >= nextScanTime;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[scanKey].wasPressedThisFrame)
        {
            Scan();
        }
    }

    public int Scan()
    {
        if (!IsReady)
        {
            return 0;
        }

        nextScanTime = Time.time + cooldownSeconds;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);
        int revealedCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            DiscoveryState discovery = hits[i].GetComponentInParent<DiscoveryState>();

            if (discovery != null && !discovery.discovered)
            {
                discovery.Reveal();
                revealedCount++;
            }
        }

        Debug.Log("Scanner pulse revealed " + revealedCount + " objects");
        return revealedCount;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.8f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
