using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

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

    [Header("Jump Mechanics")] 
    public float jumpForce = 100f;
    public bool isJump;

    [Header("Sprint Mechanics")] 
    public int stamina;
    public int maxStamina = 40;
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
    public TextMeshProUGUI healthText;
    public int hp;
    public int healAmount;
    public bool canHeal;


    // Play on awake, sets up player input system then moves are being stored, also initializes weapon system
    private void Awake()
    {
        // Player variables 
        currentWeapon = weapons[0];
        SwitchedGuns();
        
        // Instantiate Input Systems
        playerInputActions = new InputSystem_Actions();

        // Input callbacks
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += Look;
        playerInputActions.Player.Jump.performed += _ => Jump();
        playerInputActions.Player.Sprint.performed += _ => Sprint(2f);
        playerInputActions.Player.Sprint.canceled += _ => Sprint(1f);
        playerInputActions.Player.FireGun.performed += _ => FireCurrentWeapon();
        playerInputActions.Player.Reload.performed += _ => StartCoroutine(currentWeapon.GetComponent<WeaponClass>().Reload());
        playerInputActions.Player.Switch.performed += OnScroll;

        // Player callbacks
        playerInputActions.Player.Heal.performed += _ => StartCoroutine(Heal());
        
        // Rigidbody Variables
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
        Debug.Log(playerRigidbody);
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
        Vector3 wishDirection = Vector3.Normalize(new Vector3(moveInput.x, 0, moveInput.y));
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
        float tempY = mouseDelta.y is > -85 and < 85 ? mouseDelta.y * rotationSpeed : 0;
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
    private void FireCurrentWeapon()
    {
        if (cloneWepMod != null && currentWeapon != null)
        {
            
            currentWeapon.GetComponent<WeaponClass>().Fire();
        }
        else
        {
            Debug.Log("No weapon equipped");
        }
    }

    // Sprint logic modifies movement speed in fixed update
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
        gunText.text = (currentWeaponGunClass) ? "Weapon:" + currentWeaponGunClass.gunName : "Gun: " + currentWeaponGunClass.gunName;
        ammoText.text = (currentWeaponGunClass.isMelee) ? "Infinite" : "Ammo: " + currentWeaponGunClass.currRounds;
    }

    //Heal Stuff-will add slider logic when I have time
    private IEnumerator Heal()
    {
        if (healAmount <= 0) yield break;
        if (canHeal)
        {
            Debug.Log("Healing");
            hp += (hp + 10 > 0) ? 0 : 10; //temp
            healthText.text = "HP: " + hp;
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.Log("Stopped");
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