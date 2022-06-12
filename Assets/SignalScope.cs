using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RNG = UnityEngine.Random;

public class SignalScope : MonoBehaviour
{
    private Coroutine spinRoutine = null;
    private string outer, inner;

    private bool IsSolved { get; set; }
    private int _id = ++_idc;
    private static int _idc;

    [SerializeField]
    private KMBombInfo _info;

    [SerializeField]
    private Transform _solarSystem;

    private void Start()
    {
        MuteAll(true);
        GetComponentInChildren<ViewCone>().OnPlanetCollision += (s, m) => Collide(s, m, true);
        GetComponentInChildren<ViewCone>().OnPlanetExit += (s, m) => Collide(s, m, false);

        string[] instruments = new[] { "Banjo", "Clarinet", "Drums", "Harmonica", "Piano", "Whistling" }.Shuffle();
        Planet[] planets = GetComponentsInChildren<Planet>().Where(p => p.Instrument == "").ToArray();
        for(int i = 0; i < planets.Length; i++)
            planets[i].Instrument = instruments[i];

        KMSelectable self = GetComponent<KMSelectable>();
        self.Children[0].OnInteract += () => { RotateScope(true); return false; };
        self.Children[0].OnInteractEnded += () => { StopRotation(); };
        self.Children[2].OnInteract += () => { RotateScope(false); return false; };
        self.Children[2].OnInteractEnded += () => { StopRotation(); };
        self.Children[1].OnInteract += () => { if(!IsSolved) Submit(); return false; };
        GetComponentsInChildren<Planet>().First(p => p.Instrument == "-").transform.parent.localEulerAngles += new Vector3(0f, RNG.Range(0f, 360f), 0f);

#if UNITY_EDITOR
        self.OnFocus += () => { MuteAll(false); };
        self.OnDefocus += () => { MuteAll(true); StopRotation(); };
#else
        self.OnFocus += () => { if(TwitchPlaysActive) return; MuteAll(false); };
        self.OnDefocus += () => { if(TwitchPlaysActive) return; MuteAll(true); StopRotation(); };
#endif

        outer = planets.First(p => p.name == "Interloper").Instrument;
        inner = planets.First(p => p.name == "Twins").Instrument;
        Log(string.Format("The outermost planet is playing {0} and the innermost planet is playing {1}.", outer, inner));
    }

    private void MuteAll(bool set)
    {
        foreach(CustomAudio a in GetComponentsInChildren<CustomAudio>())
            a.Mute = set;
    }

    private void Log(string message)
    {
        Debug.LogFormat("[Signal Scope #{0}] " + message, _id);
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

        float cur = GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles.y;
        cur = (cur % 360f + 390f) % 360f;
        if(cur > target + 30 || cur < target - 30)
        {
            Log(string.Format("Your orientation was wrong! You were at {0}, but I expected {1}.", (cur + 330f) % 360f, target - 30));
            GetComponent<KMBombModule>().HandleStrike();
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
        int cur2 = Mathf.FloorToInt(_info.GetTime() % 60) / 10;
        if(cur2 != target)
        {
            Log(string.Format("Your submitted time was wrong! You were at XX:{0}X, but I expected XX:{1}X.", cur2, target));
            GetComponent<KMBombModule>().HandleStrike();
            return;
        }

        Log("Great job!");
        if(TwitchPlaysActive)
            MuteAll(true);
        IsSolved = true;
        GetComponent<KMBombModule>().HandlePass();
    }

    private void Collide(string name, KMBombModule module, bool enter)
    {
        if(module.GetComponent<SignalScope>() != null && module.GetComponent<SignalScope>()._id == _id)
            foreach(CustomAudio a in GetComponentsInChildren<CustomAudio>().Where(a => a.gameObject.name == name))
                a.Volume = enter ? 1f : 0f;
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
            GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles += new Vector3(0f, speed, 0f) * Time.deltaTime;
        }
    }

