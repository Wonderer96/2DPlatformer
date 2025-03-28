using UnityEngine;

public class VelocityDetector : MonoBehaviour
{
    [System.NonSerialized]
    public Vector2 lastRecordedVelocity;
    public Rigidbody2D trackedRb;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            trackedRb = rb;
            lastRecordedVelocity = rb.velocity;
        }
    }

}
