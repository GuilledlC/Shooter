using FishNet.Object;
using UnityEngine;
using KinematicCharacterController;

public enum CrouchInput {
	None,
	Hold
}

public enum Stance {
	Stand,
	Crouch,
	Sprint,
	Dive
}

public struct CharacterState {
	public bool Grounded;
	public Stance Stance;
	public float Speed;
	public float Stamina;
}

public struct CharacterMovementInput {
	public Quaternion Rotation;
	public Vector2 Move;
	public bool Jump;
	public bool JumpSustain;
	public CrouchInput Crouch;
	public bool Sprint;
}

//Thanks to https://www.youtube.com/watch?v=NsSk58un8E0
public class PlayerCharacter : NetworkBehaviour, ICharacterController {

	#region Serialized Attributes

		[SerializeField] private KinematicCharacterMotor motor;
		[SerializeField] private Transform root;
		[SerializeField] private CameraTarget cameraTarget;
		[Space]
		[SerializeField] private float maxStamina = 3f;
		[SerializeField] private float minStamina = 0.1f;
		[SerializeField] private float staminaCooldown = 0.5f;
		[Tooltip("How much faster the stamina bar fills up")]
		[SerializeField] private float staminaRecMult = 2f;
		[Space]
		[SerializeField] private float walkSpeed = 20f;
		[SerializeField] private float crouchSpeed = 7f;
		[SerializeField] private float sprintSpeed = 30f;
		[Space]
		[SerializeField] private float walkResponse = 25f;
		[SerializeField] private float crouchResponse = 20f;
		[SerializeField] private float sprintResponse = 30f;
		[Space]
		[SerializeField] private float airSpeed = 15f;
		[SerializeField] private float airAcceleration = 180;
		[Space]
		[SerializeField] private float diveSpeed = 24f;
		[SerializeField] private float diveTime = 0.3f;
		[SerializeField] private float diveStaminaCost = 0.2f;
		[SerializeField] private float diveGravityMultiplier = 0.7f;
		[SerializeField] private float diveJumpNegateMultiplier = 0.4f;
		[Space]
		[SerializeField] private float jumpSpeed = 20f;
		[SerializeField] private float coyoteTime = 0.2f;
		[SerializeField] private float jumpStaminaCost = 0.3f;
		[SerializeField] private float jumpSustainGravityMultiplier = 0.55f;
		[SerializeField] private float gravity = -90f;
		[SerializeField] private float terminalFallSpeed = -50f;
		[Space]
		[SerializeField] private float standHeight = 2f;
		[SerializeField] private float crouchHeight = 1f;
		[SerializeField] private float crouchHeightResponse = 15f;
		[Range(0f, 1f)]
		[SerializeField] private float standCameraHeight = 0.9f;
		[Range(0f, 1f)]
		[SerializeField] private float crouchCameraHeight = 0.7f;

	#endregion

	#region Private Attributes

		private CharacterState _state;
		private CharacterState _lastState;
		private CharacterState _tempState;
		
		private float _currentStamina;

		private Quaternion _requestedRotation;
		private Vector3 _requestedMovement;
		private bool _requestedJump;
		private bool _requestedSustainedJump;
		private bool _requestedCrouch;
		private bool _requestedSprint;

		private float _timeSinceUngrounded;
		private float _timeSinceJumpRequest;
		private bool _ungroundedDueToJump;

		private float _timeSinceNoStamina;
		
		private Collider[] _uncrouchOverlapResults;

	#endregion
	
	#region RPCs

	[ServerRpc]
	private void ChangeCapsuleHeightServer(GameObject player, Vector3 cameraTargetHeight, Vector3 rootTargetScale) {
		ChangeCapsuleHeight(player, cameraTargetHeight, rootTargetScale);
	}

	[ObserversRpc(ExcludeOwner = true, BufferLast = true)]
	private void ChangeCapsuleHeight(GameObject player, Vector3 cameraTargetHeight, Vector3 rootTargetScale) {
		player.GetComponent<PlayerCharacter>().UpdateCapsuleHeight(cameraTargetHeight, rootTargetScale);
	}
			
