using UnityEngine;

public interface EnemyInterface
{
    int HP { get; set;}

    void TakeDamage(int amount);

    //If you can think of anything else put it here
}
