using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PressurePlate : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("可以触发压力板的标签列表")]
    public List<string> activatorTags = new List<string>() { "Player" };
    public float pressDistance = 0.2f;
    public float moveSpeed = 1f;

    [Header("Debug")]
    [SerializeField] private int activeCount = 0;
    [SerializeField] private bool isActivated = false;

    private Vector3 originalPosition;
    private Vector3 pressedPosition;

    public bool IsActivated => isActivated;

    void Start()
    {
        originalPosition = transform.position;
        pressedPosition = originalPosition - Vector3.up * pressDistance;
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        UpdatePosition();
        UpdateActivationState();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activeCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activeCount = Mathf.Max(0, activeCount - 1);
        }
    }

private void UpdatePosition()
    {
        Vector3 targetPosition = activeCount > 0 ? pressedPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    private void UpdateActivationState()
    {
        bool newState = activeCount > 0;
        if (newState != isActivated)
        {
            isActivated = newState;
        }
    }
}
