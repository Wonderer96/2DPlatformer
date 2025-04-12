using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class BlackScreenSubtitleTrigger2D : MonoBehaviour
{
    [Header("Character Control")]
    public MainCharacterControllerRealWorld mainCharacter;
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
        {
            mainCharacter.enabled = false;
        }

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
                    // 处理多个对象的SetActive
                    foreach (var pair in segment.setActivePairs)
                    {
                        if (pair.targetObject != null)
                            pair.targetObject.SetActive(pair.setActiveValue);
                    }
                    break;

                case DialogueType.Teleport:
                    if (segment.targetObject && segment.teleportTargetPosition)
                        segment.targetObject.transform.position = segment.teleportTargetPosition.position;
                    break;

                case DialogueType.MoveTo:
                    if (segment.targetObject && segment.teleportTargetPosition)
                        yield return StartCoroutine(MoveObject(
                            segment.targetObject,
                            segment.teleportTargetPosition.position,
                            segment.moveDuration,
                            segment.faceForward,
                            segment.flipX)); // 新增flipX参数
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
            if (segment.reverseDisplay)
            {
                // 反向显示：完整文字逐个减少
                messageText.text = segment.text;
                for (int i = segment.text.Length; i >= 0; i--)
                {
                    messageText.text = segment.text.Substring(0, i);
                    yield return new WaitForSeconds(typeSpeed);
                }
            }
            else
            {
                // 正向显示：逐个字符显示
                foreach (char c in segment.text)
                {
                    messageText.text += c;
                    yield return new WaitForSeconds(typeSpeed);
                }
            }

            yield return new WaitForSeconds(segment.duration);

            // 淡出字幕
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        }

        // 播放完所有字幕后关闭黑屏
        if (isBlackScreenVisible)
            yield return StartCoroutine(FadeImageAlpha(blackOverlay, 1f, 0f, fadeDuration));

        if (mainCharacter != null)
        {
            mainCharacter.enabled = true;
        }
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

    private IEnumerator MoveObject(GameObject obj, Vector3 targetPos, float duration, bool faceForward, bool flipX)
    {
        Vector3 start = obj.transform.position;
        Vector3 originalScale = obj.transform.localScale;
        float t = 0f;

        Animator animator = obj.GetComponent<Animator>();
        bool isPlayer = obj.layer == LayerMask.NameToLayer("Player");
        bool moveSpeedChanged = false;

        if (isPlayer && animator != null)
        {
            animator.SetFloat("MoveSpeed", 1f);
            moveSpeedChanged = true;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerpT = Mathf.Clamp01(t / duration);
            obj.transform.position = Vector3.Lerp(start, targetPos, lerpT);

            // 2D朝向控制
            Vector3 moveDirection = (targetPos - obj.transform.position).normalized;
            if (moveDirection != Vector3.zero)
            {
                if (flipX)
                {
                    // 通过X轴缩放控制朝向
                    float xScale = Mathf.Abs(originalScale.x);
                    if (faceForward)
                    {
                        obj.transform.localScale = new Vector3(
                            moveDirection.x > 0 ? xScale : -xScale,
                            originalScale.y,
                            originalScale.z
                        );
                    }
                    else
                    {
                        obj.transform.localScale = new Vector3(
                            moveDirection.x > 0 ? -xScale : xScale,
                            originalScale.y,
                            originalScale.z
                        );
                    }
                }
                else
                {
                    // 通过Y轴缩放控制朝向（如果需要）
                    float yScale = Mathf.Abs(originalScale.y);
                    if (faceForward)
                    {
                        obj.transform.localScale = new Vector3(
                            originalScale.x,
                            moveDirection.y > 0 ? yScale : -yScale,
                            originalScale.z
                        );
                    }
                    else
                    {
                        obj.transform.localScale = new Vector3(
                            originalScale.x,
                            moveDirection.y > 0 ? -yScale : yScale,
                            originalScale.z
                        );
                    }
                }
            }

            yield return null;
        }

        obj.transform.position = targetPos;

        if (moveSpeedChanged && animator != null)
        {
            animator.SetFloat("MoveSpeed", 0f);
        }
    }
}

[System.Serializable]
public class SetActivePair
{
    public GameObject targetObject;
    public bool setActiveValue;
}
[System.Serializable]
public class DialogueSegment
{
    [TextArea]
    public string text;
    public float duration = 2f;
    public bool reverseDisplay = false; // 新增反向显示开关

    public DialogueType type = DialogueType.TextOnly;

    [Header("Optional Target Object")]
    public GameObject targetObject;

    [Header("SetActive Settings")]
    public List<SetActivePair> setActivePairs = new List<SetActivePair>(); // 修改为支持多个对象

    [Header("Teleport / MoveTo Settings")]
    public Transform teleportTargetPosition;
    public float moveDuration = 1f;
    public bool faceForward = true; // 新增：移动时是否面朝方向
    public bool flipX = true; // 默认使用X轴翻转来控制朝向

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





