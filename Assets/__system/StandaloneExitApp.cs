using UnityEngine;
using System.Collections;

public class StandaloneExitApp : MonoBehaviour 
{
#if UNITY_STANDALONE
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
#else
    void Awake()
    {
        Destroy(this);
    }
#endif
}
