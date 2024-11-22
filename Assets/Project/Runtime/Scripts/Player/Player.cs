using UnityEngine;

//Thanks to https://www.youtube.com/watch?v=NsSk58un8E0
public class Player : MonoBehaviour {

	[SerializeField] private PlayerCharacter playerCharacter;
	[SerializeField] private PlayerCamera playerCamera;
	[SerializeField] private PlayerUI playerUI;
	[Space]
	[SerializeField] private CameraSpring cameraSpring;

	private PlayerInputActions _inputActions;
	
    void Start() {
	    Cursor.lockState = CursorLockMode.Locked;
	    
	    _inputActions = new PlayerInputActions();
	    _inputActions.Enable();
	    
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        
        cameraSpring.Initialize();
    }

    void OnDestroy() {
	    _inputActions.Dispose();
    }

    void Update() {
	    var input = _inputActions.Gameplay;
	    var deltaTime = Time.deltaTime;
	    
	    //Get camera input and update its rotation
	    var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
	    playerCamera.UpdateRotation(cameraInput);
	    
	    //Get character input and update it
	    var characterInput = new CharacterInput {
		    Rotation    = playerCamera.transform.rotation,
		    Move	    = input.Move.ReadValue<Vector2>(),
		    Jump	    = input.Jump.WasPressedThisFrame(),
		    JumpSustain = input.Jump.IsPressed(),
		    Crouch	    = input.Crouch.IsPressed() ? CrouchInput.Hold : CrouchInput.None,
		    Sprint		= input.Sprint.IsPressed()
	    };
	    playerCharacter.UpdateInput(characterInput);
	    playerCharacter.UpdateBody(deltaTime);
	    playerUI.UpdateUI(playerCharacter);
    }

    private void LateUpdate() {
	    var deltaTime = Time.deltaTime;
	    var cameraTarget = playerCharacter.GetCameraTarget();
	    
	    playerCamera.UpdatePosition(cameraTarget);
	    cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
    }
}
