using UnityEngine;
using TMPro;

public class InteractPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public float interactDistance = 3f;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}