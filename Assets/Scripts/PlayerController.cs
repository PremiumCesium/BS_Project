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
<<<<<<< Updated upstream
    private WeaponClass currentWeapon;
=======
    private Camera playerCamera; // for walljump
>>>>>>> Stashed changes

    [Header("Movement Mechanics")] 
    public Vector2 moveInput;
    public float moveSpeed;
    public float rotationSpeed = 1f;
    public float rotationY;
    public float rotationX;
    public float dragCoefficient = 0.01f;
    public float maxSpeed = 20f;

    [Header("Jump Mechanics")] public float jumpForce = 100f;
    public bool isJump;
    public bool isWallJump;
    public bool collidedWithWall;

    [Header("Sprint Mechanics")] public int stamina;
    public int maxStamina = 40;
    public float moveDefault;
    public float moveSprint;
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
<<<<<<< Updated upstream
        fpsCam = Camera.main;
=======
        moveSpeed = moveDefault;
        playerCamera = Camera.main;
        
        // Instantiate Input Systems
>>>>>>> Stashed changes
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Move.performed +=
            ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled +=
            ctx => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += ctx => Look(ctx);
        playerInputActions.Player.Jump.performed += ctx => Jump();
        playerInputActions.Player.Sprint.performed += ctx => Sprint(2f);
        playerInputActions.Player.Sprint.canceled += ctx => Sprint(1f);
        playerInputActions.Player.FireGun.performed += ctx => fireCurrentWeapon();
        playerInputActions.Player.Reload.performed +=
           ctx => StartCoroutine(Reload());
        playerInputActions.Player.Switch.performed += ctx => OnScroll(ctx);
        playerInputActions.Player.Heal.performed += ctx => StartCoroutine(Heal());
    


<<<<<<< Updated upstream
=======
        // Input callbacks
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += Look;
        playerInputActions.Player.Jump.performed += _ => Jump();
        playerInputActions.Player.Sprint.performed += _ => Sprint(true);
        playerInputActions.Player.Sprint.canceled += _ => Sprint(false);
        playerInputActions.Player.FireGun.performed += _ => currentWeapon.GetComponent<WeaponClass>().Fire();
        playerInputActions.Player.Reload.performed += _ => StartCoroutine(currentWeapon.GetComponent<WeaponClass>().Reload());
        playerInputActions.Player.Switch.performed += OnScroll;
        playerInputActions.Player.Crouch.performed += _ => Crouch(true);
        playerInputActions.Player.Crouch.canceled += _ => Crouch(false);
        playerInputActions.Player.Pause.performed += _ => Pause();
        playerInputActions.Player.Interact.performed += _ => TestInteract();
        
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
<<<<<<< Updated upstream
        Vector3 force = wishDirection * (moveSpeed * moveMultiplier);
=======
        Vector3 force = new Vector3(wishDirection.x * (moveSpeed) * Time.deltaTime, 0, wishDirection.z * (moveSpeed) * Time.deltaTime); //Integral change, bount to x and z now
>>>>>>> Stashed changes

        playerRigidbody.AddForce(force, ForceMode.Impulse);

        // Ensures the player will EVENTUALLY slow down, otherwise the player will be unable to stop to go AFK
        playerRigidbody.linearVelocity *= 1 - dragCoefficient;
    }

    // Looking
    private void Look(InputAction.CallbackContext ctx)
    {
<<<<<<< Updated upstream
=======
        if(isPaused) return;
        if(isWallJump) return;
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        if (isJump) return;
=======
        if(isPaused) return;
        if (isJump && isWallJump) return;
>>>>>>> Stashed changes

        if(isJump && !isWallJump)
        {
            WallJump();
        }
        else
        {
            isJump = true;
            Debug.Log("Jump pressed");

            this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce,
                ForceMode.Impulse);
        }

        
    }

    //Wall Jump logic on FEIN
    private void WallJump()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 10f))
        {
            if(hit.transform.CompareTag("Walls"))
            {
                Debug.Log("Detected Wall:Performing Jump");
                isWallJump = true;
                //cast to rotation variable quaternion
                Quaternion angleJump = Quaternion.Euler(Mathf.Atan(hit.distance)-90f, transform.eulerAngles.y, transform.eulerAngles.z).normalized;
                StartCoroutine(smoothAngleTransition(angleJump));
            }
        }
    }

    //for smooth ahh rotation-Need to Fix
    private IEnumerator smoothAngleTransition(Quaternion jump)
    {
        //start a loop
        while(Mathf.Abs(transform.eulerAngles.x - jump.eulerAngles.x) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, jump, Time.deltaTime * 0.5f);
            if(collidedWithWall)
            {
                break;
            }
            yield return null;
        }
        this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce,
        ForceMode.Impulse);
        transform.rotation = new Quaternion(transform.eulerAngles.x, -transform.eulerAngles.y, transform.eulerAngles.z, 0);
        this.GetComponent<Rigidbody>().AddForce(transform.forward * 2.5f,
        ForceMode.Impulse);
        yield break;
    }

    //Checks if player is on ground
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") &&
            other.gameObject.transform.position.y <=
            this.gameObject.transform.position.y)
        {
            isJump = false;
            isWallJump = false;
        }
        if((other.gameObject.CompareTag("Walls")))
        {
            collidedWithWall = true;
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

<<<<<<< Updated upstream
    //sprint logic modifies movement speed in fixed update
    private void Sprint(float i)
=======
    //ResumeLogic
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;
        Time.timeScale = 1.0f;
    }

    //quit application logic (For now, when we get main menu will change)
    public void Quit()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
        Application.Quit();
    }

    //crouch logic
    private void Crouch(bool i)
    {
        if(isJump) return;
        this.GetComponent<CapsuleCollider>().height = (i) ? 0.5f : hitboxHeight;
    }

    

    // Sprint logic modifies movement speed in fixed update
    private void Sprint(bool i)
>>>>>>> Stashed changes
    {
        if(i){
            if (stamina <= 0)
            {
                moveSpeed = moveDefault;
                StartCoroutine(RegainStamina());
                canSprint = false;
            }
            else
            {
                moveSpeed = moveSprint;
            }
        }
        else
        {
            moveSpeed = moveDefault;
            StartCoroutine(RegainStamina());
        }
    }

    // Stamina logic
    private IEnumerator RegainStamina()
    {
        if (stamina < 40)
        {
            stamina += 1;
            yield return new WaitForSeconds(0.01f);
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

/*

    //Firing Logic-At ScriptableObjectLayer-using raycast logic
    private void StartFiring()
    {
        
    }
/*
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

    /*
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


    // Switching guns logic - Should we may it overlay on screen? if so how?
    private void SwitchedGuns()
    {


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
    



    // To do: crouch, aim, shoot and the rest of the game :/
}