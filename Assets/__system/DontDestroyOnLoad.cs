using UnityEngine;
using System.Collections;

namespace Wingrove
{

    public class DontDestroyOnLoad : MonoBehaviour
    {

        // Use this for initialization
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

    }

}