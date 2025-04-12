using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BlackScreenSubtitleTrigger2D : MonoBehaviour
{
    [Header("References")]
    public GameObject displayObject;
    public TextMeshProUGUI messageText;
    public CanvasGroup textCanvasGroup;
    public Image blackOverlay;

    [Header("Dialogue Settings")]
    public List<DialogueSegment> dialogues;
    public float typeSpeed = 0.05f;
    public float fadeDuration = 0.5f;

    [Header("Input")]
    public KeyCode activationKey = KeyCode.J;

    private bool playerInside = false;
    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = true;
            displayObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            playerInside = false;
            displayObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInside && !hasPlayed && Input.GetKeyDown(activationKey))
        {
            hasPlayed = true;
            displayObject.SetActive(false);
            StartCoroutine(PlayDialogues());
        }
    }

    private IEnumerator PlayDialogues()
    {
        bool isBlackScreenVisible = false;

        foreach (DialogueSegment segment in dialogues)
        {
            messageText.text = "";

            // 处理黑屏逻辑
            if (segment.useBlackScreen && !isBlackScreenVisible)
            {
                if (blackOverlay != null && !blackOverlay.gameObject.activeSelf)
                    blackOverlay.gameObject.SetActive(true);
                yield return StartCoroutine(FadeImageAlpha(blackOverlay, 0f, 1f, fadeDuration));
                isBlackScreenVisible = true;
            }
            else if (!segment.useBlackScreen && isBlackScreenVisible)
            {
                yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, fadeDuration));
                isBlackScreenVisible = false;
            }

            // 执行动作类型
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

                case DialogueType.MoveTo:
                    if (segment.targetObject && segment.teleportTargetPosition)
                        yield return StartCoroutine(MoveObject(segment.targetObject, segment.teleportTargetPosition.position, segment.moveDuration));
                    break;

                case DialogueType.ChangeScene:
                    if (!string.IsNullOrEmpty(segment.sceneToLoad))
                    {
                        SceneManager.LoadScene(segment.sceneToLoad);
                        yield break;
                    }
                    break;
            }

            // 淡入字幕
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

            // 打字机效果
            foreach (char c in segment.text)
            {
                messageText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(segment.duration);

            // 淡出字幕
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        }

        // 播放完所有字幕后关闭黑屏
        if (isBlackScreenVisible)
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, fadeDuration));

        messageText.text = "";
    }

    private IEnumerator FadeImageAlpha(Image image, float from, float to, float duration)
    {
        Color color = image.color;
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            color.a = Mathf.Lerp(from, to, t);
            image.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = to;
        image.color = color;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float time = 0f;
        group.alpha = from;
        while (time < duration)
        {
            float t = time / duration;
            group.alpha = Mathf.Lerp(from, to, t);
            time += Time.deltaTime;
            yield return null;
        }
        group.alpha = to;
    }

    private IEnumerator MoveObject(GameObject obj, Vector3 targetPos, float duration)
    {
        Vector3 start = obj.transform.position;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            obj.transform.position = Vector3.Lerp(start, targetPos, lerpT);
            yield return null;
        }

        obj.transform.position = targetPos;
    }
}

[System.Serializable]
public class DialogueSegment
{
    [TextArea]
    public string text;
    public float duration = 2f;

    public DialogueType type = DialogueType.TextOnly;

    [Header("Optional Target Object")]
    public GameObject targetObject;

    [Header("SetActive Settings")]
    public bool setActiveValue;

    [Header("Teleport / MoveTo Settings")]
    public Transform teleportTargetPosition;
    public float moveDuration = 1f;

    [Header("ChangeScene Settings")]
    public string sceneToLoad;

    [Header("Black Screen")]
    public bool useBlackScreen = true; // 每段字幕是否使用黑屏
}

public enum DialogueType
{
    TextOnly,
    SetActive,
    Teleport,
    MoveTo,
    ChangeScene
}





