using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 暴露的角色对象，可以在Unity编辑器中关联
    public Transform target;
    public Transform mainCharacterPoint;
    public bool isoverlapped = false;

    // 摄像机与目标的偏移量
    private Vector3 offset;

    void Start()
    {
        target = mainCharacterPoint;
        if (target != null)
        {
            // 计算初始偏移量
            offset = transform.position - target.position;
        }
        else
        {
            Debug.LogError("请在编辑器中设置目标对象（Target）！");
        }
    }

    void LateUpdate()
    {
        if (target != null && isoverlapped == false)
        {
            // 更新摄像机的位置，仅跟随X轴和Y轴
            transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        }
    }

    public void SetTarget(Transform newtarget)
    {
        target = newtarget;
    }
    public void SetMainCharacterAsTarget()
    {
        target = mainCharacterPoint;
    }

}
