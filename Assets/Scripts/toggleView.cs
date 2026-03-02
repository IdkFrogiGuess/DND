using UnityEngine;
using UnityEngine.InputSystem;

public class toggleView : MonoBehaviour
{
    public GameObject FirstCam;
    public GameObject ThirdCam;
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.GetComponent<Movement>();
        Player.GetComponent<FirstPerMovment>();
        ThirdCam.SetActive(true);
        FirstCam.SetActive(false);
    }
    public void VeiwToggle(InputAction.CallbackContext context)
    {
        ThirdCam.SetActive(!ThirdCam.activeSelf);
        if (ThirdCam.activeSelf) { FirstCam.SetActive(false); }
        else { FirstCam.SetActive(true); }
        Debug.Log($"view toggled");
    }
    void Update()
    {
        if (ThirdCam.activeSelf)
        {
            Player.GetComponent<Movement>().enabled = true;
            Player.GetComponent<FirstPerMovment>().enabled = false;
        }
        else
        {
            Player.GetComponent<Movement>().enabled = false;
            Player.GetComponent<FirstPerMovment>().enabled = true;
        }
    }
}
