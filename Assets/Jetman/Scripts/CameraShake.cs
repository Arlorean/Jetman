using UnityEngine;
using System.Collections;

/// <summary>
/// Modified version of this GitHub gist:
/// https://gist.github.com/ftvs/5822103#file-camerashake-cs
/// </summary>
public class CameraShake : MonoBehaviour
{	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.2f;
	
	Vector3 originalPos;
	
	void OnEnable() {
		originalPos = transform.localPosition;
	}

    void OnDisable() {
		transform.localPosition = originalPos; 
    }

    void Update() {
        transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
	}
}
