using UnityEngine;

public class PixelButtonScript : DesignerSimpletonDesign {
    [SerializeField]
    private AudioScript _audio, _audio2;
    [SerializeField]
    private AudioClip _beep, _gong;

    public override void Hook(DesignerSimpletonScript module, DesignerSimpletonData data)
    {
        module.Get<KMSelectable>().OnFocus += () => StartSounds();
        module.Get<KMSelectable>().OnDefocus += () => EndSounds();
    }

    private void EndSounds()
    {
        _audio.Fade(0f, 0.5f);
    }

    private bool StartSounds()
    {
        _audio.Fade(1f, 0.5f);
        return false;
    }

    void Start () {
        GetComponent<KMSelectable>().OnInteract += () => { _audio2.Play(_gong); GetComponent<Animator>().SetBool("Disappearing", true); return false; };
        _audio.Play(_beep, true);
        _audio.Volume = 0f;
    }
}
