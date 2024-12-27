using UnityEngine;

public class ViewManager : MonoBehaviour {
    
	public static ViewManager Instance { get; private set; }

	[SerializeField]
	private bool autoInitialize;
	[SerializeField]
	private View[] views;
	[SerializeField]
	private View defaultView;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		Initialize();
	}

	public void Initialize() {
		foreach (View view in views) {
			view.Initialize();
			view.Hide();
		}
		
		if(defaultView != null)
			defaultView.Show();
	}

	public void Show<TView>() where TView : View {
		foreach (View view in views) {
			if(view is TView)
				view.Show();
			else
				view.Hide();
		}
	}
	
}
