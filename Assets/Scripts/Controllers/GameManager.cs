using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public GameObject mainCamera;
    public MainCharacterController mainCharacter;
    public GameObject currentRespawnPoint;

    void Start()
    {

    }
    void Update()
    {
        if (mainCharacter.hP <= 0)
        {
            ReSpawn();
        }
    }

    void LateUpdate()
    {

    }
    public void ReSpawn()
    {
        // ʹ�� currentRespawnPoint �� transform.position ����ȡλ��
        mainCharacter.gameObject.transform.position = currentRespawnPoint.transform.position;
        mainCharacter.hP = 3;
    }

}
