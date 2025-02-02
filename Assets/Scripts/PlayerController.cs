using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private InputSystem_Actions playerinputactions; //to store the input manager
    //variables for unity new input system
    private Vector2 moveInput;
    private float rotationSpeed = 1f;
    private float rotationY = 0f;

    //jumpLogic
    [Header("Jump Mechanics")]
    public float jumpForce = 100f;
    public bool isJump;

    void Awake(){
        playerinputactions = new InputSystem_Actions(); //instantiate a input manager

        //for move actions basically just instantiates the move commands via script
        //ctx stands for callback context look it up im too lazy to explain in comments
        playerinputactions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerinputactions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        playerinputactions.Player.Look.performed += ctx => Look(ctx);
        playerinputactions.Player.Jump.performed += ctx => Jump();
    }

    //when script is enabled in play mode
    void OnEnable()
    {
        playerinputactions.Enable(); 
    }

    //when script is disabled via scene change or unload
    void OnDisable(){
        playerinputactions.Disable(); 
    }

    //called every frame
    void FixedUpdate(){
        //propels movement via Rigidbodies velocity system
        this.GetComponent<Rigidbody>().freezeRotation = true;
        if(moveInput.x > 0){
            this.GetComponent<Rigidbody>().AddForce(500f * transform.right * Time.deltaTime);
        }
        else if(moveInput.x < 0){
            this.GetComponent<Rigidbody>().AddForce(500f * -transform.right * Time.deltaTime);
        }
        else if(moveInput.y > 0){
            this.GetComponent<Rigidbody>().AddForce(500f * transform.forward * Time.deltaTime);
        }
        else if(moveInput.y < 0){
            this.GetComponent<Rigidbody>().AddForce(-500f * transform.forward * Time.deltaTime);
        }
        else{
            this.GetComponent<Rigidbody>().linearVelocity = new Vector2(0, this.GetComponent<Rigidbody>().linearVelocity.y);
        }
        if(this.GetComponent<Rigidbody>().linearVelocity.y == 0){
            isJump = false;
        }
        
    }

    //it just rotates the player via calculated the rotation by position of the mouse and then using Quaternion
    void Look(InputAction.CallbackContext ctx){
        this.GetComponent<Rigidbody>().freezeRotation = false;
        Vector2 mouseDelta = ctx.ReadValue<Vector2>();

        rotationY += mouseDelta.x * rotationSpeed; //where 10 is the rotation speed

        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
        this.GetComponent<Rigidbody>().freezeRotation = true;

    }

    //ALL OF THIS IS JUMP LOGIC
    private void Jump(){
        if(!isJump){
            Debug.Log("Jump pressed");
            isJump = true;
            this.GetComponent<Rigidbody>().useGravity=false;
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            this.GetComponent<Rigidbody>().useGravity=true;
        }
    }

    private void OnCollisionEnter(Collision other){
        if(other.gameObject.CompareTag("Ground") && other.gameObject.transform.position.y <= this.gameObject.transform.position.y){
            isJump = false;
        }
    }

    //To do for next time when we get basic models
    //implement sprint
    //work on movement so it is from perpective of the camera
    //enable crouch
    //enable aiming methods
    //fix camera setting-only for educational purposes

}
