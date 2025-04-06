using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // 可配置的三个摄像机
    public Camera mainCamera;
    public Camera miniMapCamera;
    public Camera fullMapCamera;

    // 小地图在屏幕上的显示区域
    public Rect miniMapRect = new Rect(0, 0, 0.2f, 0.2f);

    // 当前模式枚举
    private enum CameraMode
    {
        Main,
        MiniMap,
        FullMap
    }

    private CameraMode currentMode = CameraMode.Main;

    void Start()
    {
        // 初始化摄像机状态
        SetCameraActiveStates();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // 循环切换模式
            currentMode = (CameraMode)(((int)currentMode + 1) % 3);
            SetCameraActiveStates();
        }
    }

    void SetCameraActiveStates()
    {
        // 根据当前模式设置摄像机状态
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
        // 只有在MiniMap模式下才绘制小地图
        if (currentMode == CameraMode.MiniMap && miniMapCamera != null)
        {
            // 计算小地图在屏幕上的实际位置和大小
            float width = Screen.width * miniMapRect.width;
            float height = Screen.height * miniMapRect.height;
            Rect rect = new Rect(Screen.width * miniMapRect.x,
                               Screen.height * (1 - miniMapRect.y - miniMapRect.height),
                               width, height);

            // 绘制小地图
            GUI.DrawTexture(rect, miniMapCamera.targetTexture);
        }
    }
}
