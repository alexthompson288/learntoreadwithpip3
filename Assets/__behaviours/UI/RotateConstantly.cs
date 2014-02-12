using UnityEngine;
using System.Collections;

public class RotateConstantly : MonoBehaviour {

    [SerializeField]
    private float m_rotateSpeed;
    [SerializeField]
    private float m_randomRange = 20;

    private float m_rotateSpeedInternal;

    void Start()
    {
        transform.Rotate(Vector3.forward, Random.Range(0.0f,360.0f));
    }

    void OnEnable()
    {
        m_rotateSpeedInternal = m_rotateSpeed + Random.Range(-m_randomRange, m_randomRange);
    }

	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(Vector3.forward, m_rotateSpeedInternal * Time.deltaTime);
	}
}
