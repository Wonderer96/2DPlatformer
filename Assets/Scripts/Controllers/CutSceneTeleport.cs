using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CutSceneTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform targetPosition;    // 目标传送位置
    public float fadeDuration = 0.5f;    // 淡入淡出持续时间
    public Image blackScreen;           // 全屏黑幕UI

    private bool isTransitioning = false;

    void Start()
    {
        // 初始化黑幕为透明
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 只检测Player层的物体
        if (!isTransitioning && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    IEnumerator TeleportSequence(Transform player)
    {
        isTransitioning = true;

        // 淡入黑幕
        blackScreen.gameObject.SetActive(true);
        yield return StartCoroutine(FadeScreen(0, 1, fadeDuration));

        // 执行传送
        player.position = targetPosition.position;

        // 保持黑屏1秒
        yield return new WaitForSeconds(1f);

        // 淡出黑幕
        yield return StartCoroutine(FadeScreen(1, 0, fadeDuration));
        blackScreen.gameObject.SetActive(false);

        isTransitioning = false;
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        blackScreen.color = new Color(0, 0, 0, endAlpha);
    }
}





