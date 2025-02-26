using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Weapons", order = 1)]
public class WeaponClass : MonoBehaviour //ScriptableObject
{
    public bool isMelee;
    public string gunName;
    public int damage;
    public int maxRounds;
    public int currRounds;
    public int reloadTime;
    public float range;
    
    public GameObject gunModel;

    //Gun logic
    public ParticleSystem impactParticleSystem;
    public TrailRenderer bulletTrail;
    public float fps;
    //bulletspread logic
    public Vector3 bulletVariance;
    public bool bulletSpread;

    //for later add model reference as object too

    public bool isReloading;
    private float lastShootTime;
    public Camera playerCamera;

    public PlayerController playerController;
    
    private void Awake()
    {
        playerCamera = Camera.main;
        playerController = GetComponentInParent<PlayerController>();
    }

    public void Fire()
    {
        if(!isMelee)
        {
            if(currRounds > 0)
            {
                Debug.Log("Fired");
                //implementing secret object pooling technique
                if(lastShootTime + fps < Time.time)
                {
                    Debug.Log("Fired2");
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
                Debug.Log("Out of Ammo");
            }
        }
        else
        {
            if(lastShootTime + fps < Time.time)
            {
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
        
    }


    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if(bulletSpread)
        {
            direction += new Vector3(
                Random.Range(-bulletVariance.x, bulletVariance.x),
                Random.Range(-bulletVariance.y, bulletVariance.y),
                Random.Range(-bulletVariance.z, bulletVariance.z)
            );

            direction.Normalize();
        }
        
        return direction;
    }
    
    //random Trailgenerator
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
        yield return new WaitForSeconds((float)reloadTime);
        
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
