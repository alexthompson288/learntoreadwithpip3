using UnityEngine;
using System.Collections;

public class VoyageButton : MonoBehaviour 
{
    [SerializeField]
    private EventRelay m_buttonRelay;
    [SerializeField]
    private UISprite m_buttonSprite;
    [SerializeField]
    private GameObject m_shadowSprite;

    DataRow m_session;

    void Awake()
    {
        m_buttonRelay.SingleClicked += OnClickRelay;
    }

    public void SetUp(DataRow session, Color col)
    {
        m_session = session;
        gameObject.SetActive(m_session != null);

        string unlockedSpriteName = VoyageInfo.Instance.GetSessionBackground(session.GetId());
        bool hasUnlocked = !string.IsNullOrEmpty(unlockedSpriteName);

        m_shadowSprite.SetActive(!hasUnlocked);

        m_buttonSprite.color = hasUnlocked ? Color.white : col;
        m_buttonSprite.atlas = hasUnlocked ? BasicGameMenuCoordinator.Instance.sessionUnlockedAtlas : BasicGameMenuCoordinator.Instance.sessionLockedAtlas;
        m_buttonSprite.spriteName = hasUnlocked ? unlockedSpriteName : "button_square";
        m_buttonSprite.MakePixelPerfect();
    }

    void OnClickRelay(EventRelay relay)
    {
        BasicGameMenuCoordinator.Instance.OnClickVoyageButton(m_session);
    }
}
