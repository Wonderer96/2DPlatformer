using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TriggerTextDisplay : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSegment
    {
        [TextArea] public string text;
        public GameObject targetPosition; // 每段话显示的位置
        public float duration = 3f; // 每段话显示时间
    }

    [Header("UI Elements")]
    public GameObject imageObject;
    public TextMeshPro messageText;

    [Header("Dialogue Settings")]
    public List<DialogueSegment> dialogues = new List<DialogueSegment>();
    public float typeSpeed = 0.05f;
    public float fadeDuration = 0.5f;

    private bool playerInside = false;
    private bool dialogueStarted = false;
    private CanvasGroup textCanvasGroup;

    private void Start()
    {
        imageObject.SetActive(false);
        messageText.text = "";

        textCanvasGroup = messageText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = messageText.gameObject.AddComponent<CanvasGroup>();
        }
        textCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (playerInside && !dialogueStarted && Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(PlayDialogues());
        }
    }

    private IEnumerator PlayDialogues()
    {
        dialogueStarted = true;
        imageObject.SetActive(false);

        foreach (DialogueSegment segment in dialogues)
        {
            // 设置位置
            messageText.rectTransform.position = segment.targetPosition.transform.position;

            // 初始化文字显示
            messageText.text = "";
            textCanvasGroup.alpha = 0f;

            // 淡入
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

            // 打字效果
            foreach (char c in segment.text)
            {
                messageText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            // 等待该段持续时间
            yield return new WaitForSeconds(segment.duration);

            // 淡出
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        }

        messageText.text = "";
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
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
            textCanvasGroup.alpha = 0f;
            dialogueStarted = false;
        }
    }
}



