using UnityEngine;
using UnityEngine.SceneManagement; // ���볡�����������ռ�

public class SceneTrigger : MonoBehaviour
{
    [Tooltip("��Ҫ���������壨�����ɫ��")]
    public GameObject targetObject; // ָ������������
    [Tooltip("��Ҫ���صĳ�������")]
    public string sceneToLoad; // Ҫ���صĳ�������

    private void OnTriggerEnter(Collider other)
    {
        // �����봥������������Ƿ���ָ������
        if (other.gameObject == targetObject)
        {
            Debug.Log($"�������س���: {sceneToLoad}");

            // ����ָ������
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

