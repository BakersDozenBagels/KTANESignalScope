using UnityEngine;

public class Planet : MonoBehaviour {
    public Vector3 Rotation;
    public string Instrument;

    private void FixedUpdate () {
        transform.localEulerAngles += Rotation * Time.fixedDeltaTime;
	}
}
