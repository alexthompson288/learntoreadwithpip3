using UnityEngine;
using System.Collections;

public class Catapultable : MonoBehaviour 
{
    [SerializeField]
    private ScoreCatapault m_catapault;

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "ScoreCatapaultOrigin")
        {
            m_catapault.ReleaseLineEnd();
        }
    }
}
