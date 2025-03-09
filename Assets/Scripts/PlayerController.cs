using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    // Internal player references
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
    public float hitboxHeight;

    [Header("Jump Mechanics")] 
    public float jumpForce = 100f;
    public bool isJump;
    public bool isWallJump;
    public bool collidedWithWall;

    [Header("Sprint Mechanics")] 
    public int stamina;
    public int maxStamina = 40;
    public float moveDefault;
    public float moveSprint;
    public int moveMultiplier = 1;
    public bool canSprint = true;

    [Header("GunMechanics")] 
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI gunText;
    public List<GameObject> weapons;
    public GameObject cloneWepMod;
    public GameObject hand;
    public int currentGunIndex;
    public int totalAmmo;
    public GameObject currentWeapon;

    [Header("Health")] 
    public Collider hitBox;
    public TextMeshProUGUI healthText;
    public int health = 100;
    public int maxHealth = 100;

    [Header("UI")]
    //Menu
    public GameObject pauseMenu;
    public bool isPaused;
    //Interact
    public GameObject interactNot;

    


    
    // Play on awake, sets up player input system then moves are being stored, also initializes weapon system
    private void Awake()
    {
        // Player variables 
        currentWeapon = weapons[0];
        SwitchedGuns();
        moveSpeed = moveDefault;
        // Instantiate Input Systems
        playerInputActions = new InputSystem_Actions();

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
        
        // Rigidbody Variables
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        Debug.Log(playerRigidbody);
        healthText.text = "HP: " + health;
    }

    // When script is enabled in play mode
    private void OnEnable()
    {
        playerInputActions.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // When script is disabled via scene change or unload
    private void OnDisable()
    {
        playerInputActions.Disable();
    }
    
    private void OnScroll(InputAction.CallbackContext ctx)
    {
        if(isPaused) return;
        Vector2 scrollDelta = ctx.ReadValue<Vector2>();
        float scrollY = scrollDelta.y; // Vertical scrolling
        
        currentGunIndex = (scrollY > 1) ? currentGunIndex + 1 : currentGunIndex - 1;
        currentGunIndex = (currentGunIndex < 0) ? (weapons.Count - 1) : currentGunIndex;
        currentGunIndex = (currentGunIndex >= weapons.Count) ? 0 : currentGunIndex;
        currentWeapon = weapons[currentGunIndex];
        SwitchedGuns();
    }

    // Called every frame, movement system via rigidbody and the player input
    private void FixedUpdate()
    {
        Move();

    }
    
    
    // Movement Code
    private void Move()
    {
        if(isPaused) return;
        Vector3 wishDirection = Vector3.Normalize(new Vector3(moveInput.x, 0, moveInput.y));
        wishDirection = transform.rotation * wishDirection;
        Vector3 force = new Vector3(wishDirection.x * (moveSpeed * Time.deltaTime), 0, wishDirection.z * (moveSpeed * Time.deltaTime)); //Integral change, bount to x and z now

        playerRigidbody.AddForce(force, ForceMode.Impulse);

        // Ensures the player will EVENTUALLY slow down, otherwise the player will be unable to stop to go AFK
        playerRigidbody.linearVelocity *= 1 - dragCoefficient;
    }

    //interact method
    private void TestInteract()
    {
        if(isPaused) return;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 50f))
        {
            GameObject detectedObj= hit.transform.gameObject;
            if(detectedObj.GetComponent<InteractableInterface>() != null)
            {
                detectedObj.GetComponent<InteractableInterface>().Interact();
            }
            else
            {
                Debug.Log("Failed");
            }
        }
        else
        {
            Debug.Log("Failed");
        }
    }

    // Looking
    private void Look(InputAction.CallbackContext ctx)
    {
        if(isPaused) return;
        this.GetComponent<Rigidbody>().freezeRotation = false;
        Vector2 mouseDelta = ctx.ReadValue<Vector2>();

        rotationY += mouseDelta.x * rotationSpeed;
        float tempY = mouseDelta.y is > -85 and < 85 ? mouseDelta.y * rotationSpeed : 0;
        rotationX = Mathf.Clamp(-tempY + rotationX, -40, 70);
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        this.GetComponent<Rigidbody>().freezeRotation = true;
    }

    // Jump logic, ERROR: player can jump twice? They aren't supposed to jump unless they touch the ground
    private void Jump()
    {
        if(isPaused) return;
        if (isJump && isWallJump) return;
        if(isJump)
        {
            WallJump();
        }
        isJump = true;
        
        Debug.Log("Jump pressed");

        this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce,
            ForceMode.Impulse);
    }

    //Wall Jump logic on FEIN
    private void WallJump()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f))
        {
            if(hit.transform.CompareTag("Walls"))
            {
                Debug.Log("Detected Wall:Performing Jump");
                isWallJump = true;
                //cast to rotation variable quaternion(NEED TO IMPLEMENT CORRECTLY)
                Quaternion angleJump = Quaternion.Euler(Mathf.Atan(hit.distance)-10f, 180f+transform.eulerAngles.y, transform.eulerAngles.z).normalized;
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
            isPaused = true;
            transform.rotation = Quaternion.Slerp(transform.rotation, jump, Time.deltaTime * 0.5f);
            if(collidedWithWall)
            {
                this.GetComponent<Rigidbody>().AddForce(Vector3.up * (jumpForce/2),
            ForceMode.Impulse);
                break;
            }
            yield return null;
        }
        transform.rotation = jump;
        this.GetComponent<Rigidbody>().AddForce(transform.forward * 0.5f,
            ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        isPaused = false;
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

    //Checks if player is in airs
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") &&
            other.gameObject.transform.position.y <=
            this.gameObject.transform.position.y)
        {
            isJump = true;
            
        }
    }
    
    //Pause Logic
    private void Pause()
    {
        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        isPaused = true;
        Time.timeScale = 0.0f;
    }

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
    {
        if(i){
            if (stamina > 0)
            {
                moveSpeed = moveSprint;
            }
        }
        else
        {
            moveSpeed = moveDefault;
        }
        
        if (stamina <= 0)
        {
            moveSpeed = 1f;
            StartCoroutine(RegainStamina());
            canSprint = false;
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

    // Switching guns logic - Should we may it overlay on screen? if so how?
    private void SwitchedGuns()
    {
        if (cloneWepMod)
        {
            Destroy(cloneWepMod);
            cloneWepMod = null;
        }

        cloneWepMod = Instantiate(currentWeapon, hand.transform.position, transform.rotation);
        cloneWepMod.transform.SetParent(hand.transform);
        currentWeapon = cloneWepMod;
        WeaponClass currentWeaponGunClass = currentWeapon.GetComponent<WeaponClass>();
        currentWeaponGunClass.playerController = this;
        gunText.text = (currentWeaponGunClass) ? "Weapon:" + currentWeaponGunClass.gunName : "Gun: " + currentWeaponGunClass.gunName;
        ammoText.text = (currentWeaponGunClass.isMelee) ? "Infinite" : "Ammo: " + currentWeaponGunClass.currRounds;
    }

    //Heal Stuff-will add slider logic when I have time
    public IEnumerator HealPlayer(int healAmount)
    {
        while (true)
        {
            if (healAmount <= 0) yield break;
            {
                Debug.Log("Healing");
                int healMin = Mathf.Min(maxHealth - health, healAmount);
                health += healMin;
                healthText.text = "HP: " + health;
                yield return new WaitForSeconds(1f);
            }
        }
        
    }
    
    //Heal Stuff-will add slider logic when I have time
    public IEnumerator DamagePlayer(int damageAmount)
    {
        while (true)
        {
            if (damageAmount <= 0) yield break;
            {
                Debug.Log("Damaging");
                int healMin = Mathf.Min(maxHealth - health, damageAmount);
                health -= healMin;
                healthText.text = "HP: " + health;
                yield return new WaitForSeconds(1f);
            }
        }
        
    }
    

    //Take Damage From attacks
    public IEnumerator TakeDamage()
    {
        Debug.Log("Damage");
        yield break;
    }


    // To do: crouch, aim, shoot and the rest of the game :/
}