using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理命名空间

public class SceneTrigger : MonoBehaviour
{
    [Tooltip("需要触发的物体（例如角色）")]
    public GameObject targetObject; // 指定触发的物体
    [Tooltip("需要加载的场景名称")]
    public string sceneToLoad; // 要加载的场景名称

    private void OnTriggerEnter(Collider other)
    {
        // 检测进入触发区域的物体是否是指定物体
        if (other.gameObject == targetObject)
        {
            Debug.Log($"触发加载场景: {sceneToLoad}");

            // 加载指定场景
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}

