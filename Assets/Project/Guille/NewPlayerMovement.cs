using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.GuilledlC.Shooter {
	
	public class NewPlayerMovement : MonoBehaviour {
	
		#region Public Variables

		[Header("Base Options")]
		[Tooltip("Height of the player")]
		public float playerHeight = 1.7f;
		[Tooltip("The radius of the player")]
		public float playerRadius = 0.45f;
		
		[Header("Physics")]
		[Tooltip("The gravity applied to our player")]
		public float gravity = 9.8f;
		
		[Header("Movement")]
		[Tooltip("The speed at which our player moves")]
		public float speed;
		[Tooltip("The speed of the dampening")]
		public float smoothSpeed = 9f;
		[Tooltip("How much the speed increases while sprinting")]
		public float sprintMultiplier = 1.7f;
		[Tooltip("How much the speed decreases while walking")]
		public float walkMultiplier = 0.5f;
		
		[Header("Jump Settings")]
		[Tooltip("The force with which our player jumps")]
		public float jumpForce;
		[Tooltip("How much the jump force decreases over time")]
		public float jumpDecrease;
		[Tooltip("The minimum distance to an object from our head such that we can't jump")]
		public float jumpBarrier;
		
		[Header("Grounding Options")]
		public float maxGroundingDist = 50;
		[Tooltip("This point checks if you are ACTUALLY colliding with the floor")]
		public Vector3 groundCheckPoint = new Vector3(0, -0.85f, 0);
		[Tooltip("This radius specifies how big of an area you are checking for the ground")]
		public float groundCheckRadius = 0.35f;
		[Tooltip("Gives you the ability to make the player hover above the ground if you need it")]
		public float pushupDist = 0f;

		[Header("Slope Options")]
		//These options don't work as expected all the time... I would suggest leaving these alone unless you need higher slopes.
		[Range(0, 180)]
		public float minSlope = 0;
		[Range(0, 180)]
		public float maxSlope = 75;
		[Range(0, 180)]
		[Tooltip("If slope of the ground is higher than 45f the ground check is extended to 4 to make sure you dont fall through the floor")]
		public float maxModifiedSlope = 45f;
		[Tooltip("Bigger the more smooth coming down slopes will be... Might cause weird behaviour if too high")]
		public float newMinGroundingDist = 4f;
		//This is the original ground value.
		public float oldMinGroundingDist = 2f;
		
		[Header("Obstacle Settings")]
		[Tooltip("The biggest step over an object you can take")]
		public float maxStepHeight = 1.57f;
		[Tooltip("The point where a ray is cast to check for the ground")]
		public Vector3 liftSlopePoint = new Vector3(0, 1.13f, 0);
		[Tooltip("The radius of the ground check, bigger radius = ability to detect ground around the player")]
		public float lifeSlopeRadius = 0.2f;
		
		[Header("Layers")]
		public LayerMask groundLayer;
		public LayerMask discludePlayer;
		
		[Header("References")]
		public BoxCollider collider;
		
		[Header("Keybinds")]
		public KeyCode jumpKey = KeyCode.Space;
		public KeyCode sprintKey = KeyCode.LeftShift;
		public KeyCode walkKey = KeyCode.X;
		public KeyCode crouchKey = KeyCode.C;
		
		#endregion

		#region Private Variables

		private float jumpHeight;		//The jump force applied to the velocity
		private Vector3 velocity;		//The player's velocity
		private Vector3 lastVelocity;	//Last frame's velocity
		private float speedMultiplier;
		private float minGroundingDist = 2f;
		private bool inputJump = false;
		private float currentPlayerHeight;
		
		//Grounding Information
		private RaycastHit groundHit = new();
		private float currentGroundSlope;
		private bool grounded;			//If the player is touching the ground
		private Vector3 groundingClamp;

		#endregion

		void Update() {
			Gravity();					//Subtracts gravity from velocity Y
			PlayerInput();				//Adds WASD movement to velocity X and Z
			Crouch();
			Jump();						//Adds jumpHeight to velocity Y
			ApplyMovement();			//Transforms the position using velocity * deltaTime and resets the velocity
			GroundCheck();
			CollisionCheck();
			
		}
		
		#region Gravity

		private void Gravity() {
			if (!grounded)				//If the player is in mid-air
				velocity.y -= gravity;	//apply gravity
		}

		#endregion

		#region Player Input

		private void PlayerInput() {
			float horizontalInput = Input.GetAxisRaw("Horizontal");		//AD
			float verticalInput = Input.GetAxisRaw("Vertical");			//WS

			if (Input.GetKey(sprintKey))
				speedMultiplier = sprintMultiplier;
			else if (Input.GetKey(walkKey) || Input.GetKey(crouchKey))
				speedMultiplier = walkMultiplier;
			else
				speedMultiplier = 1;
			
			velocity += new Vector3(horizontalInput, 0f, verticalInput) * speedMultiplier;
		}

		#endregion

		#region Crouch

		private void Crouch() {

			if (grounded && !inputJump && Input.GetKey(crouchKey)) {
				currentPlayerHeight = playerHeight / 2;
			}
			else {
				currentPlayerHeight = playerHeight;
			}

			collider.size = new Vector3(collider.size.x, currentPlayerHeight, collider.size.z);

		}

		#endregion
		
		#region Jump

		private void Jump() {
			//Checking for things above us
			bool canJump = !Physics.Raycast(transform.position, Vector3.up, jumpBarrier + playerHeight/2, discludePlayer);

			if (grounded && jumpHeight > 0.2f || jumpHeight <= 0.2f && grounded) {
				jumpHeight = 0;
				inputJump = false;
			}
			
			if (grounded) {									//If we're on the floor,
				if (canJump && Input.GetKeyDown(jumpKey)) {	//theres nothing above us, and we want to jump,
					inputJump = true;
					transform.position += Vector3.up * (groundCheckRadius * 2);
					jumpHeight += jumpForce;				//then jump
				}
			}
			else //Because it is an impulse we need to decrease the amount of force we are applying over time
				jumpHeight -= (jumpHeight * jumpDecrease * Time.deltaTime);

			velocity.y += jumpHeight;
		}

		#endregion
		
		#region Apply Movement

		private void ApplyMovement() {
			Vector3 finalVelocity = transform.TransformDirection(velocity * speed); //Calculate final velocity
			lastVelocity = finalVelocity * Time.deltaTime; //Save this frame's velocity
			transform.position += lastVelocity; //Apply transform
			velocity = Vector3.zero; //Reset velocity
		}

		#endregion

		#region Ground Check

		private void GroundCheck() {

			Ray ray = new Ray(transform.TransformPoint(liftSlopePoint), Vector3.down);
			
			//Will be replaced with groundHit if the ground is actually hit...
			RaycastHit tempHit = new RaycastHit();

			if (Physics.SphereCast(ray, lifeSlopeRadius, out tempHit, maxGroundingDist, discludePlayer))
				GroundConfirm(tempHit);
			else
				grounded = false;
		}

		private void GroundConfirm(RaycastHit tempHit) {

			float currentSlope = Vector3.Angle(tempHit.normal, Vector3.up);
			if (currentSlope > maxModifiedSlope)
				minGroundingDist = newMinGroundingDist;
			else
				minGroundingDist = oldMinGroundingDist;

			//Is able to be the ground then... Walls cannot be the ground 
			if (currentSlope >= minSlope && currentSlope <= maxSlope) {
				groundingClamp = new Vector3(transform.position.x, tempHit.point.y + groundCheckRadius / 2, transform.position.z);
				Collider[] colliders = new Collider[3];
				int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint), groundCheckRadius,
					colliders, discludePlayer);

				grounded = false;

				for (int i = 0; i < num; i++) {
					//If you are colliding with the ground object then you need to snap...
					if (colliders[i].transform == tempHit.transform) {
						groundHit = tempHit;
						currentGroundSlope = currentSlope;
						grounded = true;
						if (groundHit.point.y <= transform.position.y + maxStepHeight && inputJump == false) //Snap
							transform.position = Vector3.Lerp(								//Move from
								transform.position,										//the current position
								new Vector3(												//to
									transform.position.x,									//the same X,
									(groundHit.point.y + currentPlayerHeight / 2 + pushupDist),	//ground + position Y,
									transform.position.z),									//same Z,
								smoothSpeed * Time.deltaTime);								//smoothly
						
						break;
					}
				}

				if (num <= 1 && inputJump == false && tempHit.distance <= minGroundingDist) {
					if (colliders[0] != null) {
						Ray ray = new Ray(transform.TransformPoint(liftSlopePoint), Vector3.down);
						RaycastHit hit;

						if (Physics.Raycast(ray, out hit, minGroundingDist, discludePlayer)) {
							if (hit.transform != colliders[0].transform) {
								grounded = false;
								return;
							}
						}
					}

					grounded = true;
					if (tempHit.point.y <= transform.position.y + maxStepHeight && inputJump == false) //Snap
						transform.position = Vector3.Lerp(								//Move from
							transform.position,										//the current position
							new Vector3(												//to
								transform.position.x,									//the same X,
								(tempHit.point.y + currentPlayerHeight / 2 + pushupDist),	//ground + position Y,
								transform.position.z),									//same Z,
							smoothSpeed * Time.deltaTime);								//smoothly
					
					return;
				}
			}

		}

		#endregion

		#region Collision Check

		private void CollisionCheck() {

			Collider[] overlaps = new Collider[4];

			//The center of the sphere at the start of the capsule
			Vector3 point0 = transform.TransformPoint(collider.center + new Vector3(0f, currentPlayerHeight/2 - collider.size.x / 2, 0f));
			//The center of the sphere at the end of the capsule
			Vector3 point1 = transform.TransformPoint(collider.center - new Vector3(0f, currentPlayerHeight/2 - collider.size.x / 2, 0f));
			
			int num = Physics.OverlapCapsuleNonAlloc(point0, point1, collider.size.x / 2, overlaps, discludePlayer,
				QueryTriggerInteraction.UseGlobal);

			for (int i = 0; i < num; i++) {
				//Current collider
				Transform t = overlaps[i].transform;
				Vector3 dir;
				float dist;
				if (Physics.ComputePenetration(
					    //Us
					    collider, transform.position, transform.rotation,
					    //Them
					    overlaps[i], t.position, t.rotation,
					    //Output
					    out dir, out dist)) {
					Vector3 penetrationVector = dir * dist;
					if (penetrationVector.normalized.Equals(Vector3.up))
						jumpHeight = 0;
					Vector3 velocityProjected = Vector3.Project(velocity, -dir);
					transform.position = transform.position + penetrationVector;
					velocity -= velocityProjected;
				}
			}

		}

		#endregion
		
	}

}
