using System.Collections;
using UnityEngine;

public class HealBed : MonoBehaviour
{
    public GameObject player;
    public PlayerController playerController;
    private Coroutine healingCoroutine;
    public int healAmount = 5;

    //Cleaned it up a bit-So this uses start and stop coroutine to loop a continuously looping thang
    private void OnTriggerEnter(Collider other)
    {
        player = other.gameObject;
        if(player.GetComponent<PlayerController>() == null) return;
        Debug.Log("Player entered heal bed.");
        healingCoroutine =  StartCoroutine(player.GetComponent<PlayerController>().HealPlayer(healAmount));
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player || healingCoroutine == null) return;
        Debug.Log("Player exited heal bed.");
        StopCoroutine(healingCoroutine);
        healingCoroutine = null;
    }

    // private IEnumerator HealPlayer()
    // {
    //     while (true)
    //     {
    //         playerController.health += 5;
    //         playerController.healthText.text = "HP: " + playerController.health;
    //         yield return new WaitForSeconds(1f);
    //     }
    // }
}