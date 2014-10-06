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
    [SerializeField]
    private UITexture m_temporaryIconTexture;

    DataRow m_section;


    public void On(DataRow section, Color completedColor)
    {
        m_section = section;

        int sectionId = System.Convert.ToInt32(m_section["id"]);

        //bool hasCompleted = OldVoyageInfo.Instance.HasCompletedSection(sectionId);
        bool hasCompleted = false;

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;
        m_background.color = hasCompleted ? completedColor : Color.white;

        m_border.SetActive(hasCompleted);


        DataRow game = DataHelpers.GetGameForSection(m_section);

        if (game != null)
        {
            //string skillName = game ["skill"] != null ? game ["skill"].ToString() : game["label_text"].ToString();
            //m_label.text = skillName;

            string labelText = game["labeltext"] != null ? game["labeltext"].ToString() : game["name"].ToString();
            m_label.text = labelText;

            string completedString = hasCompleted ? "complete" : "incomplete";

            //////////D.Log(System.String.Format("icon: {0}_{1}", skillName.ToLower(), completedString));
            //m_icon.spriteName = System.String.Format("{0}_{1}", skillName.ToLower(), completedString);
            m_icon.spriteName = System.String.Format("{0}_{1}", game["name"], completedString);

            //////////D.Log("texName: " + m_icon.spriteName);

            m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(System.String.Format("{0}_{1}", game["name"], completedString));
            if (m_temporaryIconTexture.mainTexture == null)
            {
                m_temporaryIconTexture.mainTexture = Resources.Load<Texture2D>(game ["name"].ToString());
            }
            
            m_temporaryIconTexture.gameObject.SetActive(m_temporaryIconTexture.mainTexture != null);

            float iconAlpha = hasCompleted ? 1 : 0.8f;
            m_icon.color = new Color(1, 1, 1, iconAlpha);
        } 
        else
        {
            gameObject.SetActive(false);
        }
    }
}
