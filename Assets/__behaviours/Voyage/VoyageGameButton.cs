using UnityEngine;
using System.Collections;

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

    int m_sectionId;

    public void On(DataRow section)
    {
        m_sectionId = System.Convert.ToInt32(section["id"]);

  
        bool hasCompleted = VoyageInfo.Instance.HasCompleted(m_sectionId);

        Debug.Log("Game: " + m_sectionId + " - " + hasCompleted);

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;

        m_border.SetActive(hasCompleted);

        string skillName = section ["sectiontype"] != null ? section ["sectiontype"].ToString() : "";

        m_label.text = skillName;

        string completedString = hasCompleted ? "complete" : "incomplete";

        m_icon.spriteName = System.String.Format("{0}_{1}", skillName, completedString);
    }

    void OnClick()
    {

    }
}
