using UnityEngine;
using System.Collections;

public class EventAudio : MonoBehaviour 
{
    [SerializeField]
    private string m_press;
    [SerializeField]
    private string m_unpress;
    [SerializeField]
    private string m_click;

	void OnPress(bool pressed)
    {
        string audioEvent = pressed ? m_press : m_unpress;

        if (!string.IsNullOrEmpty(audioEvent))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
    }

    void OnClick()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent(m_click);
    }
}
