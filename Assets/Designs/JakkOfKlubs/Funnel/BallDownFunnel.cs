using System.Collections;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class BallDownFunnel : DesignerSimpletonDesign
{
    private float rand;

    private Coroutine moveBall;
    private bool ballMoving = false;

    public KMSelectable funnel;
    public GameObject BallCenter;
    public GameObject BallObject;
    
    void Start()
    {
        rand = Rnd.Range(0f, 360f);
        BallCenter.transform.localEulerAngles = new Vector3(0f, rand, 0f);
        funnel.OnInteract += delegate ()
        {
            if (!ballMoving)
            {
                moveBall = StartCoroutine(MoveBall());
                ballMoving = true;
                GetComponent<AudioScript>().PlayStackable("MarbleSound");
            }
            return false;
        };
    }

    IEnumerator MoveBall()
    {
        float duration = 12.0f;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            BallCenter.transform.localEulerAngles = new Vector3(0f, Easing.InQuad(elapsed, rand + 0f, rand + 2880f, duration), 0f);
            BallObject.transform.localEulerAngles = new Vector3(Easing.InQuad(elapsed, 0f, -2880f, duration), 0f, 0f);
            if (elapsed < 10.5f)
            {
                BallObject.transform.localPosition = new Vector3(Easing.InQuad(elapsed, 0.06f, 0f, 10.5f), Easing.InQuad(elapsed, 0f, -0.015f, 10.5f), 0f);
                yield return null;
            }
            else
            {
                BallObject.transform.localPosition = new Vector3(Easing.InQuad(elapsed - 10.5f, 0f, 0f, 1.5f), Easing.OutQuad(elapsed - 10.5f, -0.015f, -0.07f, 1.5f), 0f);
                yield return null;
            }
            elapsed += Time.deltaTime;
        }
        BallObject.transform.localPosition = new Vector3(0f, -0.04f, 0f);
    }

    public override void Hook(DesignerSimpletonScript module, DesignerSimpletonData data)
    {

    }
}