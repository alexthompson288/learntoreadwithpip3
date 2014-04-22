using UnityEngine;
using System.Collections;

public class BankShowCoordinator : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_correctButton;
    [SerializeField]
    private ClickEvent m_incorrectButton;
    [SerializeField]
    private ClickEvent m_skipButton;
    [SerializeField]
    private ClickEvent m_previousButton;

    bool m_showPictures;
    bool m_showSounds;

    void Start()
    {
        m_showPictures = GameManager.Instance.dataType != "alphabet";
        m_showSounds = GameManager.Instance.dataType != "Keywords";
    }

    void OnClickCorrect(ClickEvent click)
    {
        OnClickSkip(click);
    }

    void OnClickIncorrect(ClickEvent click)
    {
        OnClickSkip(click);
    }

    void OnClickSkip(ClickEvent click)
    {
    }

    void OnClickPrevious(ClickEvent click)
    {
    }
}
