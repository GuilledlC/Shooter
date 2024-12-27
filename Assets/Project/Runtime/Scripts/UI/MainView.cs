using FishNet;
using UnityEngine;
using TMPro;

public class MainView : View {

	[SerializeField]
	private TMP_Text text;

	private void LateUpdate() {
		text.text = "IsServerStarted: " + InstanceFinder.IsServerStarted + " IsClientStarted" +
		            InstanceFinder.IsClientStarted + " IsHostStarted" + InstanceFinder.IsHostStarted;
	}
	
}
