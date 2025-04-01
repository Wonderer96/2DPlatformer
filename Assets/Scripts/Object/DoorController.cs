using UnityEngine;
using System.Linq;

public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public BaseTrigger[] pressurePlates;
    public float moveDistance = 3f;
    public float moveSpeed = 1f;

    private Vector3 originalPosition;
    private Vector3 loweredPosition;
    private bool shouldLower = false;

    void Start()
    {
        originalPosition = transform.position;
        loweredPosition = originalPosition - Vector3.up * moveDistance;
    }

    void Update()
    {
        CheckPressurePlates();
        MoveDoor();
    }

    private void CheckPressurePlates()
    {
        shouldLower = pressurePlates.All(plate => plate != null && plate.IsActivated);
    }

    private void MoveDoor()
    {
        Vector3 targetPosition = shouldLower ? loweredPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
