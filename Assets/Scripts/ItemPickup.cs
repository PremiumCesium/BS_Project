using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public bool isHeal;
    public bool isAmmo;
    public int ammoAmount;
    
    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
        if(pc != null){
            pc.healAmount += isHeal ? 10 : 0;
            pc.totalAmmo += isAmmo ? ammoAmount : 0;
            Destroy(this.gameObject);
        }
    }
}
