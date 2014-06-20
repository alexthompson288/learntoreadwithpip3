using UnityEngine;
using System.Collections;

public class LogPosition : MonoBehaviour 
{
    IEnumerator Start()
    {
        yield return null;

        Debug.Log(name + " global: " + transform.position);
        Debug.Log(name + " local: " + transform.localPosition);
    }
}
