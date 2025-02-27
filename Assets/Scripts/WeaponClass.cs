using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapons", order = 1)]
public class WeaponClass : MonoBehaviour //ScriptableObject
{
    // Weapon properties
    public bool isMelee;
    public string gunName;
    public int damage;
    public int maxRounds;
    public int currRounds;
    public int reloadTime;
    public float range;
    public float roundsPerSecond;
    
    // Bullet spread logic
    public Vector3 bulletVariance;
    public bool bulletSpread;
    
    // Internal variables
    private bool isReloading;
    private float lastShootTime;
    private Camera playerCamera;
    public PlayerController playerController;
    
    // Visuals
    public GameObject gunModel;
    public ParticleSystem impactParticleSystem;
    public TrailRenderer bulletTrail;
    
    private void Awake()
    {
        playerCamera = Camera.main;
    }

    public void Fire()
    {
        if(playerController.isPaused) return;
        if(!isMelee)
        {
            if (currRounds <= 0) return;
            
            // Implementing secret object pooling technique
            if(lastShootTime + roundsPerSecond < Time.time)
            {
                if (currRounds <= 0) return;
                Vector3 shotDirection = GetDirection();
                TrailRenderer trail = Instantiate(bulletTrail, transform.position, Quaternion.identity);
                if (Physics.Raycast(playerCamera.transform.position,
                        playerCamera.transform.forward, out RaycastHit hit, range))
                {
                    StartCoroutine(SpawnOnThatThang(trail, hit));
                    Debug.Log(hit.transform.name);
                    if(hit.transform.gameObject.GetComponent<EnemyInterface>() != null)
                    {
                        hit.transform.gameObject.GetComponent<EnemyInterface>().TakeDamage(damage);
                    }
                        
                    lastShootTime = Time.time;
                }
                
                RaycastHit fakeHit = new RaycastHit();
                fakeHit.point = playerCamera.transform.forward * 1000;
                StartCoroutine(SpawnOnThatThang(trail, fakeHit));
            }
            currRounds -= 1;
            playerController.ammoText.text = "Ammo: " +  currRounds.ToString();
        }
        else
        {
            if (!(lastShootTime + roundsPerSecond < Time.time)) return;
            
            Debug.Log("Started Swinging");
            if (Physics.Raycast(playerCamera.transform.position,
                    playerCamera.transform.forward, out RaycastHit hit, range))
            {
                Debug.Log(hit.transform.name);
                if(hit.transform.gameObject.GetComponent<EnemyInterface>() != null)
                {
                    hit.transform.gameObject.GetComponent<EnemyInterface>().TakeDamage(damage * (int)playerController.GetComponent<Rigidbody>().
                        linearVelocity.x);
                }


            }
            else if (Physics.SphereCast(playerCamera.transform.position, 1, 
                         playerCamera.transform.forward, out RaycastHit hittwo, range))
            {
                Debug.Log("Hit: " + hittwo.transform.name);
            }
            lastShootTime = Time.time;
        }
        
    }


    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if (!bulletSpread) return direction;
        
        direction += new Vector3(
            Random.Range(-bulletVariance.x, bulletVariance.x),
            Random.Range(-bulletVariance.y, bulletVariance.y),
            Random.Range(-bulletVariance.z, bulletVariance.z)
        );

        direction.Normalize();

        return direction;
    }
    
    // Random trail generator
    private IEnumerator SpawnOnThatThang(TrailRenderer trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        } 
        
        trail.transform.position = hit.point;
        Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(trail.gameObject, trail.time);
    }
    
    public IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (isMelee) yield break;
        
        isReloading = true;
        Debug.Log("Starting Reload wait " + reloadTime + " seconds");
        yield return new WaitForSeconds(reloadTime);
        
        int missingRounds = maxRounds - currRounds;
        int reloadAmount = Mathf.Min(missingRounds, playerController.totalAmmo);
        
        if (reloadAmount > 0)
        {
            playerController.totalAmmo -= reloadAmount; 
            currRounds += reloadAmount;
        }
        
        isReloading = false;
        playerController.ammoText.text = "Ammo: " + currRounds;
    }
}
