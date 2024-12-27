using UnityEngine;

public abstract class View : MonoBehaviour {
	
    public bool isInitialized { get; private set; }

    public virtual void Initialize() {
	    isInitialized = true;
    }

    public virtual void Show() {
	    gameObject.SetActive(true);
    }

    public virtual void Hide() {
	    gameObject.SetActive(false);
    }
    
}
