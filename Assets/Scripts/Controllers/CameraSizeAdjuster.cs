using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
    // ָ���Ķ���
    public GameObject targetObject;

    // ָ���������
    public Camera targetCamera;

    // Ŀ���С
    public float targetSize = 5f;
    public float originalSize ;

    // �����ٶ�
    public float adjustSpeed = 2f;

    // �ڲ���־����ʾ�����Ƿ��ѽ��봥����
    private bool isTargetInside = false;

    private void OnTriggerEnter(Collider other)
    {
        // �����봥�������Ƿ���ָ������
        if (other.gameObject == targetObject)
        {
            originalSize = targetCamera.orthographicSize;
            isTargetInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ����뿪���������Ƿ���ָ������
        if (other.gameObject == targetObject)
        {
            isTargetInside = false;
        }
    }

    private void Update()
    {
        // ���Ŀ������ڴ������ڣ��𽥵����������Size
        if (isTargetInside && targetCamera != null)
        {
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, targetSize, Time.deltaTime * adjustSpeed);
        }
        else
        {
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, originalSize, Time.deltaTime * adjustSpeed);
        }
    }
}
