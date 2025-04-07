using UnityEngine;
using System.Linq;

public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public BaseTrigger[] pressurePlates;
    public float moveDistance = 3f;
    public float moveSpeed = 1f;
    public bool reverseLogic = false;  // New bool variable to reverse the door logic

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
        bool allPlatesActivated = pressurePlates.All(plate => plate != null && plate.IsActivated);
        shouldLower = reverseLogic ? !allPlatesActivated : allPlatesActivated;
    }

    private void MoveDoor()
    {
        Vector3 targetPosition = shouldLower ? loweredPosition : originalPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
