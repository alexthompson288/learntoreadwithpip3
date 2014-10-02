using UnityEngine;
using System.Collections;
using System;

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
    private Transform m_games;
    [SerializeField]
    private Transform m_stories;
    [SerializeField]
    private EventRelay[] m_returnToMainButtons;
    [SerializeField]
    private Transform m_mathsButtonParent;
    [SerializeField]
    private Transform m_readingButtonParent;
    [SerializeField]
    private Transform m_storiesButtonParent;

    float m_cameraTweenDuration = 0.25f;

    void Start()
    {
        m_games.position = m_subMenuOffLocation.position;
        m_stories.position = m_subMenuOffLocation.position;

        foreach (EventRelay relay in m_returnToMainButtons)
        {
            relay.SingleClicked += OnClickReturnToMain;
        }

        SetUpButtons(m_mathsButtonParent, OnClickMaths);
        SetUpButtons(m_readingButtonParent, OnClickReading);
        SetUpButtons(m_storiesButtonParent, OnClickStories);
    }

    void SetUpButtons(Transform parent, EventRelay.SimpleRelayEventHandler action)
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            parent.GetChild(i).GetComponent<EventRelay>().SingleClicked += action;
            parent.GetChild(i).GetComponent<PipColorWidgets>().SetPipColor((ColorInfo.PipColor)i);
        }
    }

    void OnClickReturnToMain(EventRelay relay)
    {
        iTween.MoveTo(m_camera, m_main.position, m_cameraTweenDuration);
    }

    void OnClickMaths(EventRelay relay)
    {
        BasicGameMenuCoordinator.Instance.On(relay.GetComponent<PipColorWidgets>().color, true);
        MoveToSubMenu(m_games);
    }

    void OnClickReading(EventRelay relay)
    {
        BasicGameMenuCoordinator.Instance.On(relay.GetComponent<PipColorWidgets>().color, false);
        MoveToSubMenu(m_games);
    }

    void OnClickStories(EventRelay relay)
    {
        BasicStoriesMenuCoordinator.Instance.On(relay.GetComponent<PipColorWidgets>().color);
        MoveToSubMenu(m_stories);
    }

    void MoveToSubMenu(Transform target)
    {
        m_games.position = target == m_games ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;
        m_stories.position = target == m_stories ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;

        iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
    }
}
