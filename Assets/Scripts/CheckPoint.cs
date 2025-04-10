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
        // ��� redFlag �� blueFlag �Ƿ�Ϊ��
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
        // ֻ�б�ǩΪ "Player" �Ķ�����ܴ���
        if (collision.CompareTag("Player"))
        {
            gameManager.currentRespawnPoint = this.gameObject;

            // ��� blueFlag �� redFlag �Ƿ�Ϊ��
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


