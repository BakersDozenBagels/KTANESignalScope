using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CustomAudio : MonoBehaviour
{
    private AudioSource _src;

    private void Awake()
    {
        _src = GetComponent<AudioSource>();
        Mute = true;
        _src.enabled = true;
        _src.Play();
    }

    private bool _mute;
    public bool Mute
    {
        get
        {
            return _mute;
        }
        set
        {
            _mute = value;
            if(_mute)
                _src.volume = 0f;
            else
                _src.volume = Volume;
        }
    }

    private float _volume;
    public float Volume
    {
        get
        {
            return _volume;
        }
        set
        {
            _volume = value;
            if(!Mute)
                _src.volume = value;
        }
    }
}