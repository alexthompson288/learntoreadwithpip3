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

    public void On(DataRow section)
    {
        m_section = section;

        int sectionId = System.Convert.ToInt32(m_section["id"]);

        bool hasCompleted = VoyageInfo.Instance.HasCompleted(sectionId);

        Debug.Log("Game: " + sectionId + " - " + hasCompleted);

        m_background.spriteName = hasCompleted ? m_completeName : m_incompleteName;

        m_border.SetActive(hasCompleted);

        string skillName = m_section ["gameprogresstype"] != null ? m_section ["gameprogresstype"].ToString() : "";

        m_label.text = skillName;

        string completedString = hasCompleted ? "complete" : "incomplete";

        m_icon.spriteName = System.String.Format("{0}_{1}", skillName, completedString);
    }

    void OnClick()
    {
        VoyageInfo.Instance.OnChooseSection(System.Convert.ToInt32(m_section["id"]));

        DataRow game = DataHelpers.FindGameForSection(m_section);

        Debug.Log("game: " + game);

        if(game != null)
        {
            int module = VoyageCoordinator.Instance.currentModuleMap.mapIndex;
            int sessionNum = VoyageCoordinator.Instance.currentModuleMap.sessionNum;
            ColorInfo.PipColor color = VoyageCoordinator.Instance.currentModuleMap.moduleColor;

            VoyageInfo.Instance.SetCurrentLocation(module, sessionNum, color);

            string dbGameName = game["name"].ToString();
            string sceneName = GameLinker.Instance.GetSceneName(dbGameName);

            Debug.Log("dbGameName: " + dbGameName);
            Debug.Log("sceneName: " + sceneName);

            // Set scenes
            GameManager.Instance.SetScenes(sceneName);

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
            }

            // Words/Keywords
            dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE programsession_id=" + sessionId);
            if(dt.Rows.Count > 0)
            {
                List<DataRow> words = dt.Rows.FindAll(word => (word["tricky"] == null || word["tricky"].ToString() == "f") && (word["nondecodeable"] == null || word["nondecodeable"].ToString() == "f"));
                GameManager.Instance.AddData("words", words);

                List<DataRow> keywords = dt.Rows.FindAll(word => (word["tricky"] != null && word["tricky"].ToString() == "t") || (word["nondecodeable"] != null && word["nondecodeable"].ToString() == "t"));
                GameManager.Instance.AddData("keywords", keywords);
            }

            GameManager.Instance.StartGames();
        }
    }
}
