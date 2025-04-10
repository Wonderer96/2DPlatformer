using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    // ����һ��ZoneManager���͵�public����
    public ZoneManager zoneManager;

    // ������player tag��object�������triggerʱ
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ����zoneManager��DestroyAllObjects����
            if (zoneManager != null)
            {
                zoneManager.DestroyAllObjects();
            }
            else
            {
                Debug.LogWarning("ZoneManager is not assigned in the editor.");
            }
        }
    }
}

