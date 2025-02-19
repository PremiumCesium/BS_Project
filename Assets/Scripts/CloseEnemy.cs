using UnityEngine;
using UnityEngine.AI;

public class CloseEnemy : MonoBehaviour, EnemyInterface
{
    [SerializeField]
    public int HP {get; set;} = 100;
    public Transform player;
    public NavMeshAgent agent;

    public void TakeDamage(int amount)
    {
        HP -= amount;
        Debug.Log(HP);
        Debug.Log(amount);
        if(HP <= 0)
        {
            Destroy(this.gameObject);
        }
    }  

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        agent.destination = player.position;
    }
}
