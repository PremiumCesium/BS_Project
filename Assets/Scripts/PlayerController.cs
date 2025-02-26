using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEditor;

public class PlayerController : MonoBehaviour
{
    // NOTE: Header adds titles for public variables in hierarchy. 
    // The OnEnable, Awake, OnDisable, FixedUpdate, OnCollisionEnter CANNOT have their names chaged even for camelcase or they WONT work

    // Player references
    private InputSystem_Actions playerInputActions;
    private Rigidbody playerRigidbody;

    [Header("Movement Mechanics")] 
    public Vector2 moveInput;
    public float moveSpeed = 10f;
    public float rotationSpeed = 1f;
    public float rotationY;
    public float rotationX;
    public float dragCoefficient = 0.01f;
    public float maxSpeed = 20f;

    [Header("Jump Mechanics")] public float jumpForce = 100f;
    public bool isJump;

    [Header("Sprint Mechanics")] public int stamina;
    public int maxStamina = 40;
    public int moveMultiplier = 1;
    public bool canSprint = true;

    [Header("GunMechanics")] public Camera fpsCam;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunText;
    public List<GameObject> weapons;
    public GameObject currEquip;
    public GameObject cloneWepMod;
    public GameObject hand;
    public float lastShootTime;
    public int currentGunIndex;
    public int totalAmmo;
    public WeaponClass wp;

    [Header("HealTest")]
    public TextMeshProUGUI healthText;
    public int HP;
    public int healAmount;
    public bool canHeal;
    

    // Play on awake, sets up player input system then moves are being stored, also initializes weapon system
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currEquip = weapons[currentGunIndex];
        SwitchedGuns();
        fpsCam = Camera.main;
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Move.performed +=
            ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled +=
            ctx => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += ctx => Look(ctx);
        playerInputActions.Player.Jump.performed += ctx => Jump();
        playerInputActions.Player.Sprint.performed += ctx => Sprint(2f);
        playerInputActions.Player.Sprint.canceled += ctx => Sprint(1f);
<<<<<<< Updated upstream
        playerInputActions.Player.FireGun.performed += ctx => StartFiring();
        playerInputActions.Player.Reload.performed +=
            ctx => StartCoroutine(Reload());
=======
        playerInputActions.Player.FireGun.performed += ctx => fireCurrentWeapon();
        playerInputActions.Player.Reload.performed +=
           ctx => StartCoroutine(Reload());
        playerInputActions.Player.Switch.performed += ctx => OnScroll(ctx);
        playerInputActions.Player.Heal.performed += ctx => StartCoroutine(Heal());
>>>>>>> Stashed changes

        // Rigidbody Variables
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
    }

    // When script is enabled in play mode
    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    // When script is disabled via scene change or unload
    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y; // Vertical scrolling

        Debug.Log($"Mouse Scroll: Vertical = {scrollY}");

        currentGunIndex = (scrollY > 1) ? currentGunIndex +1 : currentGunIndex -1;
        currentGunIndex = (currentGunIndex < 0) ? (weapons.Count - 1) : currentGunIndex;
        currentGunIndex = (currentGunIndex >= weapons.Count) ? 0 : currentGunIndex;
        currEquip = weapons[currentGunIndex];
        SwitchedGuns();

    }

    // Called every frame, movement system via rigidbody and the playerinput
    private void FixedUpdate()
    {
        Move();
        if(playerInputActions.Player.FireGun.IsPressed() && wp != null)
        {
            if(wp.currRounds > 0)
            {
                wp.Fire();
            }
        }
    }

    // Movement Code
    private void Move()
    {
        Vector3 wishDirection =
            Vector3.Normalize(new Vector3(moveInput.x, 0, moveInput.y));
        wishDirection = transform.rotation * wishDirection;
        Vector3 force = wishDirection * (moveSpeed * moveMultiplier);

        playerRigidbody.AddForce(force, ForceMode.Impulse);

        // Ensures the player will EVENTUALLY slow down, otherwise the player will be unable to stop to go AFK
        playerRigidbody.linearVelocity *= 1 - dragCoefficient;
    }

    // Looking
    private void Look(InputAction.CallbackContext ctx)
    {
        this.GetComponent<Rigidbody>().freezeRotation = false;
        Vector2 mouseDelta = ctx.ReadValue<Vector2>();

        rotationY += mouseDelta.x * rotationSpeed;
        float tempY = mouseDelta.y is > -85 and < 85
            ? mouseDelta.y * rotationSpeed
            : 0;
        rotationX = Mathf.Clamp(-tempY + rotationX, -40, 70);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        this.GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Jump logic, ERROR: player can jump twice? They aren't supposed to jump unless they touch the ground
    private void Jump()
    {
        if (isJump) return;

        isJump = true;
        Debug.Log("Jump pressed");

        this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce,
            ForceMode.Impulse);
        stamina -= 3;
    }

    //Checks if player is on ground
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") &&
            other.gameObject.transform.position.y <=
            this.gameObject.transform.position.y)
        {
            isJump = false;
        }
    }

    //Fixed fire call to solve ctx issue
    private void fireCurrentWeapon()
    {
        if(cloneWepMod != null && wp != null)
        {
            wp.Fire();
        }
        else
        {
            Debug.Log("No weapon equipped");
        }
    }

    //sprint logic modifies movement speed in fixed update
    private void Sprint(float i)
    {
        if (stamina <= 0)
        {
            moveSpeed = 1f;
            StartCoroutine(RegainStamina());
            canSprint = false;
        }
        else
        {
            moveSpeed = i;
        }
    }

    // Stamina logic
    private IEnumerator RegainStamina()
    {
        if (stamina < 40)
        {
            stamina += 1;
            yield return new WaitForSeconds(0.1f);
            if (stamina == 40)
            {
                canSprint = true;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.9f);
        }
    }
