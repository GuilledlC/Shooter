using FishNet;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionView : View {

	[SerializeField] private TMP_InputField ipAddress;
	[SerializeField] private Button hostButton;
	[SerializeField] private Button clientButton;
	
	public override void Initialize() {
		hostButton.onClick.AddListener(() => {
			InstanceFinder.TransportManager.Transport.SetClientAddress("localhost");
			InstanceFinder.ServerManager.StartConnection();
			JoinServer();

		});
		clientButton.onClick.AddListener(() => {
			InstanceFinder.TransportManager.Transport.SetClientAddress(ipAddress.text);
			JoinServer();
		});
	}

	private void JoinServer() {
		InstanceFinder.ClientManager.StartConnection();
		ViewManager.Instance.Show<MainView>();
	}
}
