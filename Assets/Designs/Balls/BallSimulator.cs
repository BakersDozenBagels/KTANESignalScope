using System.Collections.Generic;
using UnityEngine;

public class BallSimulator : MonoBehaviour {
    [SerializeField]
    private GameObject _cage, _pball, _vball;
    [SerializeField]
    [Range(1f, 10f)]
    private float scaler;

    internal GameObject _cone;

    private List<GameObject> _pballs = new List<GameObject>(), _vballs = new List<GameObject>();

    // Use this for initialization
    private void Awake () {
        _cage = Instantiate(_cage, transform.position + new Vector3(Random.Range(1000f, 10000f), Random.Range(1000f, 10000f), Random.Range(1000f, 10000f)), transform.rotation);
        _cage.transform.localScale *= 10f * scaler;
        for (int i = 0; i < 100; i++)
        {
            _pballs.Add(Instantiate(_pball, _cage.transform.position, Quaternion.identity, _cage.transform.GetChild(0)));
            _vballs.Add(Instantiate(_vball, transform.position, Quaternion.identity, transform));
            _pballs[i].transform.localPosition += new Vector3(0.05f * (i % 10) * scaler - 0.5f * scaler, 0.1f * scaler, 0.1f * (i / 10) * scaler - 0.5f * scaler);
            _pballs[i].transform.localScale = new Vector3(10f * scaler, 10f * scaler, 10f * scaler);
            _vballs[i].transform.localPosition += new Vector3(0f, 0.01f);
        }
        _cone = _cage.transform.GetChild(0).gameObject;
	}

    // Update is called once per frame
    private void FixedUpdate () {
        _cage.transform.rotation = transform.rotation;
        for (int i = 0; i < _pballs.Count; i++)
        {
            _vballs[i].transform.localPosition = _pballs[i].transform.localPosition / 10 * scaler;
        }
    }
}
