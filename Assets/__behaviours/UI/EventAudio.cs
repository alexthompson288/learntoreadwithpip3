using UnityEngine;
using System.Collections;

public class EventAudio : MonoBehaviour 
{
    [SerializeField]
    private string m_press;
    [SerializeField]
    private string m_unpress;

	void OnPress(bool pressed)
    {
        string audioEvent = pressed ? m_press : m_unpress;

        if (!string.IsNullOrEmpty(audioEvent))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
    }
}
