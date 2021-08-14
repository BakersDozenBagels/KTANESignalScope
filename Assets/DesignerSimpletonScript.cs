using KeepCoding;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DesignerSimpletonScript : ModuleScript {
    [SerializeField]
    private GameObject[] _buttons;
    [SerializeField]
    private GameObject[] _backgrounds;
    [SerializeField]
    private Transform[] _SLPoses;
    [SerializeField]
    private string[] _names;
    [SerializeField]
    private Transform _sLParent;

#if UNITY_EDITOR
    private static int _counter = 0;
#endif

    // Use this for initialization
    new void Awake () {
        base.Awake();
#if UNITY_EDITOR
        int data = Enumerable.Range(0, _names.Length).ToArray()[_counter++ % _names.Length];
#else
        int data = Enumerable.Range(0, _names.Length).PickRandom();
#endif
        Log("Using design: " + _names[data]);
        DesignerSimpletonData nd = new DesignerSimpletonData
        {
            Background = Instantiate(_backgrounds[data], transform)
        };
        GameObject button = Instantiate(_buttons[data], transform);
        nd.Button = button;
        _sLParent.localPosition = _SLPoses[data].localPosition;
        nd.SLPos = _sLParent;
        nd.DesignName = _names[data];

        KMSelectable sel = button.GetComponentInChildren<KMSelectable>();
        Get<KMSelectable>().Children = new KMSelectable[] { sel };
        sel.Parent = Get<KMSelectable>();
        sel.OnInteract += () => { Solve("You pushed the *fancy* button!"); return false; };

        StartCoroutine(WaitShortly(() => { button.GetComponent<DesignerSimpletonDesign>().Hook(this, nd); }));
	}

    private void Start()
    {
        Get<KMSelectable>().UpdateChildren();
    }

    private IEnumerator WaitShortly(Action a)
    {
        yield return null;
        a();
    }
}
