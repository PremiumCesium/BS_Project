using System.Collections;
using UnityEngine;

public class HealBed : MonoBehaviour
{
    private GameObject player;
    private PlayerController playerController;
    private Coroutine healingCoroutine;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;
        Debug.Log("Player entered heal bed.");
        healingCoroutine = StartCoroutine(HealPlayer());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != player || healingCoroutine == null) return;
        Debug.Log("Player exited heal bed.");
        StopCoroutine(healingCoroutine);
        healingCoroutine = null;
    }

    private IEnumerator HealPlayer()
    {
        while (true)
        {
            playerController.health += 5;
            playerController.healthText.text = "HP: " + playerController.health;
            yield return new WaitForSeconds(1f);
        }
    }
}