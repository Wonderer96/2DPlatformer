using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject mainCamera;
    public MainCharacterController mainCharacter;
    public GameObject currentRespawnPoint;
    public ZoneManager currentZone;
    public TextMeshProUGUI coinNumUI;
    public GameObject clone;
    public GameObject cloneSpawnPoint;

    public int coin = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {

    }
    void Update()
    {
        if (mainCharacter.hP <= 0)
        {
            ReSpawn();
        }
        coinNumUI.text = coin.ToString();
    }

    void LateUpdate()
    {

    }
    public void ReSpawn()
    {
        // ʹ�� currentRespawnPoint �� transform.position ����ȡλ��
        Rigidbody2D rb = mainCharacter.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f; // ��ѡ�����������ת�Ļ�Ҳ���
        }
        mainCharacter.gameObject.transform.position = currentRespawnPoint.transform.position + new Vector3(0, 1, 0);
        mainCharacter.hP = mainCharacter.maxHP;
        currentZone.StopAllSpawning();
        currentZone.DestroyAllObjects();
        currentZone.StartSpawning();
        Instantiate(clone, cloneSpawnPoint.transform.position, Quaternion.identity);
    }

}
