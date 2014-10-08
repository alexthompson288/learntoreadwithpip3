using UnityEngine;
using System.Collections;
using System;

public class BasicMenuNavigation : Menu 
{
    [SerializeField]
    private UIPanel m_panel;
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
    private Transform m_flashcards;
    [SerializeField]
    private Transform m_colourInfo;
    [SerializeField]
    private EventRelay[] m_returnToMainButtons;
    [SerializeField]
    private Transform m_mathsButtonParent;
    [SerializeField]
    private Transform m_readingButtonParent;
    [SerializeField]
    private Transform m_storiesButtonParent;
    [SerializeField]
    private EventRelay m_flashcardsButton;
    [SerializeField]
    private EventRelay m_colourInfoButton;
    [SerializeField]
    private EventRelay m_dismissBackground;
    [SerializeField]
    private UISprite[] m_lockSprites;

    float m_cameraTweenDuration = 0.25f;

    static Bookmark m_bookmark;
    
    class Bookmark
    {
        public string m_subMenu;
        public ColorInfo.PipColor m_pipColor;
        public int m_storyId = 0;

        public Bookmark(string mySubMenu, ColorInfo.PipColor myPipColor)
        {
            m_subMenu = mySubMenu;
            m_pipColor = myPipColor;
        }

        public void SetStoryId(int myStoryId)
        {
            m_storyId = myStoryId;
        }
    }

    public override void On()
    {
        m_camera.transform.position = m_main.position;
        TweenAlpha.Begin(m_panel.gameObject, 0.25f, 1);
    }

    public void OnClickDismiss(EventRelay relay)
    {
        TweenAlpha.Begin(m_panel.gameObject, 0.25f, 0);
    }

