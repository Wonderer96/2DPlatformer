using UnityEngine;

public class GravityItem : MonoBehaviour
{
    public GameObject icon;
    public GravityController gravityController;
    private void OnTriggerEnter2D(Collider2D other)
    {
        MainCharacterController character = other.GetComponent<MainCharacterController>();
        if (character != null)
        {
            gravityController.enabled = true;
            icon.SetActive(true);
        }
    }

}
