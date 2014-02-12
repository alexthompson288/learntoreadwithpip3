using UnityEngine;
using System.Collections;

public class DragRecenter : MonoBehaviour
{

    Vector3 m_initialPos;

    // Use this for initialization
    void Start()
    {

        m_initialPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = m_initialPos;
    }
}
