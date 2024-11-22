using UnityEngine;

public class OldPlayerMovement : MonoBehaviour {
	[Header("Movement")]
	public float moveSpeed;
	public float groundDrag;
	public float jumpForce;
	public float jumpCooldown;
	public float airMultiplier;
	private bool readyToJump = true;

	[Header("Keybinds")]
	public KeyCode jumpKey = KeyCode.Space;
	
	[Header("Ground Check")]
	public float playerHeight;
	public LayerMask whatIsGround;
	private bool grounded;

	public Transform orientation;

	private float horizontalInput;
	private float verticalInput;

	private Vector3 moveDirection;
	private Rigidbody rigidbody;
	
	void Start() {
		rigidbody = GetComponent<Rigidbody>();
		rigidbody.freezeRotation = true;
	}

	void Update() {
		
		//Ground Check
		grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f);
		
	    MyInput();
	    SpeedControl();

	    if (grounded)
		    rigidbody.drag = groundDrag;
	    else
		    rigidbody.drag = 0;
	}

	void FixedUpdate() {
		MovePlayer();
	}
	
    private void MyInput() {
	    horizontalInput = Input.GetAxisRaw("Horizontal");
	    verticalInput = Input.GetAxisRaw("Vertical");

	    if (Input.GetKey(jumpKey) && readyToJump && grounded) {
		    readyToJump = false;
		    Jump();
		    Invoke(nameof(ResetJump), jumpCooldown);
	    }
    }

    private void MovePlayer() {
	    //Calculate movement direction
	    moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
	    
	    if(grounded)
			rigidbody.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
	    else if(!grounded)
		    rigidbody.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void SpeedControl() {
	    Vector3 flatVel = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);

	    if (flatVel.magnitude > moveSpeed) {
		    Vector3 limitedVel = flatVel.normalized * moveSpeed;
		    rigidbody.velocity = new Vector3(limitedVel.x, rigidbody.velocity.y, limitedVel.z);
	    }
    }

    private void Jump() {
	    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
	    
	    rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
	    readyToJump = true;
    }
}
