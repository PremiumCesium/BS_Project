using UnityEngine;

public class HitBox : MonoBehaviour
{
    public GameObject enemy;
    private Coroutine damageCoroutine;
    public int damageAmount = 5;
    private void OnTriggerEnter(Collider other)
    {
        enemy = other.gameObject;
        if(enemy.GetComponent<EnemyInterface>() == null) return;
        Debug.Log("Enemy in personal space");
        damageCoroutine =  StartCoroutine(this.GetComponent<PlayerController>().DamagePlayer(damageAmount));
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != enemy || damageCoroutine == null) return;
        Debug.Log("enemy exited personal space.");
        StopCoroutine(damageCoroutine);
        damageCoroutine = null;
    }
}
