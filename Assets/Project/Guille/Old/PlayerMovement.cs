using System;
using TMPro;
using UnityEngine;

namespace net.GuilledlC.Shooter {

	[RequireComponent(typeof(Rigidbody))]
	public class PlayerMovement : MonoBehaviour {

		[Header("Speed")]
		public float actualSpeed;
		public float Speed;
		public float SprintMultiplier;
		public float WalkMultiplier;
		
		[Header("Ground Control")]
		public float PlayerHeight;
		public LayerMask GroundLayer;
		public float GroundDrag;
		public bool grounded;

		[Header("Jump Control")]
		public float JumpForce;
		public float JumpCooldown;
		public float AirMultiplier;
		private bool readyToJump;

		[Header("Controls")]
		public KeyCode SprintKey = KeyCode.LeftShift;
		public KeyCode WalkKey = KeyCode.LeftAlt;
		public KeyCode JumpKey = KeyCode.Space;
		
		private float horizontalInput;
		private float verticalInput;
		private Vector3 moveDirection;

		public TextMeshProUGUI text_speed;
		
		private Rigidbody rigidbody;

		void Start() {
			rigidbody = GetComponent<Rigidbody>();
			rigidbody.freezeRotation = true;
			readyToJump = true;
		}

		void Update() {
			
			GroundCheck();
			PlayerInput();
			DragControl();
			//SpeedControl();

			Vector3 flatVel = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z);
			text_speed.SetText("Speed: " + flatVel.magnitude);

		}

		void FixedUpdate() {
			
			MovePlayer();
			text_speed.SetText("Speed: " + Mathf.Round(new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z).magnitude * 100));

		}

		private void SpeedControl() {
			Vector3 flatVel = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.z);

			// limit velocity if needed
			if(flatVel.magnitude > Speed) {
				Vector3 limitedVel = flatVel.normalized * Speed;
				rigidbody.linearVelocity = new Vector3(limitedVel.x, rigidbody.linearVelocity.y, limitedVel.z);
			}
		}
		
		private void PlayerInput() {
			
			horizontalInput = Input.GetAxisRaw("Horizontal");
			verticalInput = Input.GetAxisRaw("Vertical");

			actualSpeed = Speed;
			if (Input.GetKey(SprintKey))
				actualSpeed *= SprintMultiplier;
			else if(Input.GetKey(WalkKey))
				actualSpeed *= WalkMultiplier;

			if (Input.GetKeyDown(JumpKey) && grounded && readyToJump) {
				Jump();
				Invoke(nameof(ResetJump), JumpCooldown);
			}

		}

		private void DragControl() {
			
			if (grounded)
				rigidbody.linearDamping = GroundDrag;
			else
				rigidbody.linearDamping = 0;
		}
		
		private void MovePlayer() {
			moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

			if (grounded)
				rigidbody.AddForce(moveDirection.normalized * actualSpeed, ForceMode.Force);
			else
				rigidbody.AddForce(moveDirection.normalized * (actualSpeed * AirMultiplier), ForceMode.Force);
		}

		private void GroundCheck() {
			grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.1f, GroundLayer);
		}

		private void Jump() {
			
			//Reset vertical velocity
			rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x, 0f, rigidbody.linearVelocity.y);
			
			rigidbody.AddForce(transform.up * JumpForce, ForceMode.Impulse);
			readyToJump = false;
		}

		private void ResetJump() {
			readyToJump = true;
		}
	}
}