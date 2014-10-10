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
    [SerializeField]
    private RotateConstantly m_rotateBehaviour;

    DataRow m_session;


    public void SetUp(DataRow session, Color col)
    {
        m_session = session;

        gameObject.SetActive(m_session != null);

        if (m_session != null)
        {
            m_label.text = m_session ["labeltext"] != null && !string.IsNullOrEmpty(m_session["labeltext"].ToString()) ? 
                m_session ["labeltext"].ToString() 
                    : "?";

            string unlockedSpriteName = VoyageInfo.Instance.GetSessionBackground(m_session.GetId());
            bool hasUnlocked = !string.IsNullOrEmpty(unlockedSpriteName);

            m_shadow.SetActive(!hasUnlocked);

            m_button.color = hasUnlocked ? Color.white : col;
            m_button.atlas = hasUnlocked ? BasicGameMenuCoordinator.Instance.sessionUnlockedAtlas : BasicGameMenuCoordinator.Instance.sessionLockedAtlas;
            m_button.spriteName = hasUnlocked ? unlockedSpriteName : "button_square";
            m_button.transform.localScale = hasUnlocked ? Vector3.one * 1.3f : Vector3.one;
            m_rotateBehaviour.enabled = hasUnlocked;

            if (!hasUnlocked)
            {
                m_button.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void OnClick()
    {
        BasicGameMenuCoordinator.Instance.OnClickVoyageButton(m_session);
    }
}
