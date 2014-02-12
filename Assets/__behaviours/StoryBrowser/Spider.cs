using UnityEngine;
using System.Collections;

public class Spider : MonoBehaviour {

    [SerializeField]
    private Transform[] m_transforms;
    [SerializeField]
    private float m_speed;
    [SerializeField]
    private float m_delayMax;
    [SerializeField]
    private float m_delayMin;

    private int m_target = 0;

	// Use this for initializationss
	IEnumerator Start () 
    {

        while (true)
        {
            m_target++;
            if (m_target >= m_transforms.Length)
            {
                m_target = 0;
            }

            while (true)
            {
                Vector3 delta = m_transforms[m_target].position - transform.position;
                float moveAmt = m_speed * Time.deltaTime;
                if (delta.magnitude > moveAmt)
                {
                    transform.position = transform.position + delta.normalized * moveAmt;
                }
                else
                {
                    transform.position = m_transforms[m_target].position;
                    break;
                }

                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(m_delayMin, m_delayMax));
        }
	}
}
