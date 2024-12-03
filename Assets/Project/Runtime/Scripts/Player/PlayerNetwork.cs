using UnityEngine;
using FishNet.Object;

public class PlayerNetwork : NetworkBehaviour {
	
	public override void OnStartClient() {
		base.OnStartClient();
		if (!base.IsOwner) {
			GetComponent<PlayerNetwork>().enabled = false;
		}
	}

	#region Height

		[ServerRpc]
		public void ChangeCapsuleHeightServer(GameObject player, Vector3 cameraTargetHeight, Vector3 rootTargetScale) {
			ChangeCapsuleHeight(player, cameraTargetHeight, rootTargetScale);
		}

		[ObserversRpc(ExcludeOwner = true, BufferLast = true)]
		public void ChangeCapsuleHeight(GameObject player, Vector3 cameraTargetHeight, Vector3 rootTargetScale) {
			player.GetComponent<PlayerCharacter>().UpdateCapsuleHeight(cameraTargetHeight, rootTargetScale);
		}
		
		[ServerRpc]
		public void ChangeMotorHeightServer(GameObject player, float radius, float height, float yOffset) {
			ChangeMotorHeight(player, radius, height, yOffset);
		}

		[ObserversRpc(ExcludeOwner = true, BufferLast = true)]
		public void ChangeMotorHeight(GameObject player, float radius, float height, float yOffset) {
			player.GetComponent<PlayerCharacter>().UpdateMotorHeight(radius, height, yOffset);
		}

	#endregion
}
