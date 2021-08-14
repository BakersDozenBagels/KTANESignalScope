using UnityEngine;

public class Rainbowifier : MonoBehaviour {
    private Material _material;
    [SerializeField]
    private AnimationCurve _ease;
    [SerializeField]
    [Range(0, 10)]
    private float _timeScale;
    [SerializeField]
    private Color[] _colors;
    [SerializeField]
    [Range(0, 10)]
    private int _offset;

    private float _time = 0f;
    
	// Use this for initialization
	private void Start () {
        _material = GetComponent<MeshRenderer>().material;
	}
	
	// Update is called once per frame
	private void Update () {
        _time += Time.deltaTime;
        _time %= _timeScale * _colors.Length;
        int colorToUse = Mathf.FloorToInt(_time / _timeScale);
        float easePercent = (_time / _timeScale) % 1f;
        _material.color = Color.Lerp(_colors[(colorToUse + _offset) % _colors.Length], _colors[(colorToUse + 1 + _offset) % _colors.Length], _ease.Evaluate(easePercent));
	}
}
