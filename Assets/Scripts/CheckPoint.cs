using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject redFlag;
    public GameObject blueFlag;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // 检查 redFlag 和 blueFlag 是否为空
        if (gameManager.currentRespawnPoint != gameObject && redFlag != null && redFlag.activeSelf)
        {
            if (blueFlag != null)
            {
                blueFlag.SetActive(true);
            }
            if (redFlag != null)
            {
                redFlag.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有标签为 "Player" 的对象才能触发
        if (collision.CompareTag("Player"))
        {
            gameManager.currentRespawnPoint = this.gameObject;

            // 检查 blueFlag 和 redFlag 是否为空
            if (blueFlag != null)
            {
                blueFlag.SetActive(false);
            }
            if (redFlag != null)
            {
                redFlag.SetActive(true);
            }
        }
    }
}


