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

        Debug.Log("Game: " + sectionId + " - " + hasCompleted);

        m_background.spriteName = hasCompleted ? VoyageInfo.Instance.GetSessionBackground(System.Convert.ToInt32(m_section["number"])) : m_incompleteName;
        m_background.color = hasCompleted ? completedColor : Color.white;

        m_border.SetActive(hasCompleted);

        m_game = DataHelpers.FindGameForSection(m_section);

        if (m_game != null)
        {
            string skillName = m_game ["skill"] != null ? m_game ["skill"].ToString() : "";

            m_label.text = skillName;

            string completedString = hasCompleted ? "complete" : "incomplete";

            Debug.Log(System.String.Format("icon: {0}_{1}", skillName.ToLower(), completedString));
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

        Debug.Log("game: " + m_game);
        Debug.Log("sectionId: " + System.Convert.ToInt32(m_section ["id"]));

        if(m_game != null)
        {
            VoyageCoordinator.Instance.CreateBookmark(System.Convert.ToInt32(m_section ["id"]));

            string dbGameName = m_game["name"].ToString();
            string sceneName = GameLinker.Instance.GetSceneName(dbGameName);

            Debug.Log("dbGameName: " + dbGameName);
            Debug.Log("sceneName: " + sceneName);

            // Set scenes

            int sessionNum = System.Convert.ToInt32(m_section["number"]);
            if(VoyageInfo.Instance.NearlyCompletedSession(sessionNum) || VoyageInfo.Instance.HasCompletedSession(sessionNum))
            {
                GameManager.Instance.SetScenes(new string[] { sceneName, "NewSessionComplete" } );
            }
            else
            {
                GameManager.Instance.SetScenes(sceneName);
            }


            // Set return scene
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // TODO: Set data type

            // Set data
            GameManager.Instance.ClearData();

            int sessionId = VoyageSessionBoard.Instance.sessionId;

         
            // Phonemes
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + sessionId);
            if(dt.Rows.Count > 0)
            {
                GameManager.Instance.AddData("phonemes", dt.Rows);

                //Debug.Log("Phonemes");
                foreach(DataRow row in dt.Rows)
                {
                    //Debug.Log(row["phoneme"].ToString());
                }
            }

            // Words/Keywords
            dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE programsession_id=" + sessionId);
            if(dt.Rows.Count > 0)
            {
                List<DataRow> words = dt.Rows.FindAll(word => (word["tricky"] == null || word["tricky"].ToString() == "f") && (word["nondecodeable"] == null || word["nondecodeable"].ToString() == "f"));
                GameManager.Instance.AddData("words", words);

                Debug.Log("Words");
                foreach(DataRow row in words)
                {
                    Debug.Log(row["id"].ToString() + " - " + row["word"].ToString());
                }

                List<DataRow> keywords = dt.Rows.FindAll(word => (word["tricky"] != null && word["tricky"].ToString() == "t") || (word["nondecodeable"] != null && word["nondecodeable"].ToString() == "t"));
                GameManager.Instance.AddData("keywords", keywords);


                //Debug.Log("Keywords");
                foreach(DataRow row in keywords)
                {
                    //Debug.Log(row["word"].ToString());
                }
            }

            List<DataRow> testWords = DataHelpers.GetWords();
            Debug.Log("TestWords");
            foreach(DataRow word in testWords)
            {
                string s = word["word"] != null ? word["word"].ToString() : "";
                Debug.Log(word["id"].ToString() + " - " + s);
            }

            GameManager.Instance.StartGames();
        }

    }
}
