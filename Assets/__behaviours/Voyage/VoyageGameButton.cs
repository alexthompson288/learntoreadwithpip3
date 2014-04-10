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

    DataRow m_section;

    public void On(DataRow section)
    {
        m_section = section;

        int sectionId = System.Convert.ToInt32(m_section["id"]);

        bool hasCompleted = VoyageInfo.Instance.HasCompleted(sectionId);

        Debug.Log("Game: " + sectionId + " - " + hasCompleted);

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;

        m_border.SetActive(hasCompleted);

        string skillName = m_section ["sectiontype"] != null ? m_section ["gameprogresstype"].ToString() : "";

        m_label.text = skillName;

        string completedString = hasCompleted ? "complete" : "incomplete";

        m_icon.spriteName = System.String.Format("{0}_{1}", skillName, completedString);
    }

    void OnClick()
    {
        VoyageInfo.Instance.OnChooseSection(System.Convert.ToInt32(m_section["id"]));

        DataRow game = DataHelpers.FindGameForSection(m_section);

        if(game != null)
        {
            string dbGameName = game["name"].ToString();
            string sceneName = GameLinker.Instance.GetSceneName(dbGameName);

            // Set scenes
            GameManager.Instance.SetScenes(new string[] { sceneName });

            // Set return scene
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // TODO: Set data type

            // Set data
        }



    }
}
