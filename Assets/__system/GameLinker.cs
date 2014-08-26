using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;

public class GameLinker : Singleton<GameLinker> 
{
	[SerializeField]
	private TextAsset m_gameNameFile;
    [SerializeField]
    private string m_filename = "/GameNames.csv";
    [SerializeField]
    private string m_relativePath;

    void Awake()
    {
        //string path = Application.dataPath;
        //path = path.Replace("Assets", ""); // Application.dataPath has "Assets" appended and GetAssetPath has "Assets" prepended, we need to remove one of these 
        //path += UnityEditor.AssetDatabase.GetAssetPath(m_gameNameFile);
        //string path = Application.dataPath + m_relativePath;
        //////D.Log("CSV Path: " + path);

        string path = Application.streamingAssetsPath + m_filename;

        using (CsvFileReader reader = new CsvFileReader(path))
        {
            ////D.Log("Reading CSV");
            CsvRow row = new CsvRow();
            while (reader.ReadRow(row))
            {
                string[] games = row.LineText.Split(',');

                if(games.Length > 1)
                {
                    for(int i = 1; i < games.Length; ++i)
                    {
                        if(!System.String.IsNullOrEmpty(games[i]))
                        {
                            m_gameNames[games[i]] = games[0];
                        }
                    }
                }
                else if(games.Length == 1 && !System.String.IsNullOrEmpty(games[0]))
                {
                    m_gameNames[games[0]] = games[0];
                }
            }
        }
    }

    void LogGameNames()
    {
        ////D.Log("LOG GAME NAMES");

        foreach (KeyValuePair<string, string> kvp in m_gameNames)
        {
            ////D.Log(kvp.Key + " - " + kvp.Value);
        }
    }
	
	// Set from file
	Dictionary<string, string> m_gameNames = new Dictionary<string, string>();

	public bool IsDBGame(string dbGameName)
	{
		return m_gameNames.ContainsKey (dbGameName);
	}

	public bool IsSceneGame(string sceneName)
	{
		return m_gameNames.ContainsValue (sceneName);
	}

	public string GetSceneName(string dbGameName)
	{
        return m_gameNames.ContainsKey (dbGameName) ? m_gameNames [dbGameName] : "";
	}
}
