using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
    // 指定的对象
    public GameObject targetObject;

    // 指定的摄像机
    public Camera targetCamera;

    // 目标大小
    public float targetSize = 5f;
    public float originalSize ;

    // 调整速度
    public float adjustSpeed = 2f;

    // 内部标志，表示对象是否已进入触发器
    private bool isTargetInside = false;

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入触发器的是否是指定对象
        if (other.gameObject == targetObject)
        {
            originalSize = targetCamera.orthographicSize;
            isTargetInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 检查离开触发器的是否是指定对象
        if (other.gameObject == targetObject)
        {
            isTargetInside = false;
        }
    }

    private void Update()
    {
        // 如果目标对象在触发器内，逐渐调整摄像机的Size
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
