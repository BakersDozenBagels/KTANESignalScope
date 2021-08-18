using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RNG = UnityEngine.Random;

public class SignalScope : ModuleScript
{
    private Coroutine spinRoutine = null;
    private string outer, inner;

    private void Start()
    {
        GetChildren<AudioScript>().ForEach(a => { a.Play(a.name, true); a.Volume = 0f; a.IsMuted = true; });
        GetChild<ViewCone>().OnPlanetCollision += (s, m) => Collide(s, m, true);
        GetChild<ViewCone>().OnPlanetExit += (s, m) => Collide(s, m, false);

        string[] instruments = new[] { "Banjo", "Clarinet", "Drums", "Harmonica", "Piano", "Whistling" }.Shuffle();
        GetChildren<Planet>().Where(p => p.Instrument == "").ForEach(p => {p.Instrument = instruments[0]; instruments = instruments.Slice(1, instruments.Length - 1).ToArray(); });

        Get<KMSelectable>().Children[0].OnInteract += () => { RotateScope(true); return false; };
        Get<KMSelectable>().Children[0].OnInteractEnded += () => { StopRotation(); };
        Get<KMSelectable>().Children[2].OnInteract += () => { RotateScope(false); return false; };
        Get<KMSelectable>().Children[2].OnInteractEnded += () => { StopRotation(); };
        Get<KMSelectable>().Children[1].OnInteract += () => { if(!IsSolved) Submit(); return false; };
        GetChildren<Planet>().First(p => p.Instrument == "-").transform.parent.localEulerAngles += new Vector3(0f, RNG.Range(0f, 360f), 0f);

        Get<KMSelectable>().OnFocus += () => { GetChildren<AudioScript>().ForEach(a => a.IsMuted = false); };
        Get<KMSelectable>().OnDefocus += () => { GetChildren<AudioScript>().ForEach(a => a.IsMuted = true); };

        outer = GetChildren<Planet>().First(p => p.name == "Interloper").Instrument;
        inner = GetChildren<Planet>().First(p => p.name == "Twins").Instrument;
        Log("The outermost planet is playing {0} and the innermost planet is playing {1}.".Form(outer, inner));
    }

    private void Submit()
    {
        int target = 0;
        switch(outer)
        {
            case "Banjo":
                target = 30;
                break;
            case "Clarinet":
                target = 90;
                break;
            case "Drums":
                target = 150;
                break;
            case "Harmonica":
                target = 210;
                break;
            case "Piano":
                target = 270;
                break;
            case "Whistling":
                target = 330;
                break;
            default:
                throw new Exception("Invalid instrument on outer planet!");
        }

        float cur = GetChild<ViewCone>().transform.parent.localEulerAngles.y;
        cur = (cur % 360f + 390f) % 360f;
        if(cur > target + 30 || cur < target - 30)
        {
            Strike("Your orientation was wrong! You were at {0}, but I expected {1}.".Form((cur + 330f) % 360f, target - 30));
            return;
        }

        switch(inner)
        {
            case "Banjo":
                target = 0;
                break;
            case "Clarinet":
                target = 1;
                break;
            case "Drums":
                target = 2;
                break;
            case "Harmonica":
                target = 3;
                break;
            case "Piano":
                target = 4;
                break;
            case "Whistling":
                target = 5;
                break;
            default:
                throw new Exception("Invalid instrument on inner planet!");
        }
        int cur2 = Mathf.FloorToInt(Get<KMBombInfo>().GetTime() % 60) / 10;
        if(cur2 != target)
        {
            Strike("Your submitted time was wrong! You were at XX:{0}X, but I expected XX:{1}X.".Form(cur2, target));
            return;
        }

        Solve("Great job!");
    }

    private void Collide(string name, KMBombModule module, bool enter)
    {
        if(module.GetComponent<SignalScope>() != null && module.GetComponent<SignalScope>().Id == Id)
        {
            GetChildren<AudioScript>().Where(a => a.gameObject.name == name).ForEach(a => a.Fade(enter ? 1f : 0f, 0.5f));
        }
    }

    private void RotateScope(bool clockwise)
    {
        spinRoutine = StartCoroutine(Spin((clockwise ? 1f : -1f) * 120f));
    }

    private IEnumerator Spin(float speed)
    {
        while(true)
        {
            yield return null;
            GetChild<ViewCone>().transform.parent.localEulerAngles += new Vector3(0f, speed, 0f) * Time.deltaTime;
        }
    }

    private void StopRotation()
    {
        StopAllCoroutines();
    }


}
