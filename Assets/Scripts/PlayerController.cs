using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //NOTE: Header adds titles for public variables in hierarchy. 
    //The OnEnable, Awake, OnDisable, FixedUpdate, OnCollisionEnter CANNOT have their names chaged even for camelcase or they WONT work
    [SerializeField]
    private InputSystem_Actions playerinputactions;
    [Header("Movment Mechanics")]
    public Vector2 moveInput;
    public float moveSpeed = 1f;
    public float rotationSpeed = 1f;
    public float rotationY = 0f;

    [Header("Jump Mechanics")]
    public float jumpForce = 100f;
    public bool isJump;

    [Header("Sprint Mechanics")]
    public int stamina = 0;
    public int maxStamina = 40;
    public bool canSprint;

    //Play on awake, sets up player input system then moves are being stored
    void Awake()
    {
        playerinputactions = new InputSystem_Actions();
        playerinputactions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerinputactions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerinputactions.Player.Look.performed += ctx => look(ctx);
        playerinputactions.Player.Jump.performed += ctx => jump();
        playerinputactions.Player.Sprint.performed += ctx => sprintonthatThang(2f);
        playerinputactions.Player.Sprint.canceled += ctx => sprintonthatThang(1f);
    }

    //when script is enabled in play mode
    void OnEnable()
    {
        playerinputactions.Enable(); 
    }

    //when script is disabled via scene change or unload
    void OnDisable()
    {
        playerinputactions.Disable(); 
    }

    //called every frame, movement system via rigidbody and the playerinput
    void FixedUpdate()
    {
        this.GetComponent<Rigidbody>().freezeRotation = true;
        if(canSprint && moveSpeed == 2 && stamina > 0)
        {
            stamina -=1;
        }
        else
        {
            if(stamina <= 0)
            {    
                moveSpeed = 1f;
                canSprint = false;
            }
            StartCoroutine(regainStamina());
        }
        if(moveInput.x > 0)
        {
            this.GetComponent<Rigidbody>().AddForce(500f * transform.right * moveSpeed * Time.deltaTime);
            
        }
        else if(moveInput.x < 0)
        {
            this.GetComponent<Rigidbody>().AddForce(500f * -transform.right * moveSpeed * Time.deltaTime);
        }
        else if(moveInput.y > 0)
        {
            this.GetComponent<Rigidbody>().AddForce(500f * transform.forward * moveSpeed * Time.deltaTime);
        }
        else if(moveInput.y < 0)
        {
            this.GetComponent<Rigidbody>().AddForce(-500f * transform.forward * moveSpeed * Time.deltaTime);
        }
        else{
            this.GetComponent<Rigidbody>().linearVelocity = new Vector2(0, this.GetComponent<Rigidbody>().linearVelocity.y);
        }
        if(this.GetComponent<Rigidbody>().linearVelocity.y == 0)
        {
            isJump = false;
        }
        
    }

    //Main method to 
    void look(InputAction.CallbackContext ctx)
    {
        this.GetComponent<Rigidbody>().freezeRotation = false;
        Vector2 mouseDelta = ctx.ReadValue<Vector2>();

        rotationY += mouseDelta.x * rotationSpeed;

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        this.GetComponent<Rigidbody>().freezeRotation = true;

    }

    //Jump logic, ERROR: player can jump twice? They arent supposed to jump unless they touch the ground
    private void jump()
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
    private void sprintonthatThang(float i)
    {
        if(stamina <= 0){
            moveSpeed = 1f;
            StartCoroutine(regainStamina());
            canSprint = false;
        }
        else{
            moveSpeed = i;
        }
        
    }

    //stamina logic
    private IEnumerator regainStamina()
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



    //To do: crouch, aim, shoot

}
