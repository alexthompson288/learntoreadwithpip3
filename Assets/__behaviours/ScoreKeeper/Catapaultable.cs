using UnityEngine;
using System.Collections;

public class Catapaultable : MonoBehaviour 
{
    [SerializeField]
    private ScoreCatapault m_catapault;

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "ScoreCatapultOrigin")
        {
            m_catapault.ReleaseLineEnd();
        }
    }
}
