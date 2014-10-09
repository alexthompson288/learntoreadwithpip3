using UnityEngine;
using System.Collections;

public class LogOnStart : MonoBehaviour 
{
    void Start()
    {
        D.Log(string.Format("LogOnStart.Start(): {0}", name));
    }
}
