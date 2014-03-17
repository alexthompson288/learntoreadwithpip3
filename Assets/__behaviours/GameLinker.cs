using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLinker : Singleton<GameLinker> 
{
	[SerializeField]
	private TextAsset m_gameNameFile;
	
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
		return m_gameNames [dbGameName];
	}
	
	void Awake()
	{
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
	}
}
