using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;

    private void Update()
    {
        // ÿ֡��һ���ٶ������ƶ�
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }
}


