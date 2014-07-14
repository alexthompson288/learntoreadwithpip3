using UnityEngine;
using System.Collections;
using Wingrove;

public class NavMenu : Singleton<NavMenu> 
{
    [SerializeField]
    private TweenOnOffBehaviour[] m_moveables;
    [SerializeField]
    private GameObject m_callButton;
    [SerializeField]
    private GameObject m_navMenuGoParent;
    [SerializeField]
    private Transform m_pipAnimLocation;
    [SerializeField]
    private GameObject m_pipAnimPrefab;
    [SerializeField]
    private GameObject m_parentGate;
    [SerializeField]
    private GameObject m_pipisodesButton;
    [SerializeField]
    private Transform m_gamesButton;
    [SerializeField]
    private Transform m_storiesButton;

    private Collector m_pipAnim;


    public enum MenuType
    {
        Main,
        Flashcard,
        Info,
        Buy
    }


    void Awake()
    {
        GameObject newPipAnim = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipAnimPrefab, m_pipAnimLocation);
        m_pipAnim = newPipAnim.GetComponent<PipAnim>() as Collector;
        m_pipAnim.StopAnim();

        m_navMenuGoParent.SetActive(true);

        m_parentGate.SetActive(true);

        foreach (TweenOnOffBehaviour moveable in m_moveables)
        {
            moveable.gameObject.SetActive(true);
        }
        
        #if UNITY_STANDALONE
        if(m_pipisodesButton != null)
        {
            m_pipisodesButton.SetActive(false);
            m_gamesButton.localPosition = new Vector3(-120, m_gamesButton.localPosition.y, m_gamesButton.localPosition.z);
            m_storiesButton.localPosition = new Vector3(120, m_storiesButton.localPosition.y, m_storiesButton.localPosition.z);
        }
        #endif
    }
    
    void Start()
    {
        if(SessionInformation.Instance.GetNumPlayers() == 2) 
        {
            m_callButton.SetActive(false);
        }
    }
    
    public void HideCallButton()
    {
        m_callButton.SetActive(false);
    }

    public void Call(MenuType menuType)
    {
        if (menuType == MenuType.Main)
        {
            CallMenu();
        } 
        else
        {
            int index = (int)menuType;

            if(index < m_moveables.Length)
            {
                CallMoveable(m_moveables[index]);
            }
        }
    }

    void CallMenu()
    {
        if(m_moveables[0].IsOn())
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

            foreach(TweenOnOffBehaviour moveable in m_moveables)
            {
                moveable.Off();
            }
            
            m_pipAnim.StopAnim();
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

            CallMoveable(m_moveables[0]);
            
            m_pipAnim.StartAnim();
        }
    }

    void CallMoveable(TweenOnOffBehaviour moveable)
    {
        if(moveable.IsOn())
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
            moveable.Off();
        }
        else
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
            moveable.On();
        }
    }
}