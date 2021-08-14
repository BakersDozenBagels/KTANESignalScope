using UnityEngine;

public class BallButtonScript : DesignerSimpletonDesign {
    [SerializeField]
    private KMSelectable _button;
    [SerializeField]
    private AudioClip _clack;

    public override void Hook(DesignerSimpletonScript module, DesignerSimpletonData data)
    {
        _button.OnInteract += () => { data.Background.GetComponentInChildren<BallSimulator>()._cone.GetComponentInChildren<Animator>().SetBool("Squash", true); return false; };
    }

    private void Start()
    {
        _button.OnInteract += () => { GetComponent<Animator>().SetBool("Squash", true); GetComponent<AudioScript>().PlayStackable(_clack); return false; };
    }
}
