using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleArea : MonoBehaviour
{
    public List<GameObject> Enemy;
    public GameObject detectedPlayer;
    void Awake()
    {
        Enemy = new List<GameObject>();
        
        Collider[] colliders = Physics.OverlapBox(transform.position, GetComponent<BoxCollider>().size / 2, transform.rotation);

        foreach (Collider col in colliders)
        {
            
            if(col.gameObject.GetComponent<CloseEnemy>() != null)
            {
                Enemy.Add(col.gameObject);
            }
        }
        Debug.Log(Enemy.Count);
        if(Enemy.Count > 0 && detectedPlayer != null)
        {  

        }
    }

    private void Update()
    {
        if(Enemy.Count > 0 && detectedPlayer != null)
        {  

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<CloseEnemy>() != null)
        {
            Enemy.Add(other.gameObject);
        }

        if (other.gameObject.GetComponent<PlayerController>() == null) return;
        detectedPlayer = other.gameObject;
        if(Enemy.Count > 0 && detectedPlayer != null)
        {  

        }
    }

    private void OnTriggerExit(Collider other){
        if(other.gameObject.GetComponent<CloseEnemy>() != null)
        {
            for(int i = 0; i < Enemy.Count; i++)
            {
                if(Enemy[i].name == other.gameObject.name)
                {
                    Enemy.RemoveAt(i);
                }
            }
        }

        if (other.gameObject.GetComponent<PlayerController>() == null) return;
        detectedPlayer = null;
    }
}
