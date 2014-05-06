using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoyageGameButton : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_border;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private string m_incompleteName;
    [SerializeField]
    private string m_completeName;
    [SerializeField]
    private UISprite m_icon;
    [SerializeField]
    private UILabel m_label;

    DataRow m_section;

    public void On(DataRow section, Color completedColor)
    {
        m_section = section;

        int sectionId = System.Convert.ToInt32(m_section["id"]);

        bool hasCompleted = VoyageInfo.Instance.HasCompletedSection(sectionId);

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;
        m_background.color = hasCompleted ? completedColor : Color.white;

        m_border.SetActive(hasCompleted);

        DataRow game = DataHelpers.FindGameForSection(m_section);

        if (game != null)
        {
            string skillName = game ["skill"] != null ? game ["skill"].ToString() : "";

            m_label.text = skillName;

            string completedString = hasCompleted ? "complete" : "incomplete";

            //Debug.Log(System.String.Format("icon: {0}_{1}", skillName.ToLower(), completedString));
            m_icon.spriteName = System.String.Format("{0}_{1}", skillName.ToLower(), completedString);

            float iconAlpha = hasCompleted ? 1 : 0.8f;
            m_icon.color = new Color(1, 1, 1, iconAlpha);
        } 
        else
        {
            Debug.LogError("Could not find game for section_id: " + m_section["id"].ToString());
            gameObject.SetActive(false);
        }
    }

    void OnClick()
    {
        VoyageCoordinator.Instance.StartGame(m_section);
    }
}