    void Start()
    {
        m_games.position = m_subMenuOffLocation.position;
        m_stories.position = m_subMenuOffLocation.position;

        m_flashcardsButton.SingleClicked += OnClickFlashcards;
        m_colourInfoButton.SingleClicked += OnClickColourInfo;

        foreach (EventRelay relay in m_returnToMainButtons)
        {
            relay.SingleClicked += OnClickReturnToMain;
        }

        System.Array.Sort(m_lockSprites, CollectionHelpers.LocalLeftToRight);

        for(int i = 0; i < m_lockSprites.Length; ++i)
        {
            m_lockSprites[i].enabled = !BuyInfo.Instance.IsColorPurchased((ColorInfo.PipColor)i + 1);
        }

        SetUpButtons(m_mathsButtonParent, OnClickMaths);
        SetUpButtons(m_readingButtonParent, OnClickReading);
        SetUpButtons(m_storiesButtonParent, OnClickStories);

        m_callButton.SetActive(SessionInformation.Instance.GetNumPlayers() == 1);
        
        bool isStartLevel = Application.loadedLevel == 0;
        
        m_panel.alpha = isStartLevel ? 1 : 0;
        if (isStartLevel)
        {
            if (m_bookmark != null)
            {
                if (m_bookmark.m_subMenu == "Stories")
                {
                    BasicStoriesMenuCoordinator.Instance.On(m_bookmark.m_pipColor, m_bookmark.m_storyId);
                    m_stories.position = m_subMenuOnLocation.position;
                    m_camera.transform.position = m_stories.position;
                } 
                else
                {
                    bool isMaths = m_bookmark.m_subMenu == "Maths";
                    BasicGameMenuCoordinator.Instance.On(m_bookmark.m_pipColor, isMaths);
                    m_games.position = m_subMenuOnLocation.position;
                    m_camera.transform.position = m_games.position;
                }
            }
        }
        else
        {
            m_dismissBackground.SingleClicked += OnClickDismiss;
        }
    }

//    void OnLevelWasLoaded(int level)
//    {
//        UICamera uiCam = GetComponentInChildren<UICamera>() as UICamera;
//        if (uiCam != null)
//        {
//            uiCam.enabled = true;
//        }
//
//        m_callButton.SetActive(SessionInformation.Instance.GetNumPlayers() == 1);
//        
//        bool isStartLevel = level == 0;
//        
//        m_panel.alpha = isStartLevel ? 1 : 0;
//        if (isStartLevel)
//        {
//            m_dismissBackground.SingleClicked -= OnClickDismiss;
//
//            if (m_bookmark != null)
//            {
//                if (m_bookmark.m_subMenu == "Stories")
//                {
//                    BasicStoriesMenuCoordinator.Instance.On(m_bookmark.m_pipColor, m_bookmark.m_storyId);
//                    m_stories.position = m_subMenuOnLocation.position;
//                    m_camera.transform.position = m_stories.position;
//                } 
//                else
//                {
//                    bool isMaths = m_bookmark.m_subMenu == "Maths";
//                    BasicGameMenuCoordinator.Instance.On(m_bookmark.m_pipColor, isMaths);
//                    m_games.position = m_subMenuOnLocation.position;
//                    m_camera.transform.position = m_games.position;
//                }
//            }
//        }
//        else
//        {
//            m_dismissBackground.SingleClicked += OnClickDismiss;
//        }
//    }

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
        m_bookmark = null;
        iTween.MoveTo(m_camera, m_main.position, m_cameraTweenDuration);
    }

    void OnClickMaths(EventRelay relay)
    {
        ColorInfo.PipColor pipColor = relay.GetComponent<PipColorWidgets>().color;

#if UNITY_EDITOR || UNITY_STANDALONE
        bool canAccess = true;
#else
        bool canAccess = BuyInfo.Instance.IsColorPurchased(pipColor);
#endif

        if (canAccess)
        {
            m_bookmark = new Bookmark("Maths", pipColor);

            BasicGameMenuCoordinator.Instance.On(pipColor, true);
            MoveToSubMenu(m_games);
        }
        else
        {
            BuyBasicColors.Instance.On(pipColor);
        }
    }

    void OnClickReading(EventRelay relay)
    {
        ColorInfo.PipColor pipColor = relay.GetComponent<PipColorWidgets>().color;

#if UNITY_EDITOR || UNITY_STANDALONE
        bool canAccess = true;
#else
        bool canAccess = BuyInfo.Instance.IsColorPurchased(pipColor);
#endif
        
        if (canAccess)
        {
            m_bookmark = new Bookmark("Reading", pipColor);

            BasicGameMenuCoordinator.Instance.On(pipColor, false);
            MoveToSubMenu(m_games);
        }
        else
        {
            BuyBasicColors.Instance.On(pipColor);
        }
    }

    void OnClickStories(EventRelay relay)
    {
        ColorInfo.PipColor pipColor = relay.GetComponent<PipColorWidgets>().color;

#if UNITY_EDITOR || UNITY_STANDALONE
        bool canAccess = true;
#else
        bool canAccess = BuyInfo.Instance.IsColorPurchased(pipColor);
#endif
        
        if (canAccess)
        {
            m_bookmark = new Bookmark("Stories", pipColor);

            BasicStoriesMenuCoordinator.Instance.On(pipColor);
            MoveToSubMenu(m_stories);
        }
        else
        {
            BuyBasicColors.Instance.On(pipColor);
        }
    }

    void OnClickFlashcards(EventRelay relay)
    {
        m_bookmark = new Bookmark("Flashcards", ColorInfo.PipColor.Pink);
        MoveToSubMenu(m_flashcards);
    }

    void OnClickColourInfo(EventRelay relay)
    {
        MoveToSubMenu(m_colourInfo);
    }

    void MoveToSubMenu(Transform target)
    {
        m_games.position = target == m_games ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;
        m_stories.position = target == m_stories ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;
        m_flashcards.position = target == m_flashcards ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;
        m_colourInfo.position = target == m_colourInfo ? m_subMenuOnLocation.position : m_subMenuOffLocation.position;

        iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
    }

    public void SetBookmarkStoryId(int storyId)
    {
        if (m_bookmark != null)
        {
            m_bookmark.SetStoryId(storyId);
        }
    }
}