<<<<<<< Updated upstream

    //Firing Logic-At ScriptableObjectLayer-using raycast logic
    private void StartFiring()
    {
        //implementing secret object pooling technique
        if(lastShootTime + currEquip.fps < Time.time)
        {
            if (currEquip.currRounds <= 0) return;
            Vector3 shotDirection = GetDirection();


            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position,
                    fpsCam.transform.forward, out hit, currEquip.range))
            {
                TrailRenderer trail = Instantiate(currEquip.bulletTrail, cloneWepMod.transform.position, Quaternion.identity);

                StartCoroutine(SpawnOnThatThang(trail, hit));
                Debug.Log(hit.transform.name);
                if(hit.transform.gameObject.GetComponent<EnemyInterface>() != null)
                {
                    hit.transform.gameObject.GetComponent<EnemyInterface>().TakeDamage(currEquip.damage);
                }
                
                lastShootTime = Time.time;
            }

            currEquip.currRounds -= 1;
            ammoText.text = "Ammo: " + currEquip.currRounds;
        }
    }

    //for bulletspread: if no bulletspread, then the bullets shoot forward, else random spread
    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if(currEquip.bulletSpread)
        {
            direction += new Vector3(
                Random.Range(-currEquip.bulletVariance.x, currEquip.bulletVariance.x),
                Random.Range(-currEquip.bulletVariance.y, currEquip.bulletVariance.y),
                Random.Range(-currEquip.bulletVariance.z, currEquip.bulletVariance.z)
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
        Instantiate(currEquip.impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(trail.gameObject, trail.time);
        

    }
=======
>>>>>>> Stashed changes

    // Switching guns logic - Should we may it overlay on screen? if so how?
    private void SwitchedGuns()
    {
<<<<<<< Updated upstream
        ammoText.text = "Ammo: " + currEquip.currRounds;
        if (cloneWepMod != null)
        {
            Destroy(cloneWepMod);
            cloneWepMod = null;
        }

        cloneWepMod = Instantiate(currEquip.gunModel, hand.transform.position,
            transform.rotation);
        cloneWepMod.transform.SetParent(hand.transform);
    }


    // Reloading logic
=======
        // currentWeapon = this.GetComponentInChildren<WeaponClass>();
        
        if (cloneWepMod != null)
        {
           Destroy(cloneWepMod);
           cloneWepMod = null;
        }

        cloneWepMod = Instantiate(currEquip, hand.transform.position,
           transform.rotation);
        cloneWepMod.transform.SetParent(hand.transform);
        wp = cloneWepMod.GetComponent<WeaponClass>();
        gunText.text = (wp.isMelee) ? "Weapon:" + wp.gunName : "Gun: " + wp.gunName;
        ammoText.text = (wp.isMelee) ? "Infinite" :"Ammo: " + wp.currRounds;
        wp.playerCamera = Camera.main;
        wp.playerController = this;
    }

    // Reloading logic 
>>>>>>> Stashed changes
    private IEnumerator Reload()
    {   
        if(!wp.isMelee)
        {
            Debug.Log("Starting Reload wait " + wp.reloadTime + " seconds");
            yield return new WaitForSeconds((float)wp.reloadTime);
            int deduct = (wp.maxRounds - wp.currRounds);
            if (totalAmmo - deduct >= 0)
            {
                wp.currRounds += deduct;
                totalAmmo -= deduct;
            }
            else
            {
                wp.currRounds += totalAmmo;
                totalAmmo = 0;
            }
            
            ammoText.text = "Ammo: " + wp.currRounds;
        }
<<<<<<< Updated upstream
        else
        {
            currEquip.currRounds += totalAmmo;
            totalAmmo = 0;
        }
    }
=======
    }

    //Heal Stuff-will add slider logic when I have time
    private IEnumerator Heal()
    {
        if(healAmount > 0)
        {
            if(canHeal)
            {
                Debug.Log("Healing");
                HP += (HP + 10 > 0) ? 0: 10; //temp
                healthText.text = "HP: " + HP;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log("Stopped");
            }
            
        }
        yield break;
    }

    //Take Damage From attacks
    private IEnumerator TakeDamage()
    {
        Debug.Log("Damage");
        yield break;
    }
    
>>>>>>> Stashed changes


    // To do: crouch, aim, shoot and the rest of the game :/
}