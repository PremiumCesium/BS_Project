using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // NOTE: Header adds titles for public variables in hierarchy. 
    // The OnEnable, Awake, OnDisable, FixedUpdate, OnCollisionEnter CANNOT have their names chaged even for camelcase or they WONT work
    [SerializeField]
    private InputSystem_Actions playerInputActions;
    private Rigidbody playerRigidbody;
    [Header("Movement Mechanics")]
    public Vector2 moveInput;
    public float moveSpeed = 10f;
    public float rotationSpeed = 1f;
    public float rotationY = 0f;
    public float rotationX = 0f;
    public float dragCoefficient = 0.01f;
    public float maxSpeed = 20f;

    [Header("Jump Mechanics")]
    public float jumpForce = 100f;
    public bool isJump;

    [Header("Sprint Mechanics")]
    public int stamina = 0;
    public int maxStamina = 40;
    public int moveMultiplier = 1;
    public bool canSprint = true;

    [Header("GunMechanics")]
    private Camera fpsCam;
    public TextMeshProUGUI ammoText;
    public List<WeaponClass> weapons;
    public WeaponClass currEquip;
    public GameObject cloneWepMod;
    public GameObject hand;
    public int totalAmmo;
    
    // Play on awake, sets up player input system then moves are being stored, also initializes weapon system
    void Awake()
    {
        currEquip = weapons[0];
        SwitchedGuns();
        fpsCam = Camera.main;
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerInputActions.Player.Look.performed += ctx => Look(ctx);
        playerInputActions.Player.Jump.performed += ctx => Jump();
        playerInputActions.Player.Sprint.performed += ctx => Sprint(2f);
        playerInputActions.Player.Sprint.canceled += ctx => Sprint(1f);
        playerInputActions.Player.FireGun.performed += ctx => StartFiring();
        playerInputActions.Player.Reload.performed += ctx => StartCoroutine(Reload());
        
        // Rigidbody Variables
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;
    }

    // When script is enabled in play mode
    void OnEnable()
    {
        playerInputActions.Enable(); 
    }

    // When script is disabled via scene change or unload
    void OnDisable()
    {
        playerInputActions.Disable(); 
    }

    // Called every frame, movement system via rigidbody and the playerinput
    void FixedUpdate()
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
        float tempY = (mouseDelta.y > -40 && mouseDelta.y < 70) ? mouseDelta.y * rotationSpeed : 0;
        rotationX = Mathf.Clamp(tempY + rotationX, -40, 70);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        this.GetComponent<Rigidbody>().freezeRotation = true;

    }

    // Jump logic, ERROR: player can jump twice? They arent supposed to jump unless they touch the ground
    private void Jump()
    {
        if(!isJump){
            isJump = true;
            Debug.Log("Jump pressed");
            
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            stamina -=3;
        }
    }

    //Checks if player is on ground
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Ground") && other.gameObject.transform.position.y <= this.gameObject.transform.position.y){
            isJump = false;
        }
    }

    //sprint logic modifies movement speed in fixed update
    private void Sprint(float i)
    {
        if(stamina <= 0){
            moveSpeed = 1f;
            StartCoroutine(RegainStamina());
            canSprint = false;
        }
        else{
            moveSpeed = i;
        }
        
    }

    // Stamina logic
    private IEnumerator RegainStamina()
    {
        if(stamina < 40)
        {
            stamina +=1;
            yield return new WaitForSeconds(0.1f);
            if(stamina == 40)
            {
                canSprint = true;
            }
        }
        else{
            yield return new WaitForSeconds(0.9f);
        }
        yield break;
    }

    //Firing Logic-At ScriptibleObjectLayer-using raycast logic
    private void StartFiring()
    {
        if(currEquip.currRounds > 0){
            RaycastHit hit;
            if(Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, currEquip.range)){
                Debug.Log(hit.transform.name);
            }
            currEquip.currRounds -= 1;
            ammoText.text = "Ammo: " + currEquip.currRounds;
            Debug.Log(currEquip.currRounds);
        }

    }

    //swtiching guns logic - Should we may it overlayed on screen? if so how?
    void SwitchedGuns()
    {
        ammoText.text = "Ammo: " + currEquip.currRounds;
        if(cloneWepMod != null){
            Destroy(cloneWepMod);
            cloneWepMod = null;
            
        }
        cloneWepMod = Instantiate(currEquip.gunModel, hand.transform.position, transform.rotation);
        cloneWepMod.transform.SetParent(hand.transform);
    }

    
    //Reloading logic
    private IEnumerator Reload()
    {
        Debug.Log("Starting Reload wait " + currEquip.reloadTime +  " seconds");
        yield return new WaitForSeconds((float) currEquip.reloadTime);
        int deduct = (currEquip.maxRounds - currEquip.currRounds);
        int reloaded = 0;
        if(totalAmmo - deduct >= 0)
        {
            currEquip.currRounds += deduct;
            totalAmmo -= deduct;
        }
        else
        {
            currEquip.currRounds += totalAmmo;
            totalAmmo = 0;
        }
    }





    // To do: crouch, aim, shoot and the rest of the game :/

}
