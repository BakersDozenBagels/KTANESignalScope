using System;
using UnityEngine;

public class ViewCone : MonoBehaviour
{
    public event Action<string, KMBombModule> OnPlanetCollision = (s, m) => { };
    public event Action<string, KMBombModule> OnPlanetExit = (s, m) => { };

#if UNITY_EDITOR
    const string Tag = "SignalScopePlanet";
#else
    const string Tag = "Pickup";
#endif

    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.tag == Tag && other.GetComponent<Planet>() != null)
            OnPlanetCollision(other.GetComponent<Planet>().Instrument, other.GetComponentInParent<KMBombModule>());
    }

    private void OnTriggerExit(Collider other)
    {
        if(other != null && other.tag == Tag && other.GetComponent<Planet>() != null)
            OnPlanetExit(other.GetComponent<Planet>().Instrument, other.GetComponentInParent<KMBombModule>());
    }
}