    private void StopRotation()
    {
        if(spinRoutine != null)
            StopCoroutine(spinRoutine);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = @"Use ""!{0} follow <instrument> <time>"" to rotate the scope to follow that instrument for that amount of time in seconds. Use ""!{0} follow sun <time>"" to follow the sun for that amount of time. Use ""!{0} point <degrees>"" to point the scope in that direction. Use ""!{0} submit <time>"" to submit when the seconds digits of the timer match that time. Use ""!{0} mute"" to stop sound from the module until your next command.";
#pragma warning restore 414

    private bool ZenModeActive;
    bool TwitchShouldCancelCommand;
    bool TwitchPlaysActive;
    private static readonly string[] _targets = new string[] { "banjo", "clarinet", "drums", "harmonica", "piano", "whistling", "sun" };

    private const float EPSILON = 2f;

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if(IsSolved)
            yield break;

        command = command.Trim().ToLowerInvariant();
        string[] parts = command.Split(' ');

        if(parts.Length == 0)
            yield break;

        if(parts[0] == "follow")
        {
            if(parts.Length != 3)
            {
                yield return "sendtochaterror You must have a target and a time!";
                yield break;
            }

            if(!_targets.Contains(parts[1]))
            {
                yield return "sendtochaterror You must have a valid target!";
                yield break;
            }

            float t;
            if(!float.TryParse(parts[2], out t))
            {
                yield return "sendtochaterror You must submit a number as a time!";
                yield break;
            }
            if(t <= 0)
            {
                yield return "sendtochaterror You must submit a positive time!";
                yield break;
            }

            Transform planet;

            if(parts[1] == "sun")
                planet = GetComponentsInChildren<Planet>().First(p => p.Instrument == "-").transform;
            else
                planet = GetComponentsInChildren<Planet>().First(p => p.Instrument.ToLower() == parts[1]).transform.GetChild(0);

            yield return null;

            MuteAll(false);

            float end = Time.time + t;
            KMSelectable heldButton = null;

            while(Time.time < end)
            {
                yield return null;
                if(TwitchShouldCancelCommand)
                {
                    if(heldButton != null && heldButton.OnInteractEnded != null)
                        heldButton.OnInteractEnded();

                    yield return "cancelled";
                    yield break;
                }

                Vector3 loc = _solarSystem.InverseTransformPoint(planet.position);
                float theta = Mathf.Rad2Deg * Mathf.Atan2(-loc.z, loc.x);
                theta = (theta % 360f + 360f) % 360f;

                float cur = GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles.y;
                cur = (cur % 360f + 360f) % 360f;

                if(cur < theta ? theta - cur < EPSILON : cur - theta < EPSILON)
                {
                    if(heldButton != null)
                    {
                        heldButton.OnInteractEnded();
                        heldButton = null;
                        yield return new WaitForSeconds(0.3f);
                    }
                }
                else if(heldButton == null)
                {
                    if(cur < theta ? theta - cur < cur + 360f - theta : theta + 360f - cur < cur - theta)
                        heldButton = GetComponent<KMSelectable>().Children[0];
                    else
                        heldButton = GetComponent<KMSelectable>().Children[2];

                    heldButton.OnInteract();
                }
            }

            if(heldButton != null && heldButton.OnInteractEnded != null)
                heldButton.OnInteractEnded();
        }

        if(parts[0] == "point")
        {
            float theta;
            if(parts.Length != 2 || !float.TryParse(parts[1], out theta))
            {
                yield return "sendtochaterror You must submit a number as an angle!";
                yield break;
            }

            theta = (theta % 360f + 360f) % 360f;

            yield return null;

            MuteAll(false);

            float cur = GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles.y;
            cur = (cur % 360f + 360f) % 360f;

            KMSelectable button;

            if(cur < theta ? theta - cur < cur + 360f - theta : theta + 360f - cur < cur - theta)
                button = GetComponent<KMSelectable>().Children[0];
            else
                button = GetComponent<KMSelectable>().Children[2];

            button.OnInteract();

            yield return new WaitUntil(() => ((GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles.y - theta) % 360f + 360f) % 360f <= 15f || ((GetComponentInChildren<ViewCone>().transform.parent.localEulerAngles.y + theta) % 360f - 360f) % 360f >= 345f);

            button.OnInteractEnded();
        }

        if(parts[0] == "submit")
        {
            int t;
            if(parts.Length != 2 || !int.TryParse(parts[1], out t))
            {
                yield return "sendtochaterror You must submit a number as a time!";
                yield break;
            }
            if(t < 0 || t >= 60)
            {
                yield return "sendtochaterror You must submit a time that actually exists!";
                yield break;
            }

            float targetTime;
            float bombTime = _info.GetTime();
            float seconds = bombTime % 60;

            if(ZenModeActive)
            {
                if(seconds < t)
                    targetTime = bombTime - seconds + t;
                else
                    targetTime = bombTime - seconds + 60 + t;
            }
            else
            {
                if(seconds > t)
                    targetTime = bombTime - seconds + t;
                else
                    targetTime = bombTime - seconds - 60 + t;
            }

            if(targetTime < 0)
            {
                yield return "sendtochaterror The timer is too low to submit then!";
                yield break;
            }

            yield return null;

            MuteAll(false);

            if((ZenModeActive ? bombTime - targetTime : targetTime - bombTime) > 10f)
                yield return "waiting music";

            yield return "sendtochat Target submission time is " + ((int)targetTime - t) / 60 + ":" + ((int)targetTime % 60 < 10 ? "0" : "") + (int)targetTime % 60 + "!";

            while((int)_info.GetTime() % 60 != t)
            {
                yield return true;
                if(TwitchShouldCancelCommand)
                    yield return "cancelled";
            }

            GetComponent<KMSelectable>().Children[1].OnInteract();

            if(targetTime - bombTime > 10f)
                yield return "end waiting music";
        }

        if(parts.Length == 1 && parts[0] == "mute")
        {
            yield return null;
            MuteAll(true);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if(IsSolved)
            yield break;

        Log("Force solve started.");

        int target, target2;
        switch(outer)
        {
            case "Banjo":
                target = 0;
                break;
            case "Clarinet":
                target = 60;
                break;
            case "Drums":
                target = 120;
                break;
            case "Harmonica":
                target = 180;
                break;
            case "Piano":
                target = 240;
                break;
            case "Whistling":
                target = 300;
                break;
            default:
                throw new Exception("Invalid instrument on outer planet!");
        }

        switch(inner)
        {
            case "Banjo":
                target2 = 0;
                break;
            case "Clarinet":
                target2 = 1;
                break;
            case "Drums":
                target2 = 2;
                break;
            case "Harmonica":
                target2 = 3;
                break;
            case "Piano":
                target2 = 4;
                break;
            case "Whistling":
                target2 = 5;
                break;
            default:
                throw new Exception("Invalid instrument on inner planet!");
        }

        IEnumerator cmd = ProcessTwitchCommand("point " + target);

        while(cmd.MoveNext())
            yield return cmd.Current;

        cmd = ProcessTwitchCommand("submit " + target2 + ((((int)_info.GetTime() % 60) / 10) == target2 ? ((int)_info.GetTime() % 10).ToString() : "9"));

        while(cmd.MoveNext())
            yield return cmd.Current;

        MuteAll(true);

        Log("Force solve complete.");
    }
}
