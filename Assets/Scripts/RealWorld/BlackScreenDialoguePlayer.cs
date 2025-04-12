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

            // ��������߼�
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

            // ִ�ж�������
            switch (segment.type)
            {
                case DialogueType.SetActive:
                    // �����������SetActive
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
                            segment.flipX)); // ����flipX����
                    break;

                case DialogueType.ChangeScene:
                    if (!string.IsNullOrEmpty(segment.sceneToLoad))
                    {
                        SceneManager.LoadScene(segment.sceneToLoad);
                        yield break;
                    }
                    break;
            }

            // ������Ļ
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, fadeDuration));

            // ���ֻ�Ч��
            if (segment.reverseDisplay)
            {
                // ������ʾ�����������������
                messageText.text = segment.text;
                for (int i = segment.text.Length; i >= 0; i--)
                {
                    messageText.text = segment.text.Substring(0, i);
                    yield return new WaitForSeconds(typeSpeed);
                }
            }
            else
            {
                // ������ʾ������ַ���ʾ
                foreach (char c in segment.text)
                {
                    messageText.text += c;
                    yield return new WaitForSeconds(typeSpeed);
                }
            }

            yield return new WaitForSeconds(segment.duration);

            // ������Ļ
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, fadeDuration));
        }

        // ������������Ļ��رպ���
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

            // 2D�������
            Vector3 moveDirection = (targetPos - obj.transform.position).normalized;
            if (moveDirection != Vector3.zero)
            {
                if (flipX)
                {
                    // ͨ��X�����ſ��Ƴ���
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
                    // ͨ��Y�����ſ��Ƴ��������Ҫ��
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
    public bool reverseDisplay = false; // ����������ʾ����

    public DialogueType type = DialogueType.TextOnly;

    [Header("Optional Target Object")]
    public GameObject targetObject;

    [Header("SetActive Settings")]
    public List<SetActivePair> setActivePairs = new List<SetActivePair>(); // �޸�Ϊ֧�ֶ������

    [Header("Teleport / MoveTo Settings")]
    public Transform teleportTargetPosition;
    public float moveDuration = 1f;
    public bool faceForward = true; // �������ƶ�ʱ�Ƿ��泯����
    public bool flipX = true; // Ĭ��ʹ��X�ᷭת�����Ƴ���

    [Header("ChangeScene Settings")]
    public string sceneToLoad;

    [Header("Black Screen")]
    public bool useBlackScreen = true; // ÿ����Ļ�Ƿ�ʹ�ú���
}

public enum DialogueType
{
    TextOnly,
    SetActive,
    Teleport,
    MoveTo,
    ChangeScene
}