	[ServerRpc]
	private void ChangeMotorHeightServer(GameObject player, float radius, float height, float yOffset) {
		ChangeMotorHeight(player, radius, height, yOffset);
	}

	[ObserversRpc(ExcludeOwner = true, BufferLast = true)]
	private void ChangeMotorHeight(GameObject player, float radius, float height, float yOffset) {
		player.GetComponent<PlayerCharacter>().UpdateMotorHeight(radius, height, yOffset);
	}

	#endregion
	
	private void UpdateCapsuleHeight(Vector3 cameraTargetHeight, Vector3 rootTargetScale) {
		cameraTarget.UpdateLocalPosition(cameraTargetHeight);
		root.localScale = rootTargetScale;
	}

	private void UpdateMotorHeight(float radius, float height, float yOffset) {
		motor.SetCapsuleDimensions(radius, height, yOffset);
	}
	
	
	public void Initialize() {
		_state.Stance = Stance.Stand;
		_state.Speed = 0;
		_state.Stamina = maxStamina;
		_lastState = _state;
		
		_uncrouchOverlapResults = new Collider[5];
		_currentStamina = maxStamina;
		
		motor.CharacterController = this;
	}

	//Since this method is called every frame by the Player, and the KinematicCharacterController methods
	//(UpdateRotation, UpdateVelocity) are called every physics tick by the character motor, we are going,
	//and this method could get called more than once between physics ticks or not at all, we are going to
	//queue requests for the character motor to use in its next physics update
	public void UpdateInput(CharacterMovementInput movementInput) {
		//Rotation input
		_requestedRotation = movementInput.Rotation;
		//Takes the 2D input vector and transforms it into a 3D vector onto the XZ plane
		_requestedMovement = new Vector3(movementInput.Move.x, 0, movementInput.Move.y);
		//Normalizes it so diagonal movement is the same as any movement
		_requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1);
		//Orient it so it's relative to the direction the player is facing
		_requestedMovement = movementInput.Rotation * _requestedMovement;

		//Jump input
		var wasRequestingJump = _requestedJump;				//Previous jump request
		_requestedJump = _requestedJump || movementInput.Jump;		//Current jump request
		if (_requestedJump && !wasRequestingJump)
			_timeSinceJumpRequest = 0f;						//Reset the jump timer
		
		_requestedSustainedJump = movementInput.JumpSustain;
		
		//Crouch input
		_requestedCrouch = movementInput.Crouch is CrouchInput.Hold;
		
