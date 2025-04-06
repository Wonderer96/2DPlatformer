using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // �����õ����������
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Camera fullMapCamera;

    // С��ͼ����Ļ�ϵ���ʾ����
    public Rect miniMapRect = new Rect(0, 0, 0.2f, 0.2f);

    // ��ǰģʽö��
    private enum CameraMode
    {
        Main,
        MiniMap,
        FullMap
    }

    private CameraMode currentMode = CameraMode.Main;

    void Start()
    {
        // ��ʼ�������״̬
        SetCameraActiveStates();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // ѭ���л�ģʽ
            currentMode = (CameraMode)(((int)currentMode + 1) % 3);
            SetCameraActiveStates();
        }
    }

    void SetCameraActiveStates()
    {
        // ���ݵ�ǰģʽ���������״̬
        switch (currentMode)
        {
            case CameraMode.Main:
                mainCamera.enabled = true;
                miniMapCamera.enabled = false;
                fullMapCamera.enabled = false;
                break;

            case CameraMode.MiniMap:
                mainCamera.enabled = true;
                miniMapCamera.enabled = true;
                fullMapCamera.enabled = false;
                break;

            case CameraMode.FullMap:
                mainCamera.enabled = false;
                miniMapCamera.enabled = false;
                fullMapCamera.enabled = true;
                break;
        }
    }

    void OnGUI()
    {
        // ֻ����MiniMapģʽ�²Ż���С��ͼ
        if (currentMode == CameraMode.MiniMap && miniMapCamera != null)
        {
            // ����С��ͼ����Ļ�ϵ�ʵ��λ�úʹ�С
            float width = Screen.width * miniMapRect.width;
            float height = Screen.height * miniMapRect.height;
            Rect rect = new Rect(Screen.width * miniMapRect.x,
                               Screen.height * (1 - miniMapRect.y - miniMapRect.height),
                               width, height);

            // ����С��ͼ
            GUI.DrawTexture(rect, miniMapCamera.targetTexture);
        }
    }
}
