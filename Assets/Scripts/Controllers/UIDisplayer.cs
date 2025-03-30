using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    public Image[] heartImages; // ���ĵ�Image����
    public int HP; // ����ֵ����
    public MainCharacterController mainCharacterController;

    void Start()
    {
        UpdateHealthUI(); // ��ʼ��UI��ʾ
    }
    private void Update()
    {
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        HP = mainCharacterController.hP;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < HP)
            {
                heartImages[i].enabled = true; // ��ʾ����
            }
            else
            {
                heartImages[i].enabled = false; // ���ذ���
            }
        }
    }

    public void SetHP(int newHP)
    {
        HP = newHP; // ��������ֵ
        UpdateHealthUI(); // ͬ������UI
    }
}

