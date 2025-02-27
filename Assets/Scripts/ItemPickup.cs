using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public bool isHeal;
    public bool isAmmo;
    public int ammoAmount;
    
    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.gameObject.GetComponent<PlayerController>();
        if (pc == null) return;
        
        if (isHeal) 
        {
            pc.health += 20;
            pc.healthText.text = "HP: " + pc.health;
        }
        
        pc.totalAmmo += isAmmo ? ammoAmount : 0;
        Destroy(this.gameObject);
    }
}
