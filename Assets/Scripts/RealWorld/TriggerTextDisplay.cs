using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TriggerTextDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject imageObject;
    public TextMeshPro messageText;

    [Header("Text Settings")]
    [TextArea] public string fullMessage;
    public float typeSpeed = 0.05f;
    public float messageDuration = 3f;
    public float fadeDuration = 0.5f; // 淡入淡出时间

    private bool playerInside = false;
    private bool messageShown = false;
    private CanvasGroup textCanvasGroup;

    private void Start()
    {
        imageObject.SetActive(false);
        messageText.text = "";

        // 尝试获取 CanvasGroup，如果没有就自动添加
        textCanvasGroup = messageText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = messageText.gameObject.AddComponent<CanvasGroup>();
        }
        textCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (playerInside && !messageShown && Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(DisplayMessage());
        }
    }

    private IEnumerator DisplayMessage()
    {
        messageShown = true;
        imageObject.SetActive(false);
        messageText.text = "";
        textCanvasGroup.alpha = 0f;

        // 淡入文字
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

        for (int i = 0; i < fullMessage.Length; i++)
        {
            messageText.text += fullMessage[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(messageDuration);

        // 淡出文字
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        messageText.text = "";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }
        group.alpha = endAlpha;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            imageObject.SetActive(true);
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = false;
            imageObject.SetActive(false);
            messageText.text = "";
            messageShown = false;
            textCanvasGroup.alpha = 0f;
        }
    }
}


