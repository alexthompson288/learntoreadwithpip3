using UnityEngine;
using System.Collections;

public class PipPadPhonemeSubButton : MonoBehaviour 
{
	public delegate void ButtonClick(PipPadPhoneme pipPadPhoneme);
	public event ButtonClick OnButtonClick;

    [SerializeField]
    private PipPadPhoneme m_pipPadPhoneme;

    void OnClick()
    {
		if(OnButtonClick != null)
		{
			OnButtonClick(m_pipPadPhoneme);
		}

		if(m_pipPadPhoneme != null)
		{
        	m_pipPadPhoneme.Activate();
		}
    }
}
