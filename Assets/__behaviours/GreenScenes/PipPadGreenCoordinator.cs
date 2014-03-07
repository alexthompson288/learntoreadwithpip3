using UnityEngine;
using System.Collections;

public class PipPadGreenCoordinator : MonoBehaviour 
{
	[SerializeField]
	private string[] m_words;

	int m_index = 0;

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			--m_index;
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			++m_index;
		}

		if(m_index >= m_words.Length)
		{
			m_index = 0;
		}
		else if(m_index < 0)
		{
			m_index = m_words.Length - 1;
		}

		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			if(PipPadBehaviour.Instance.IsShowing())
			{
				PipPadBehaviour.Instance.Hide();
				StartCoroutine(ShowWord(0.5f));
			}
			else
			{
				StartCoroutine(ShowWord(0));
			}

		}
		else if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			PipPadBehaviour.Instance.Hide();
		}

#if UNITY_EDITOR
		//Debug.Log(m_words[m_index]);
#endif
	}

	IEnumerator ShowWord(float delay)
	{
		yield return new WaitForSeconds(delay);

		PipPadBehaviour.Instance.Show(m_words[m_index]);
	}
}
