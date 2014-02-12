using UnityEngine;
using System.Collections;

public class NewChallengeMenuPlayButton : MonoBehaviour {

    [SerializeField]
    private NewChallengeMenuScreenLogic m_screenLogic;

    void OnClick()
    {
        m_screenLogic.PlayButtonClicked();
    }
}
