using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    public Image[] heartImages; // 爱心的Image数组
    public int HP; // 生命值变量
    public MainCharacterController mainCharacterController;

    void Start()
    {
        UpdateHealthUI(); // 初始化UI显示
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
                heartImages[i].enabled = true; // 显示爱心
            }
            else
            {
                heartImages[i].enabled = false; // 隐藏爱心
            }
        }
    }

    public void SetHP(int newHP)
    {
        HP = newHP; // 更新生命值
        UpdateHealthUI(); // 同步更新UI
    }
}

