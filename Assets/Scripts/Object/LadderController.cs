using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LadderController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<MainCharacterController>().EnterLadder();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<MainCharacterController>().ExitLadder();
        }
    }
}
