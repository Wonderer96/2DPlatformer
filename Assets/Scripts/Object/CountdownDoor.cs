using UnityEngine;
using System.Collections;

public class CountdownDoor : MonoBehaviour
{
    [Header("Base Settings")]
    public BaseTrigger trigger;
    public Transform doorTransform;
    public float lowerHeight = 2f;
    public float countdownTime = 5f;
    public float moveSpeed = 2f;

    [Header("UI Settings")]
    private GameObject countdownBar;
    private RectTransform barRect;
    private float originalBarWidth;

    private Vector3 originalPosition;
    private Coroutine currentRoutine;
    private bool wasActivated;

    void Start()
    {
        originalPosition = doorTransform.position;
        FindCountdownBar();
    }

    void Update()
    {
        if (trigger.IsActivated != wasActivated)
        {
            if (trigger.IsActivated)
            {
                StartCountdown();
            }
            wasActivated = trigger.IsActivated;
        }
    }

    void FindCountdownBar()
    {
        countdownBar = GameObject.Find("CountDownBar");
        if (countdownBar != null)
        {
            barRect = countdownBar.GetComponent<RectTransform>();
            originalBarWidth = barRect.sizeDelta.x;
            countdownBar.SetActive(false);
        }
    }

    void StartCountdown()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DoorSequence());
    }

    IEnumerator DoorSequence()
    {
        yield return MoveDoor(originalPosition - Vector3.up * lowerHeight);

        float timer = countdownTime;
        if (countdownBar != null) countdownBar.SetActive(true);

        while (timer > 0)
        {
            UpdateProgressBar(timer / countdownTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        if (countdownBar != null) countdownBar.SetActive(false);
        yield return MoveDoor(originalPosition);

        trigger.ForceDeactivate();
        currentRoutine = null;
    }

    IEnumerator MoveDoor(Vector3 target)
    {
        while (Vector3.Distance(doorTransform.position, target) > 0.01f)
        {
            doorTransform.position = Vector3.MoveTowards(
                doorTransform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        doorTransform.position = target;
    }

    void UpdateProgressBar(float progress)
    {
        if (barRect != null)
        {
            barRect.sizeDelta = new Vector2(originalBarWidth * progress, barRect.sizeDelta.y);
        }
    }
}

