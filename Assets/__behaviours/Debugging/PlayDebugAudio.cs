using UnityEngine;
using System.Collections;

public class PlayDebugAudio : MonoBehaviour 
{
    void Start()
    {
        StartCoroutine(PlayAudio());
    }

    IEnumerator PlayAudio()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");

        yield return new WaitForSeconds(0.5f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(PlayAudio());

    }
}
