using UnityEngine;
using System.Collections;
using System.Collections.Specialized;

public class StartGamesNoData : MonoBehaviour 
{
    [SerializeField]
    private string m_returnScene = "";
    [SerializeField]
    private string[] m_sceneNames;
    [SerializeField]
    private string[] m_dbGameNames;

	void OnClick()
    {
        if (System.String.IsNullOrEmpty(m_returnScene))
        {
            m_returnScene = Application.loadedLevelName;
        }

        GameManager.Instance.SetReturnScene(m_returnScene);

        OrderedDictionary gameDictionary = new OrderedDictionary();

        foreach (string scene in m_sceneNames)
        {
            gameDictionary.Add(scene, scene);
        }

        foreach (string dbGame in m_dbGameNames)
        {
            string sceneName = GameLinker.Instance.GetSceneName(dbGame);

            if(!System.String.IsNullOrEmpty(sceneName))
            {
                gameDictionary.Add(dbGame, sceneName);
            }
        }

        GameManager.Instance.AddGames(gameDictionary);
        //GameManager.Instance.AddGames(m_scenes);

        GameManager.Instance.StartGames();
    }
}
