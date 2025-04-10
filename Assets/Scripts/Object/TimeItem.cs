using UnityEngine;

public class TimeItem : MonoBehaviour
{
    public GameObject icon1;
    public GameObject icon2;
    public PlayerReplay playerRplay;
    private void OnTriggerEnter2D(Collider2D other)
    {
        MainCharacterController character = other.GetComponent<MainCharacterController>();
        if (character != null)
        {
            playerRplay.enabled = true;
            icon1.SetActive(true);
            icon2.SetActive(true);
        }
    }

}
