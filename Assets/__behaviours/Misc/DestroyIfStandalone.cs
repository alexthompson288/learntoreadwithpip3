using UnityEngine;
using System.Collections;

public class DestroyIfStandalone : MonoBehaviour 
{
#if UNITY_STANDALONE
    void Start()
    {
        Destroy(gameObject);
    }
#endif
}
