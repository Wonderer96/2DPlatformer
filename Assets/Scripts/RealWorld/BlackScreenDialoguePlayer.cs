using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BlackScreenSubtitleTrigger2D : MonoBehaviour
{
    public enum DialogueType
    {
        TextOnly,
        SetActive,
        Teleport,
        ChangeScene

    }

    [System.Serializable]
    public class DialogueSegment
    {
        public DialogueType type = DialogueType.TextOnly;

        [TextArea] public string text;
        public float duration = 3f;

        [Header("Optional for SetActive / Teleport")]
        public GameObject targetObject;
        public bool setActiveValue;
        public Transform teleportTargetPosition;
        [Header("Optional for ChangeScene")]
        public string sceneToLoad;

    }

    [Header("Trigger Settings")]
    public LayerMask playerLayer;
    public GameObject promptObject;

    [Header("UI Elements")]
    public Image blackOverlay;
    public TextMeshProUGUI messageText;

    [Header("Dialogue Settings")]
    public List<DialogueSegment> dialogues = new List<DialogueSegment>();
    public float typeSpeed = 0.05f;
    public float fadeDuration = 0.5f;

    private bool playerInside = false;
    private bool dialogueStarted = false;
    private CanvasGroup textCanvasGroup;

    private void Start()
    {
        if (promptObject) promptObject.SetActive(false);
        SetImageAlpha(blackOverlay, 0f);

        textCanvasGroup = messageText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
            textCanvasGroup = messageText.gameObject.AddComponent<CanvasGroup>();

        textCanvasGroup.alpha = 0f;
        messageText.text = "";
    }

    private void Update()
    {
        if (playerInside && !dialogueStarted && Input.GetKeyDown(KeyCode.J))
        {
            if (promptObject) promptObject.SetActive(false);
            dialogueStarted = true;
            StartCoroutine(PlayDialogues());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            playerInside = true;
            if (promptObject) promptObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            playerInside = false;
            if (!dialogueStarted && promptObject) promptObject.SetActive(false);
        }
    }

    private IEnumerator PlayDialogues()
    {
        if (blackOverlay != null && !blackOverlay.gameObject.activeSelf)
            blackOverlay.gameObject.SetActive(true);

        yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, fadeDuration));

        foreach (DialogueSegment segment in dialogues)
        {
            messageText.text = "";

            switch (segment.type)
            {
                case DialogueType.SetActive:
                    if (segment.targetObject)
                        segment.targetObject.SetActive(segment.setActiveValue);
                    break;

                case DialogueType.Teleport:
                    if (segment.targetObject && segment.teleportTargetPosition)
                        segment.targetObject.transform.position = segment.teleportTargetPosition.position;
                    break;

                case DialogueType.ChangeScene:
                    if (!string.IsNullOrEmpty(segment.sceneToLoad))
                    {
                        SceneManager.LoadScene(segment.sceneToLoad);
                        yield break; // 中断后续字幕播放
                    }
                    break;
            }


            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

            foreach (char c in segment.text)
            {
                messageText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(segment.duration);
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        }

        yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, fadeDuration));

        // 你可以根据需求取消注释下面这行，使其结束后隐藏黑屏：
        // blackOverlay.gameObject.SetActive(false);

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

    private IEnumerator FadeImageAlpha(Image img, float from, float to, float duration)
    {
        float t = 0f;
        Color c = img.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}




