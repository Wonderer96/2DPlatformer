using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CutSceneTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform targetPosition;    // Ŀ�괫��λ��
    public float fadeDuration = 0.5f;    // ���뵭������ʱ��
    public Image blackScreen;           // ȫ����ĻUI

    private bool isTransitioning = false;

    void Start()
    {
        // ��ʼ����ĻΪ͸��
        if (blackScreen != null)
        {
            blackScreen.color = new Color(0, 0, 0, 0);
            blackScreen.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ֻ���Player�������
        if (!isTransitioning && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    IEnumerator TeleportSequence(Transform player)
    {
        isTransitioning = true;

        // �����Ļ
        blackScreen.gameObject.SetActive(true);
        yield return StartCoroutine(FadeScreen(0, 1, fadeDuration));

        // ִ�д���
        player.position = targetPosition.position;

        // ���ֺ���1��
        yield return new WaitForSeconds(1f);

        // ������Ļ
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





