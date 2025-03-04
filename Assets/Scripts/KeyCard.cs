using UnityEngine;

public class KeyCard : MonoBehaviour, InteractableInterface
{
    public void Interact()
    {
        Debug.Log("Successfully Interacted");
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj.GetComponent<PlayerController>()!= null)
        {
            obj.GetComponent<PlayerController>().interactNot.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        if(obj.GetComponent<PlayerController>()!= null)
        {
            obj.GetComponent<PlayerController>().interactNot.SetActive(false);
        }
    }
}
