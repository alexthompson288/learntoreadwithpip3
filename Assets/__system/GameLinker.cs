using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;

public class GameLinker : Singleton<GameLinker> 
{
	[SerializeField]
	private TextAsset m_gameNameFile;

    void Awake()
    {
        /*
        string allGameNames = m_gameNameFile.text;
        string[] separatedGameNames = allGameNames.Split(',');
        
        for(int i = 0; i < separatedGameNames.Length; ++i)
        {
            separatedGameNames[i] = StringHelpers.Edit(separatedGameNames[i], new string[] { "_" } );
        }
        
        for(int i = 0; i < separatedGameNames.Length - 1; i += 2)
        {
            m_gameNames[separatedGameNames[i]] = separatedGameNames[i + 1];
        }
        */

        using (CsvFileReader reader = new CsvFileReader(Application.dataPath + "/__system/GameNames.csv"))
        {
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

        foreach (KeyValuePair<string, string> kvp in m_gameNames)
        {
            Debug.Log(kvp.Key + " - " + kvp.Value);
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
