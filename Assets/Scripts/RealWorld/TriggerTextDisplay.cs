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
        public GameObject targetPosition; // ÿ�λ���ʾ��λ��
        public float duration = 3f; // ÿ�λ���ʾʱ��
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
            // ����λ��
            messageText.rectTransform.position = segment.targetPosition.transform.position;

            // ��ʼ��������ʾ
            messageText.text = "";
            textCanvasGroup.alpha = 0f;

            // ����
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

            // ����Ч��
            foreach (char c in segment.text)
            {
                messageText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            // �ȴ��öγ���ʱ��
            yield return new WaitForSeconds(segment.duration);

            // ����
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



