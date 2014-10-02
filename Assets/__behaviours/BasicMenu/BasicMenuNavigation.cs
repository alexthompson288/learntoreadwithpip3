using UnityEngine;
using System.Collections;

public class BasicMenuNavigation : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_camera;
    [SerializeField]
    private Transform m_main;
    [SerializeField]
    private Transform m_subMenuOnLocation;
    [SerializeField]
    private Transform m_subMenuOffLocation;
    [SerializeField]
    private Transform m_readingGames;
    [SerializeField]
    private Transform m_mathsGames;
    [SerializeField]
    private Transform m_stories;
    [SerializeField]
    private EventRelay[] m_returnToMainButtons;
    [SerializeField]
    private EventRelay[] m_readingGameButtons;
    [SerializeField]
    private EventRelay[] m_mathsGamesButtons;
    [SerializeField]
    private EventRelay[] m_storiesButtons;

    float m_cameraTweenDuration = 0.25f;

    void Awake()
    {
        foreach (EventRelay relay in m_returnToMainButtons)
        {
            relay.SingleClicked += OnClickReturnToMain;
        }
    }

    void OnClickReturnToMain(EventRelay relay)
    {
        iTween.MoveTo(m_camera, m_main.position, m_cameraTweenDuration);
    }

    void OnClickReading(EventRelay relay)
    {
        MoveToSubMenu(m_readingGames);
    }

    void OnClickMaths(EventRelay relay)
    {
        MoveToSubMenu(m_mathsGames);
    }

    void OnClickStories(EventRelay relay)
    {
        MoveToSubMenu(m_stories);
    }

    void MoveToSubMenu(Transform target)
    {
        m_readingGames.position = m_subMenuOffLocation.position;
        m_mathsGames.position = m_subMenuOffLocation.position;
        m_stories.position = m_subMenuOffLocation.position;

        target.position = m_subMenuOnLocation.position;

        iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
    }
}
