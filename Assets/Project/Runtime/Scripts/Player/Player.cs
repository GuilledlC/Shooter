using System.Collections;
using UnityEngine;
using FishNet.Object;
using KinematicCharacterController;

//Thanks to https://www.youtube.com/watch?v=NsSk58un8E0
public class Player : NetworkBehaviour {

	[SerializeField] private PlayerCharacter playerCharacter;
	[SerializeField] private PlayerCamera playerCamera;
	[SerializeField] private PlayerUI playerUI;
	[Space]
	[SerializeField] private CameraSpring cameraSpring;

	private PlayerInputActions _inputActions;
	private Camera mainCamera;
	
	IEnumerator SetupPlayer() {
		yield return new WaitForEndOfFrame();
		
		mainCamera = Camera.main;
		if (mainCamera != null && cameraSpring != null) {
			mainCamera.transform.SetParent(cameraSpring.transform);
			mainCamera.transform.localPosition = Vector3.zero;
			mainCamera.transform.localRotation = Quaternion.identity;
		} else {
			Debug.LogError("Camera or CameraSpring is still null!");
		}
		
		_inputActions.Enable();
	    
		playerCharacter.Initialize();
		playerCamera.Initialize(playerCharacter.GetCameraTarget());
        
		cameraSpring.Initialize();
	}
	
	public override void OnStartClient() {
		base.OnStartClient();
		_inputActions = new PlayerInputActions();
		if (base.IsOwner) {
			StartCoroutine(SetupPlayer());
		}
		else {
			this.enabled = false;
			//playerCamera.gameObject.SetActive(false);
			playerUI.gameObject.SetActive(false);
			playerCharacter.enabled = false;
			playerCharacter.gameObject.GetComponent<KinematicCharacterMotor>().enabled = false;
			_inputActions.Disable();
		}
	}
	
    void Start() {
	    Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDestroy() {
	    _inputActions.Disable();
	    _inputActions.Dispose();
    }

    void Update() {
	    if (base.IsOwner) {
		    var input = _inputActions.Gameplay;
		    var deltaTime = Time.deltaTime;

		    //Get camera input and update its rotation
		    var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
		    playerCamera.UpdateRotation(cameraInput);

		    //Get character input and update it
		    var characterInput = new CharacterInput {
			    Rotation = playerCamera.transform.rotation,
			    Move = input.Move.ReadValue<Vector2>(),
			    Jump = input.Jump.WasPressedThisFrame(),
			    JumpSustain = input.Jump.IsPressed(),
			    Crouch = input.Crouch.IsPressed() ? CrouchInput.Hold : CrouchInput.None,
			    Sprint = input.Sprint.IsPressed()
		    };
		    playerCharacter.UpdateInput(characterInput);
		    playerCharacter.UpdateBody(deltaTime);
		    playerUI.UpdateUI(playerCharacter);
	    }
    }

    private void LateUpdate() {
	    if (base.IsOwner) {
		    var deltaTime = Time.deltaTime;
		    var cameraTarget = playerCharacter.GetCameraTarget();

		    playerCamera.UpdatePosition(cameraTarget);
		    cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
	    }
    }
}
