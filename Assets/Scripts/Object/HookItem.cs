using UnityEngine;

public class HookItem : MonoBehaviour
{
    public GameObject icon;
    public GameObject monster;
    public GrapplingHook grapplingHook;
    private void OnTriggerEnter2D(Collider2D other)
    {
        MainCharacterController character = other.GetComponent<MainCharacterController>();
        if (character != null)
        {
            grapplingHook.enabled = true;
            icon.SetActive(true);
            icon.SetActive(false);
        }
    }

}
