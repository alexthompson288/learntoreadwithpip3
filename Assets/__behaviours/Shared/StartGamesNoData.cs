using UnityEngine;
using System.Collections;

public class StartGamesNoData : MonoBehaviour 
{
    [SerializeField]
    private string m_returnScene = "";
    [SerializeField]
    private string[] m_scenes;

	void OnClick()
    {
        if (System.String.IsNullOrEmpty(m_returnScene))
        {
            m_returnScene = Application.loadedLevelName;
        }

        GameManager.Instance.SetReturnScene(m_returnScene);

        GameManager.Instance.SetScenes(m_scenes);

        GameManager.Instance.StartGames();
    }
}
