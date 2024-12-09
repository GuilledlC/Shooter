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

		

	#endregion
}
