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
    DataRow m_game;

    public void On(DataRow section, Color completedColor)
    {
        m_section = section;

        int sectionId = System.Convert.ToInt32(m_section["id"]);

        bool hasCompleted = VoyageInfo.Instance.HasCompletedSection(sectionId);

        //Debug.Log("Game: " + sectionId + " - " + hasCompleted);

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;
        m_background.color = hasCompleted ? completedColor : Color.white;

        m_border.SetActive(hasCompleted);

        m_game = DataHelpers.FindGameForSection(m_section);

        if (m_game != null)
        {
            string skillName = m_game ["skill"] != null ? m_game ["skill"].ToString() : "";

            m_label.text = skillName;

            string completedString = hasCompleted ? "complete" : "incomplete";

            //Debug.Log(System.String.Format("icon: {0}_{1}", skillName.ToLower(), completedString));
            m_icon.spriteName = System.String.Format("{0}_{1}", skillName.ToLower(), completedString);

            float iconAlpha = hasCompleted ? 1 : 0.8f;
            m_icon.color = new Color(1, 1, 1, iconAlpha);
        } 
        else
        {
            m_icon.gameObject.SetActive(false);
            m_label.gameObject.SetActive(false);
        }
    }

    void OnClick()
    {
        m_game = DataHelpers.FindGameForSection(m_section);

        //Debug.Log("game: " + m_game);
        //Debug.Log("sectionId: " + System.Convert.ToInt32(m_section ["id"]));

        if(m_game != null)
        {
            VoyageCoordinator.Instance.CreateBookmark(System.Convert.ToInt32(m_section ["id"]));

            string dbGameName = m_game["name"].ToString();
            string sceneName = GameLinker.Instance.GetSceneName(dbGameName);

            //Debug.Log("dbGameName: " + dbGameName);
            //Debug.Log("sceneName: " + sceneName);

            // Set scenes
            if(VoyageInfo.Instance.NearlyCompletedSession(VoyageSessionBoard.Instance.sessionNum) || VoyageInfo.Instance.HasCompletedSession(VoyageSessionBoard.Instance.sessionNum))
            {
                Debug.Log("Nearly Complete");
                GameManager.Instance.SetScenes(new string[] { sceneName, "NewSessionComplete" } );
            }
            else
            {
                Debug.Log("Not Complete");
                GameManager.Instance.SetScenes(sceneName);
            }


            // Set return scene
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // TODO: Set data type

            // Set data
            GameManager.Instance.ClearAllData();

            int sessionId = VoyageSessionBoard.Instance.sessionId;

         
            // Phonemes
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + sessionId);
            if(dt.Rows.Count > 0)
            {
                GameManager.Instance.AddData("phonemes", dt.Rows);
                GameManager.Instance.AddTargetData("phonemes", dt.Rows.FindAll(x => x["is_target_phoneme"] != null && x["is_target_phoneme"].ToString() == "t"));
            }

            // Words/Keywords
            dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE programsession_id=" + sessionId);
            if(dt.Rows.Count > 0)
            {
                List<DataRow> words = dt.Rows.FindAll(word => (word["tricky"] == null || word["tricky"].ToString() == "f") && (word["nondecodeable"] == null || word["nondecodeable"].ToString() == "f"));
                GameManager.Instance.AddData("words", words);
                GameManager.Instance.AddTargetData("words", words.FindAll(x => x["is_target_word"] != null && x["is_target_word"].ToString() == "t"));

                List<DataRow> keywords = dt.Rows.FindAll(word => (word["tricky"] != null && word["tricky"].ToString() == "t") || (word["nondecodeable"] != null && word["nondecodeable"].ToString() == "t"));
                GameManager.Instance.AddData("keywords", keywords);
                GameManager.Instance.AddTargetData("keywords", keywords.FindAll(x => x["is_target_word"] != null && x["is_target_word"].ToString() == "t"));
            }

            GameManager.Instance.StartGames();
        }

    }
}
