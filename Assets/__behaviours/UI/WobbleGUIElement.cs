using UnityEngine;
using System.Collections;

public class WobbleGUIElement : MonoBehaviour {

    [SerializeField]
    private float m_amt;
    [SerializeField]
    private float m_speed;

    private float m_a;

    void Start()
    {
        m_a = Random.Range(0.0f, 100.0f);
    }

	// Update is called once per frame
	void Update () 
    {
        m_a += Time.deltaTime * m_speed;

        transform.localPosition = new Vector3(
            Mathf.Sin(m_a) + (Mathf.Cos(m_a * 1.42f) * 0.5f),
            Mathf.Cos(m_a) + (Mathf.Sin(m_a * 0.87f) * 0.8f), 0) * m_amt;
	}

	public void SetAmount(float amt)
	{
		m_amt = amt;
	}

	public void SetSpeed(float speed)
	{
		m_speed = speed;
	}
}
