using UnityEngine;
using System.Linq;

public class SwitchPlatfrom : MonoBehaviour
{
    [Header("Settings")]
    public BaseTrigger[] pressurePlates; // �����Ĵ�����
    public GameObject onObject; // ������ʱ��ʾ�Ķ���
    public GameObject offObject; // ��δ����ʱ��ʾ�Ķ���

    private bool isActivated = false;

    void Update()
    {
        CheckPressurePlates();
        UpdateObjects();
    }

    private void CheckPressurePlates()
    {
        // ������еĴ������Ƿ񶼱�����
        isActivated = pressurePlates.All(plate => plate != null && plate.IsActivated);
    }

    private void UpdateObjects()
    {
        if (onObject != null && offObject != null)
        {
            // ���ݼ���״̬���¶���Ŀɼ���
            onObject.SetActive(isActivated);
            offObject.SetActive(!isActivated);
        }
    }
}

