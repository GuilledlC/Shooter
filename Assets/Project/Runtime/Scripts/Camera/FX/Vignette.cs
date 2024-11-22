using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Vignette : MonoBehaviour {

	private VolumeProfile _profile;
	
	public void Initialize(VolumeProfile profile) {
		_profile = profile;
	}

	public void UpdateVignette(float deltaTime, Stance stance) { }
}
