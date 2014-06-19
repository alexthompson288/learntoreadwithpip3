using UnityEngine;
using System.Collections;

public class PipPadPhonemeSubButton : MonoBehaviour 
{
	public delegate void EventHandler(PipPadPhoneme pipPadPhoneme);
    public event EventHandler Clicked;

    [SerializeField]
    private PipPadPhoneme m_pipPadPhoneme;

    void OnClick()
    {
        if(Clicked != null)
		{
            Clicked(m_pipPadPhoneme);
		}

		if(m_pipPadPhoneme != null)
		{
        	m_pipPadPhoneme.Activate();
		}
    }
}
