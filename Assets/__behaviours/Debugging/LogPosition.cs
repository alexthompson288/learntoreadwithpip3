using UnityEngine;
using System.Collections;

public class LogPosition : MonoBehaviour 
{
    IEnumerator Start()
    {
        yield return null;

        ////D.Log(name + " global: " + transform.position);
        ////D.Log(name + " local: " + transform.localPosition);
    }
}
