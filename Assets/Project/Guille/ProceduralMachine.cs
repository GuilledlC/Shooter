using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace net.GuilledlC.Shooter {

	public class ProceduralMachine : MonoBehaviour {

		#region Public Variables

		//Disable this if you dont want to see the extra debugs...
		public bool powerDebug;

		[Header("Base Options")]
		//Height of the player
		public float playerHeight = 1.56f;

		//The radius of the player
		public float playerRadius = 0.45f;

		[Header("Movement Variables")]
		//The overall speed of the character (affects everything including gravity)
		public float movementSpeed = 4.94f;

		//If you want the movement to be smooth (will slightly clip through slopes at the feet but it looks better in FPS Mode)
		public bool smooth = true;

		//The speed of the dampening
		public float smoothSpeed = 9f;

		[Header("Physics Options")]
		//The gravity speed
		public float gravity = 2.8f;

		[Header("Jump Options")]
		//The force of the jump
		public float jumpForce = 6.9f;

		//How fast the jump decreases so you can fall quicker
		public float jumpDecrease = 3.3f;

		//This makes sure there isnt a roof above you for atleast the distance of '2' so you cant clip through walls...
		public float jumpBarrier = 2;

		//The speed of the jump
		public float jumpSpeed = 1.01f;

		[Header("Slope Options")]
		//These options don't work as expected all the time... I would suggest leaving these alone unless you need higher slopes.
		[Range(0, 180)]
		public float minSlope = 0;

		[Range(0, 180)] public float maxSlope = 75;

		[Range(0, 180)]
		//If slope of the ground is higher than 45f the ground check is extended to 4 to make sure you dont fall through the floor...
		public float maxModifiedSlope = 45f;

		//Bigger the more smooth coming down slopes will be... Might cause weird behaviour if too high.
		public float newMinGroundingDist = 4f;

		//This is the original ground value.
		public float oldMinGroundingDist = 2f;

		[Header("Grounding Options")]
		//Leave this...
		public float maxGroundingDist = 50;

		//This point checks if you are ACTUALLY colliding with the floor...
		public Vector3 groundCheckPoint = new Vector3(0, -0.85f, 0);

		//This radius specifies how big of an area you are checking for the ground
		public float groundCheckRadius = 0.35f;

		//Gives you the ability to make the player hover above the ground if you need it...
		public float pushupDist = 0f;

		[Header("Obstacle Options")]
		//The biggest step over an object you can take...
		public float maxStepHeight = 1.57f;

		//The point where a ray is cast to check for the ground..
		public Vector3 liftSlopePoint = new Vector3(0, 1.13f, 0);

		//The radius of the ground check, bigger radius = ability to detect ground around the player
		public float lifeSlopeRadius = 0.2f;

		[Header("Layer Options")]
		//Create a layer for the player and set it then select every layer in the game EXCEPT the player's layer...
		public LayerMask discludePlayer;

		[Header("References")]
		//Create a sphere collider that is where the collision will be detected... Make sure it is higher than the maxStepHeight so that collisions don't affect steps
		public SphereCollider sphereCol;
		//public SphereCollider sphereCol;

		#endregion

		#region Private Variables

		//Grounding Information
		private RaycastHit groundHit = new RaycastHit();
		private float currentGroundSlope;
		private bool grounded;
		private Vector3 groundingClamp;

		private float minGroundingDist = 2f;

		//Movement Information
		private Vector3 velocity;
		public float jumpHeight;
		private Vector3 move;
		private Vector3 lastVelocity;

		private Vector3 rawVelocity;

		//Other
		private bool inputJump = false;

		#endregion

		#region Updates

		private void Update() {
			Gravity();			//Subtracts gravity from velocity Y
			SimpleMove();		//Adds WASD movement to velocity X and Z
			Jump();				//Adds JumpHeight to velocity Y
			FinalMovement();	//Transforms the position using the velocity * DeltaTime and resets the velocity
			GroundChecking();	//Checks ground (I don't know how this works yet)
			CollisionCheck();	//
		}


		#endregion

		#region Movement

		private void SimpleMove() {
			move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			velocity += move;
		}

		private void FinalMovement() {
			Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z) * movementSpeed;
			vel = transform.TransformDirection(vel);
			transform.position += vel * Time.deltaTime;
			lastVelocity = vel * Time.deltaTime;
			rawVelocity = vel;
			if (powerDebug)
				Debug.DrawRay(transform.position, rawVelocity, Color.red, 0.2f);
			velocity = Vector3.zero;
		}

		#endregion

		#region Gravity

		private void Gravity() {
			if (grounded == false) {
				velocity.y -= gravity;
			}
		}

		#endregion

		#region GroundChecking / Snapping

		private void GroundChecking() {

			Ray ray = new Ray(tPoint(liftSlopePoint), Vector3.down);

			//Will be replaced with groundHit if the ground is actually hit...
			RaycastHit tempHit = new RaycastHit();

			if (Physics.SphereCast(ray, lifeSlopeRadius, out tempHit, maxGroundingDist, discludePlayer)) {

				GroundConfirm(tempHit);

			}
			else
				grounded = false;

		}

		private void GroundConfirm(RaycastHit tempHit) {
			float currentSlope = Vector3.Angle(tempHit.normal, Vector3.up);
			if (currentSlope >= maxModifiedSlope)
				minGroundingDist = newMinGroundingDist;
			else
				minGroundingDist = oldMinGroundingDist;

			//Is able to be the ground then... Walls cannot be the ground 
			if (currentSlope >= minSlope && currentSlope <= maxSlope) {
				groundingClamp = new Vector3(transform.position.x, tempHit.point.y + groundCheckRadius / 2,
					transform.position.z);
				Collider[] col = new Collider[3];
				int num = Physics.OverlapSphereNonAlloc(tPoint(groundCheckPoint), groundCheckRadius, col,
					discludePlayer);


				grounded = false;
				for (int i = 0; i < num; i++) {

					//If you are colliding with the ground object then you need to snap...
					if (col[i].transform == tempHit.transform) {
						groundHit = tempHit;
						currentGroundSlope = currentSlope;
						grounded = true;
						if (groundHit.point.y <= transform.position.y + maxStepHeight && inputJump == false) {
							if (!smooth)
								transform.position = new Vector3(transform.position.x,
									(groundHit.point.y + playerHeight / 2 + pushupDist), transform.position.z);
							else
								transform.position = Vector3.Lerp(transform.position,
									new Vector3(transform.position.x,
										(groundHit.point.y + playerHeight / 2 + pushupDist), transform.position.z),
									smoothSpeed * Time.deltaTime);
						}

						break;
					}

				}


				if (num <= 1 && inputJump == false && tempHit.distance <= minGroundingDist) {
					if (col[0] != null) {
						Ray ray = new Ray(tPoint(liftSlopePoint), Vector3.down);
						RaycastHit hit;

						if (Physics.Raycast(ray, out hit, minGroundingDist, discludePlayer)) {
							if (hit.transform != col[0].transform) {
								grounded = false;
								return;
							}
							else { }
						}

					}

					grounded = true;
					if (tempHit.point.y <= transform.position.y + maxStepHeight && inputJump == false) {
						if (!smooth)
							transform.position = new Vector3(transform.position.x,
								(tempHit.point.y + playerHeight / 2 + pushupDist), transform.position.z);
						else
							transform.position = Vector3.Lerp(transform.position,
								new Vector3(transform.position.x, (tempHit.point.y + playerHeight / 2 + pushupDist),
									transform.position.z), smoothSpeed * Time.deltaTime);
					}

					return;
				}

			}


		}

		#endregion

		#region Jumping


		private void Jump() {

			bool canJump = false;

			canJump = !Physics.Raycast(new Ray(transform.position, Vector3.up), jumpBarrier, discludePlayer);

			if (grounded && jumpHeight > 0.2f || jumpHeight <= 0.2f && grounded) {
				jumpHeight = 0;
				inputJump = false;
			}

			if (grounded && canJump) {
				if (Input.GetKeyDown(KeyCode.Space)) {
					inputJump = true;
					transform.position += Vector3.up * groundCheckRadius * 2;
					jumpHeight += jumpForce;
				}

			}
			else {
				if (!grounded) {
					jumpHeight -= (jumpHeight * jumpDecrease * Time.deltaTime);
				}
			}

			velocity.y += jumpHeight;


		}

		#endregion

		#region Collision

		private void CollisionCheck() {

			Collider[] overlaps = new Collider[4];

			int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphereCol.center), sphereCol.radius,
				overlaps, discludePlayer, QueryTriggerInteraction.UseGlobal);

			for (int i = 0; i < num; i++) {

				Transform t = overlaps[i].transform;
				Vector3 dir;
				float dist;
				if (Physics.ComputePenetration(sphereCol, transform.position, transform.rotation, overlaps[i],
					    t.position, t.rotation, out dir, out dist)) {
					Vector3 penetrationVector = dir * dist;
					Vector3 velocityProjected = Vector3.Project(velocity, -dir);
					transform.position = transform.position + penetrationVector;
					velocity -= velocityProjected;
				}


			}


		}

		#endregion

		#region Helper

		public PlayerPrivateData playerData() {
			PlayerPrivateData d = new PlayerPrivateData();
			d.groundHit = groundHit;
			d.rawVelocity = rawVelocity;
			d.currentGroundSlope = currentGroundSlope;
			d.grounded = grounded;
			d.groundingClamp = groundingClamp;
			d.velocity = velocity;
			d.jumpHeight = jumpHeight;
			d.move = move;
			d.inputJump = inputJump;
			d.lastVelocity = lastVelocity;
			return d;
		}

		private Vector3 surfaceDir(Vector3 dir, RaycastHit hit) {
			Vector3 d = transform.TransformDirection(dir);

			Vector3 temp = Vector3.Cross(hit.normal, d);
			//Debug.DrawRay (hit.point, temp * 20, Color.green, 0.2f);

			Vector3 myDirection = Vector3.Cross(temp, hit.normal);
			//Debug.DrawRay (hit.point, myDirection * 20, Color.red, 0.2f);

			return myDirection;
		}

		private void OnDrawGizmos() {

			if (powerDebug) {

				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(transform.position, new Vector3(playerRadius, playerHeight, playerRadius));

				if (!grounded)
					Gizmos.color = Color.red;
				else
					Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(tPoint(groundCheckPoint), groundCheckRadius);

				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(
					new Vector3(transform.position.x, transform.position.y + maxStepHeight, transform.position.z),
					new Vector3(playerRadius, 0.001f, playerRadius));

				Gizmos.color = Color.red;
				Vector3 sPoint = tPoint(liftSlopePoint);
				Gizmos.DrawLine(sPoint, new Vector3(sPoint.x, sPoint.y - maxGroundingDist, sPoint.z));
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(sPoint, lifeSlopeRadius);

				Gizmos.color = Color.magenta;
				Gizmos.DrawWireSphere(groundingClamp, groundCheckRadius);


			}
			else {
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(transform.position, new Vector3(playerRadius, playerHeight, playerRadius));

				if (!grounded)
					Gizmos.color = Color.red;
				else
					Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(tPoint(groundCheckPoint), 0.1f);

				Vector3 sPoint = tPoint(liftSlopePoint);
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(sPoint, 0.1f);


			}

		}

		private Vector3 tPoint(Vector3 p) {
			return transform.TransformPoint(p);
		}

		private Vector3 tDir(Vector3 d) {
			return transform.TransformDirection(d);
		}

		#endregion

	}

	//Ability to pass this class to other scripts to control their behaviour with the current state of this player...

	[System.Serializable]
	public struct PlayerPrivateData {
		//Grounding Information
		public RaycastHit groundHit;
		public float currentGroundSlope;
		public bool grounded;
		public Vector3 groundingClamp;

		//Movement Information
		public Vector3 velocity;
		public Vector3 lastVelocity;
		public Vector3 rawVelocity;
		public float jumpHeight;
		public Vector3 move;

		//Other
		public bool inputJump;


	}
}