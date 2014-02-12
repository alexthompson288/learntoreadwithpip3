using UnityEngine;
using System.Collections;

public class NewChallengeMenuScreenLogic : MonoBehaviour {

    [SerializeField]
    private TweenOnOffBehaviour[] m_learnPlayButtons;
    [SerializeField]
    private TweenOnOffBehaviour[] m_selectionButtons;

    public void PlayButtonClicked()
    {
        foreach (TweenOnOffBehaviour tbo in m_learnPlayButtons)
        {
            tbo.Off();
        }
        foreach (TweenOnOffBehaviour tbo in m_selectionButtons)
        {
            tbo.On();
        }
    }
}