		//Sprint input
		_requestedSprint = movementInput.Sprint;
	}
	
	public void UpdateBody(float deltaTime) {
		
		var currentHeight = motor.Capsule.height;
		var normalizedHeight = currentHeight / standHeight;
		
		var cameraTargetHeight = currentHeight * (
			_state.Stance is Stance.Crouch ? crouchCameraHeight : standCameraHeight);

		var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

		var newCameraScale = Vector3.Lerp(
			cameraTarget.GetLocalPosition(),
			new Vector3(0f, cameraTargetHeight, 0f),
			1f - Mathf.Exp(-crouchHeightResponse * deltaTime));
		var newRootScale = Vector3.Lerp(
			root.localScale,
			rootTargetScale,
			1f - Mathf.Exp(-crouchHeightResponse * deltaTime));

		UpdateCapsuleHeight(newCameraScale, newRootScale);
		ChangeCapsuleHeightServer(this.gameObject, newCameraScale, newRootScale);
	}

	public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
		
		//We need to update the character's rotation to face
		//in the same direction as the requested rotation
		
		//We don't want the character to pitch up and down,
		//so we need to "flatten" the direction the character looks
		
		//We do this by projecting the direction onto a flat ground plane

		var forward = Vector3.ProjectOnPlane(
			_requestedRotation * Vector3.forward,
			motor.CharacterUp);
		
		if(forward != Vector3.zero)
			currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
	}
	
	private void OnDrawGizmos() {
		
		/*Gizmos.color = Color.black;
		Gizmos.DrawLine(motor.transform.position, motor.transform.position + motor.CharacterForward * 5);
		
		Vector3 origin = transform.position;
		Vector3 offset = -motor.CharacterUp * 0.02f; 
		Vector3 direction = motor.CharacterForward;
		
		//We approach the wall from OUTSIDE
		bool forwardEdge = Physics.Raycast(origin + offset + direction, -direction, out RaycastHit hit, 3f);
		//Do another one but rotated 30 degrees to get a plane
		Vector3 secondVector = Quaternion.AngleAxis(15, motor.CharacterUp) * direction;
		bool secondEdge = Physics.Raycast(origin + offset + secondVector, -secondVector, out RaycastHit hit2, 3f);
		//Get top of imaginary plane
		Vector3 topOfPlane = hit2.point - offset + motor.CharacterUp;
		
		//Get the normal of the plane
		Vector3 AB = hit2.point - hit.point;
		Vector3 AC = topOfPlane - hit.point;
		Vector3 planeNormal = Vector3.Cross(AB, AC);
		
		Vector3 edgePoint = hit.point - offset;
		Vector3 newTargetVelocity = edgePoint - origin;
				
		Vector3 leftover = direction - newTargetVelocity;
		float mag = leftover.magnitude;
		leftover = Vector3.ProjectOnPlane(leftover, planeNormal).normalized * mag * 10;
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(hit.point - offset, leftover);

		if (forwardEdge) {
			//Draw first hitscan
			Gizmos.color = Color.green;
			Gizmos.DrawLine(origin, hit.point);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(hit.point, 0.05f);
		}

		if (secondEdge) {
			//Draw second hitscan
			Gizmos.color = Color.green;
			Gizmos.DrawLine(origin, hit2.point);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(hit2.point, 0.05f);
		}

		if (forwardEdge && secondEdge) {
			//Draw the plane
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(hit.point, topOfPlane);
			Gizmos.DrawLine(hit2.point, topOfPlane);
	
			//Draw the normal of the plane
			Gizmos.color = Color.black;
			Gizmos.DrawLine(topOfPlane, topOfPlane + planeNormal);
		}
		*/
		
	}

	public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {

		var deltaStamina = deltaTime * staminaRecMult;
		
		//If the player is grounded
		if (motor.GroundingStatus.IsStableOnGround) {

			if (_lastState.Stance == Stance.Dive) {
				_state.Stance = Stance.Stand;
				_requestedCrouch = true;
			}
			
			_timeSinceUngrounded = 0f;					//Reset the ungrounded timer
			_ungroundedDueToJump = false;				//Uncheck the ungrounded due to jump flag
			
			//Snap the movement to the angle of the surface the character is walking on
			var groundedMovement = motor.GetDirectionTangentToSurface(
				direction: _requestedMovement,
				surfaceNormal: motor.GroundingStatus.GroundNormal) * _requestedMovement.magnitude;

			//Calculate the speed and character's responsiveness based on the stance
			float speed = walkSpeed;
			float response = walkResponse;
			switch (_state.Stance) {
				case Stance.Crouch:
					speed = crouchSpeed;
					response = crouchResponse;
					break;
				case Stance.Sprint:
					var canSprint = _state.Grounded;
					var hasStamina = _currentStamina > minStamina;
					var isInCoolDown = _timeSinceNoStamina <= staminaCooldown;
					if (canSprint && hasStamina && !isInCoolDown) {
						speed = sprintSpeed;
						response = sprintResponse;
						deltaStamina = -deltaTime;					//Tick stamina
					}
					else {
						_state.Stance = Stance.Stand;
					}
					break;
				default:  //Redundant because of the init values
					speed = walkSpeed;
					response = walkResponse;
					break;
			}
			
			//Set target velocity
			var targetVelocity = groundedMovement * speed;
			//And smoothly move it along the ground in that direction
			currentVelocity = Vector3.Lerp(
				currentVelocity,
				targetVelocity,
				1 - Mathf.Exp(-response * deltaTime));
			
		}
		//If the player is in the air
		else {

			deltaStamina = 0;
			_timeSinceUngrounded += deltaTime;			//Tick the ungrounded timer

			//Diving
			if (_requestedCrouch) {

				var canDive = _timeSinceUngrounded < diveTime && _ungroundedDueToJump;
				var wasSprinting = _lastState.Stance == Stance.Sprint;
				if (canDive && wasSprinting) {

					_state.Stance = Stance.Dive;
					_requestedCrouch = false;
					_currentStamina -= diveStaminaCost;

					//Set minimum forward speed to the dive speed
					var currentForwardSpeed = Vector3.Dot(currentVelocity, motor.CharacterForward);
					var targetForwardSpeed = Mathf.Max(currentForwardSpeed, diveSpeed);
					//Add the difference
					currentVelocity += motor.CharacterForward * (targetForwardSpeed - currentForwardSpeed);
					
					//Negate some of the jump's vertical speed
					var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
					currentVelocity -= motor.CharacterUp * (currentVerticalSpeed * diveJumpNegateMultiplier);
				}
			}
			
			//Move in the air
			if (_requestedMovement.sqrMagnitude > 0f) {
				//Requested movement projected onto movement plane with preserved magnitude
				var planarMovement = Vector3.ProjectOnPlane(
					_requestedMovement,
					motor.CharacterUp
					) * _requestedMovement.magnitude;
				
				//Current planar velocity on movement plane
				var currentPlanarVelocity = Vector3.ProjectOnPlane(
					currentVelocity,
					motor.CharacterUp);
				
				//Calculate the movement force
				//Will be changed depending on current velocity
				var movementForce = planarMovement * (airAcceleration * deltaTime);
				
				//If moving slower than the max air speed, treat movementForce as a simple steering force
				if (currentPlanarVelocity.magnitude < airSpeed) {
					//Add it to the current planar velocity to get the target velocity
					var targetPlanarVelocity = currentPlanarVelocity + movementForce;
				
					//Limit target velocity to air speed
					targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);
				
					//Steer towards current velocity
					movementForce += targetPlanarVelocity - currentPlanarVelocity;
				}
				//Otherwise, nerf the movement force when it is in the direction of the current planar velocity
				//to prevent accelerating further beyond the max air speed
				else if(Vector3.Dot(currentPlanarVelocity, movementForce) > 0f) {
					//Project movement force onto the plane whose normal is the current planar velocity
					var constrainedMovementForce = Vector3.ProjectOnPlane(
						movementForce,
						currentPlanarVelocity.normalized);
					movementForce = constrainedMovementForce;
				}
				
				currentVelocity += movementForce;
			}
			
			//Apply gravity
			var effectiveGravity = gravity;
			var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
			
			if (_requestedSustainedJump && verticalSpeed > 0)
				effectiveGravity *= jumpSustainGravityMultiplier;
			else if (_state.Stance is Stance.Dive)
				effectiveGravity *= diveGravityMultiplier;
			
			currentVelocity += motor.CharacterUp * (effectiveGravity * deltaTime);
			
			//Clamp to terminal velocity
			verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
			if (verticalSpeed < terminalFallSpeed) {
				currentVelocity -= motor.CharacterUp * verticalSpeed;
				currentVelocity += motor.CharacterUp * terminalFallSpeed;
			}
		}
		
		//Jumping
		if (_requestedJump) {

			var grounded = motor.GroundingStatus.IsStableOnGround;
			var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;
			if (grounded || canCoyoteJump) {
				_requestedJump = false;
				_currentStamina -= jumpStaminaCost;
			
				//Unstick the player from the ground
				motor.ForceUnground(time: 0.1f);
				_ungroundedDueToJump = true;			//Check the ungrounded due to jump flag
			
				//Set minimum vertical speed to the jump speed
				var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
				var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
				//Add the difference
				currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
			}
			else {
				_timeSinceJumpRequest += deltaTime;		//Tick the jump timer

				//Defer the jump request until coyote time has passed
				var canJumpLater = _timeSinceJumpRequest < coyoteTime;
				_requestedJump = canJumpLater;
			}
		}
		
		_timeSinceNoStamina += deltaTime;				//Tick stamina timer
		_currentStamina += deltaStamina;				//Tick stamina
		if (_currentStamina <= 0)
			_timeSinceNoStamina = 0f;					//Reset stamina timer
		//Clamp stamina
		_currentStamina = Mathf.Clamp(_currentStamina, 0f, maxStamina);
		
		//Update character state
		_state.Speed = currentVelocity.magnitude; //Vector3.ProjectOnPlane(currentVelocity, motor.CharacterUp).magnitude;
		_state.Stamina = _currentStamina;

	}
	
	public void BeforeCharacterUpdate(float deltaTime) {

		_tempState = _state;
		
		var grounded = motor.GroundingStatus.IsStableOnGround;
		if (_state.Stance is Stance.Stand && grounded) {
			//Crouching
			if (_requestedCrouch) {
				_state.Stance = Stance.Crouch;
				UpdateMotorHeight(
					radius: motor.Capsule.radius,
					height: crouchHeight,
					yOffset: crouchHeight * 0.5f);
				ChangeMotorHeightServer(
					player: gameObject,
					radius: motor.Capsule.radius,
					height: crouchHeight,
					yOffset: crouchHeight * 0.5f);
			}
			//Sprinting
			else if (_requestedSprint) {
				_state.Stance = Stance.Sprint;
			}
			
		}
		
	}
	public void PostGroundingUpdate(float deltaTime) { }

	public void AfterCharacterUpdate(float deltaTime) {
		
		//Uncrouching                          | this is a very soddy check but sometimes when you finish
		//									   v diving you get stuck in a crouch height while your stance is Stand
		if ((_state.Stance is Stance.Crouch || motor.Capsule.height == crouchHeight) && !_requestedCrouch) {
			//Stand up to check if colliding
			UpdateMotorHeight(
				radius: motor.Capsule.radius,
				height: standHeight,
				yOffset: standHeight * 0.5f);
			ChangeMotorHeightServer(
				player: gameObject,
				radius: motor.Capsule.radius,
				height: standHeight,
				yOffset: standHeight * 0.5f);


			//Check if we're colliding
			var pos = motor.TransientPosition;
			var rot = motor.TransientRotation;
			var mask = motor.CollidableLayers;
			if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0) {
				//Crouch again
				UpdateMotorHeight(
					radius: motor.Capsule.radius,
					height: crouchHeight,
					yOffset: crouchHeight * 0.5f);
				ChangeMotorHeightServer(
					player: gameObject,
					radius: motor.Capsule.radius,
					height: crouchHeight,
					yOffset: crouchHeight * 0.5f);
			}
			else {
				_state.Stance = Stance.Stand;
			}
		}
		else if (_state.Stance is Stance.Sprint && !_requestedSprint) {
			_state.Stance = Stance.Stand;
		}

		//Update state to reflect relevant motor properties
		_state.Grounded = motor.GroundingStatus.IsStableOnGround;
		//Update _lastState to store the character state snapshot
		//taken at the beggining of this character update
		_lastState = _tempState;
	}
	public bool IsColliderValidForCollisions(Collider coll) {
		return true;
	}

	public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

	public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
		ref HitStabilityReport hitStabilityReport) { }

	public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
		Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

	public void OnDiscreteCollisionDetected(Collider hitCollider) { }

	public Transform GetCameraTarget() => cameraTarget.transform;
	
	public float GetMaxStamina() => maxStamina;
	
	public CharacterState GetCharacterState() => _state;
}
