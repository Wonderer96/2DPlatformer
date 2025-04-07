using UnityEngine;
using System.Linq;

public class SwitchPlatfrom : MonoBehaviour
{
    [Header("Settings")]
    public BaseTrigger[] pressurePlates; // 关联的触发器
    public GameObject onObject; // 当激活时显示的对象
    public GameObject offObject; // 当未激活时显示的对象

    private bool isActivated = false;

    void Update()
    {
        CheckPressurePlates();
        UpdateObjects();
    }

    private void CheckPressurePlates()
    {
        // 检查所有的触发器是否都被激活
        isActivated = pressurePlates.All(plate => plate != null && plate.IsActivated);
    }

    private void UpdateObjects()
    {
        if (onObject != null && offObject != null)
        {
            // 根据激活状态更新对象的可见性
            onObject.SetActive(isActivated);
            offObject.SetActive(!isActivated);
        }
    }
}

