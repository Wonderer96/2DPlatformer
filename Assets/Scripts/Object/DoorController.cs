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

        // 使用门自身的“下方”方向来计算目标位置
        Vector3 localDown = transform.TransformDirection(Vector3.down);
        loweredPosition = originalPosition + localDown * moveDistance;
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
