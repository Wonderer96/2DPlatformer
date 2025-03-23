using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // ��¶�Ľ�ɫ���󣬿�����Unity�༭���й���
    public Transform target;
    public Transform mainCharacterPoint;
    public bool isoverlapped = false;

    // �������Ŀ���ƫ����
    private Vector3 offset;

    void Start()
    {
        target = mainCharacterPoint;
        if (target != null)
        {
            // �����ʼƫ����
            offset = transform.position - target.position;
        }
        else
        {
            Debug.LogError("���ڱ༭��������Ŀ�����Target����");
        }
    }

    void LateUpdate()
    {
        if (target != null && isoverlapped == false)
        {
            // �����������λ�ã�������X���Y��
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
