using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class PressurePlate : BaseTrigger
{
    [Header("Pressure Plate Settings")]
    public float pressDistance = 0.2f;
    public float moveSpeed = 1f;

    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    private int activationCount;

    protected override void Awake()
    {
        base.Awake();
        originalPosition = transform.position;
        pressedPosition = originalPosition - Vector3.up * pressDistance;
    }

    void Update()
    {
        UpdatePosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activationCount++;
            UpdateState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
            activationCount = Mathf.Max(0, activationCount - 1);
            UpdateState();
        }
    }

    private void UpdateState()
    {
        IsActivated = activationCount > 0;
    }

    private void UpdatePosition()
    {
        Vector3 target = IsActivated ? pressedPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, target, moveSpeed * Time.deltaTime);
    }
}
