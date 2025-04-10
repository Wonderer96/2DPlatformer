using UnityEngine;

public class ClimbGloves : MonoBehaviour
{
    public GameObject icon;
    private void OnTriggerEnter2D(Collider2D other)
    {
        MainCharacterController character = other.GetComponent<MainCharacterController>();
        if (character != null)
        {
            character.canClimb = true;
            icon.SetActive(true);
        }
    }

}
