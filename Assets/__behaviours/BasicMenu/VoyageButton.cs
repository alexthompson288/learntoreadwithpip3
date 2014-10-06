using UnityEngine;
using System.Collections;

public class VoyageButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_button;
    [SerializeField]
    private GameObject m_shadow;
    [SerializeField]
    private UILabel m_label;

    DataRow m_session;


    public void SetUp(DataRow session, Color col)
    {
        m_session = session;
        gameObject.SetActive(m_session != null);

        m_label.text = m_session ["labeltext"] != null ? m_session ["labeltext"].ToString() : "?";

        string unlockedSpriteName = VoyageInfo.Instance.GetSessionBackground(m_session.GetId());
        bool hasUnlocked = !string.IsNullOrEmpty(unlockedSpriteName);

        m_shadow.SetActive(!hasUnlocked);

        m_button.color = hasUnlocked ? Color.white : col;
        m_button.atlas = hasUnlocked ? BasicGameMenuCoordinator.Instance.sessionUnlockedAtlas : BasicGameMenuCoordinator.Instance.sessionLockedAtlas;
        m_button.spriteName = hasUnlocked ? unlockedSpriteName : "button_square";

        if (hasUnlocked)
        {
            m_button.transform.localScale *= 1.3f;
            RotateConstantly rotateBehaviour = m_button.gameObject.AddComponent<RotateConstantly>();
        }
    }

    void OnClick()
    {
        BasicGameMenuCoordinator.Instance.OnClickVoyageButton(m_session);
    }
}
