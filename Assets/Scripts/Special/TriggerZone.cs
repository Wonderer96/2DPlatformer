using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    // 声明一个ZoneManager类型的public变量
    public ZoneManager zoneManager;

    // 当带有player tag的object进入这个trigger时
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 调用zoneManager的DestroyAllObjects方法
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

