using UnityEngine;
using System.Collections;

public class DestroyIfHandheld : MonoBehaviour 
{
# if UNITY_IPHONE || UNITY_ANDROID
    void Start()
    {
        Destroy(gameObject);
    }
#endif
}
