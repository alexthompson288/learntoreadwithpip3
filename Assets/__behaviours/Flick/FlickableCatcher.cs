using UnityEngine;
using System.Collections;

public class FlickableCatcher : MonoBehaviour
{
    public delegate void FlickableEnter(Collider flickable);
    public event FlickableEnter OnFlickableEnter;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Flickable" && OnFlickableEnter != null)
        {
            OnFlickableEnter(other);
        }
    }
}
