using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SessionInformation : Singleton<SessionInformation> 
{
	private string m_classicStoryLevel;
	private string m_storyType;
    private int m_currentlySelectedBook = 4;
    private int m_currentlySelectedDifficulty = 0;
    private int m_currentlySelectedNumPlayers = 1;
    private int[] m_currentPlayerIndex = new int[2];
    private int m_winningIndex = 0;
    private string m_retryScene;

    HashSet<int> m_boughtBooks = new HashSet<int>();

    HashSet<string> m_unlockedWords = new HashSet<string>();
	HashSet<string> m_unlockedLetters = new HashSet<string>();
	
	
	// TODO: It is unnecessary for m_highestLevelCompleteForApp, m_starsEarnedForHighestLevel and m_coins to be dictionaries, make them plain old integers 
    Dictionary<string, int> m_highestLevelCompleteForGame = new Dictionary<string, int>();
	Dictionary<string, int> m_highestLevelCompleteForApp = new Dictionary<string, int>();
	Dictionary<string, int> m_starsEarnedForHighestLevel = new Dictionary<string, int>();
	Dictionary<string, int> m_coins = new Dictionary<string, int>();
	//private int m_highestLevelCompleteForApp;
	//private int m_starsEarnedForHighestLevel;
	//private int m_coins;
	
	Dictionary<string, bool> m_unlockableItems = new Dictionary<string, bool>();
	
	private bool m_hasEverWonCoin;
	private bool m_hasWonRecently = false; 
	private bool m_hasNewItem = false;
	
	string m_currentApp = null;

	
    string m_selectedGame = null;
    bool m_selectedGameSupportsTwoPlayer = false;


//    WebCamTexture m_intialisedWebCamTexture;

    public static void SetDefaultPlayerVar()
    {
        SessionInformation.Instance.SetNumPlayers(1);
        SessionInformation.Instance.SetWinner(0);
        SessionInformation.Instance.SetPlayerIndex(0, 3);
    }

    void Awake()
    {
        m_retryScene = "NewGameMenu";
		
        Load();

		/*
		Object[] unlockableTextures = Resources.LoadAll("Images/collection_room_additional_images 1");
		Debug.Log("unlockableTextures: " + unlockableTextures);
		string[] unlockableItems = new string[unlockableTextures.Length];
		
		for(int index = 0; index < unlockableItems.Length; ++index)
		{
			unlockableItems[index] = unlockableTextures[index].name;
		}
		
		for(int index = 0; index < unlockableItems.Length; ++index)
		{
			string item = unlockableItems[index];

			if(!m_unlockableItems.ContainsKey(item))
			{
				m_unlockableItems.Add(item, false);
			}
		}
		
		Save ();
		*/
    }
	
	void Start()
	{
		PipGameBuildSettings pgbs = ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings()));
		if(pgbs != null)
		{
			m_currentApp = pgbs.GetConvertedName();
		}
		
		//StartCoroutine(ResetLettersAndStarVar(true));
		//StartCoroutine(ResetLettersAndStarVar(false));
		
		Save();
		
		Resources.UnloadUnusedAssets();
	}
	
	void CreateDebugStars()
	{
		int numStars = 5;
		SetStarsEarnedForHighestLevel(numStars);
		SetCoins(numStars);
		m_hasEverWonCoin = true;
		Save();
		Debug.Log("Created Debug Stars/Coins: " + GetCoins());
	}
	
	IEnumerator ResetLettersAndStarVar(bool createDebugStars)
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		yield return new WaitForSeconds(3f);
		Debug.Log("Resetting");
		m_highestLevelCompleteForApp.Clear();
		m_starsEarnedForHighestLevel.Clear();
		m_coins.Clear();
		m_unlockedLetters.Clear();
		m_unlockableItems.Clear();
		Save();
		Debug.Log("Reset");
		
		if(createDebugStars)
		{
			CreateDebugStars();
		}
		else
		{
			SetHasEverWonCoin(false);
		}
	}
	
//	public WebCamTexture GetWebCamTexture()
//    {
//        return m_intialisedWebCamTexture;
//    }
//
//    public void SetWebCamTexture(WebCamTexture wct)
//    {
//        m_intialisedWebCamTexture = wct;
//    }
	
	public bool GetHasWonRecently()
	{
		return m_hasWonRecently;
	}
	
	public void SetHasWonRecently(bool hasWonRecently)
	{
		m_hasWonRecently = hasWonRecently;
	}
	
	public bool GetHasNewItem()
	{
		return m_hasNewItem;
	}
	
	public void SetHasNewItem(bool hasNewItem)
	{
		m_hasNewItem = hasNewItem;
	}

	public string GetClassicStoryLevel()
	{
		return m_classicStoryLevel;
	}

	public void SetClassicStoryLevel(string classicStoryLevel)
	{
		m_classicStoryLevel = classicStoryLevel;
	}

	public string GetStoryType()
	{
		return m_storyType;
	}

	public void SetStoryType(string storyType)
	{
		m_storyType = storyType;
	}

    public int GetHighestLevelCompletedForGame(string gameName)
    {
        int result = 0;
        if (!string.IsNullOrEmpty(gameName))
        {
            m_highestLevelCompleteForGame.TryGetValue(gameName, out result);
        }
        return result;
    }

    public void SetHighestLevelCompletedForGame(string gameName, int level)
    {
        if (!string.IsNullOrEmpty(gameName))
        {
            m_highestLevelCompleteForGame[gameName] = level;
        }
        Save();
    }
	
	public int GetHighestLevelCompletedForApp()
	{
		int result = 0;
        if (!string.IsNullOrEmpty(m_currentApp))
        {
            m_highestLevelCompleteForApp.TryGetValue(m_currentApp, out result);
        }
        return result;
	}
	
	public void SetHighestLevelCompletedForApp(int newLevel)
	{
		if (!string.IsNullOrEmpty(m_currentApp))
        {
			// If we are changing level then reset stars 
			if(newLevel != GetHighestLevelCompletedForApp())
			{
				SetStarsEarnedForHighestLevel(0);
			}
			
            m_highestLevelCompleteForApp[m_currentApp] = newLevel;
        }
		
        Save();
	}
	
	public int GetStarsEarnedForHighestLevel()
	{
		int result = 0;
        if (!string.IsNullOrEmpty(m_currentApp))
        {
            m_starsEarnedForHighestLevel.TryGetValue(m_currentApp, out result);
        }
        return result;
	}
	
	public void SetStarsEarnedForHighestLevel(int level, int starsEarned) // Only earn new stars if level is higher than the highest completed level
	{
		if (!string.IsNullOrEmpty(m_currentApp))
		{
			if(level + 1 > GetHighestLevelCompletedForApp()) // level is zero-based, highestLevelCompleted is one-based
			{
				Debug.Log("starsEarned: " + starsEarned);
				m_starsEarnedForHighestLevel[m_currentApp] = starsEarned;
			}
		}
		Save();
	}
	
	public void SetStarsEarnedForHighestLevel(int starsEarned)
	{
		if (!string.IsNullOrEmpty(m_currentApp))
		{
			m_starsEarnedForHighestLevel[m_currentApp] = starsEarned;
		}
		Save();
	}
	
	public int GetCoins()
	{
		int result = 0;
        if (!string.IsNullOrEmpty(m_currentApp))
        {
            m_coins.TryGetValue(m_currentApp, out result);
        }
        return result;
	}
	
	public void SetCoins(int level, int coins) // Only earn new stars if level is higher than the highest completed level
	{
		if (!string.IsNullOrEmpty(m_currentApp))
		{
			if(level + 1 > GetHighestLevelCompletedForApp()) // level is zero-based, highestLevelCompleted is one-based
			{
				Debug.Log("coins: " + coins);
				m_coins[m_currentApp] = coins;
			}
		}
		Save();
	}
	
	public void SetCoins(int coins)
	{
		Debug.Log("coins: " + coins);
		if (!string.IsNullOrEmpty(m_currentApp))
        {
			m_coins[m_currentApp] = coins;
		}
		Save();
	}
	
	public bool GetHasEverWonCoin()
	{
		return m_hasEverWonCoin;
	}
	
	public void SetHasEverWonCoin(bool hasEverWonCoin)
	{
		m_hasEverWonCoin = hasEverWonCoin;
		Save ();
	}
	
	public Dictionary<string, bool> GetUnlockableItems() // TODO: Make this safer by returning a copy of the dictionary 
	{
		return m_unlockableItems;
	}
	
	public void UnlockItem(string itemName)
	{
		if(m_unlockableItems.ContainsKey(itemName))
		{
			Debug.Log("Unlocking: " + itemName);
			m_unlockableItems[itemName] = true;
		}
		else
		{
			Debug.LogError("No item with name: " + itemName);	
		}
		Save ();
		
		m_hasNewItem = true;
	}
    
    public void SetPlayerIndex(int player, int index)
    {
        m_currentPlayerIndex[player] = index;
    }

    public int GetPlayerIndexForPlayer(int player)
    {
        return m_currentPlayerIndex[player];
    }

    public void SetWinner(int winner)
    {
        m_winningIndex = winner;
    }

    public void SelectGame(string gameScene, bool supportsTwoPlayer)
    {
		Debug.Log("SelectGame() - supports 2: " + supportsTwoPlayer);
        m_selectedGame = gameScene;
        m_selectedGameSupportsTwoPlayer = supportsTwoPlayer;
    }

    public bool SupportsTwoPlayer()
    {
#if UNITY_STANDALONE
		Debug.Log("STANDALONE");
		return false;
#else
        return m_selectedGameSupportsTwoPlayer;
#endif
    }

    public string GetSelectedGame()
    {
        return m_selectedGame;
    }

    public void SetRetryScene(string retryScene)
    {
        m_retryScene = retryScene;
    }

    public string GetRetryScene()
    {
        return m_retryScene;
    }

    public int GetWinningPlayerIndex()
    {
        return m_winningIndex;
    }

    void Load()
    {
        DataSaver ds = new DataSaver("SavedInformation");
        MemoryStream data = ds.Load();
        BinaryReader br = new BinaryReader(data);

        if (data.Length != 0)
        {
            int numBooks = br.ReadInt32();
            for (int index = 0; index < numBooks; ++index)
            {
                int bookId = br.ReadInt32();
                m_boughtBooks.Add(bookId);
            }

            int numWords = br.ReadInt32();
            for (int index = 0; index < numWords; ++index)
            {
                string word = br.ReadString();
                m_unlockedWords.Add(word);
            }
			
            int numCompleted = br.ReadInt32();
            for (int index = 0; index < numCompleted; ++index)
            {
                string game = br.ReadString();
                int difficulty = br.ReadInt32();
                m_highestLevelCompleteForGame[game] = difficulty;
            }
			
			int numLetters = br.ReadInt32();
            for (int index = 0; index < numLetters; ++index)
            {
                string letter = br.ReadString();
                m_unlockedLetters.Add(letter);
            }
			
			int numCompletedApp = br.ReadInt32();
			for (int index = 0; index < numCompletedApp; ++index)
            {
                string app = br.ReadString();
                int difficulty = br.ReadInt32();
                m_highestLevelCompleteForApp[app] = difficulty;
            }
			
			int numStarsEarnedApps = br.ReadInt32();
			for (int index = 0; index < numStarsEarnedApps; ++index)
			{
				string app = br.ReadString();
				int starsEarned = br.ReadInt32();
				m_starsEarnedForHighestLevel[app] = starsEarned;
			}
			
			int numStarsSpendApps = br.ReadInt32();
			for (int index = 0; index < numStarsSpendApps; ++index)
			{
				string app = br.ReadString();
				int coins = br.ReadInt32();
				m_coins[app] = coins;
			}
			
			int numUnlockableItems = br.ReadInt32();
			for (int index = 0; index < numUnlockableItems; ++index)
			{
				string item = br.ReadString();
				bool isUnlocked = br.ReadBoolean();
				m_unlockableItems[item] = isUnlocked;
			}
			
			m_hasEverWonCoin = br.ReadBoolean();
        }
        br.Close();
        data.Close();
    }

    void Save()
    {
        DataSaver ds = new DataSaver("SavedInformation");
        MemoryStream newData = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(newData);

        bw.Write(m_boughtBooks.Count);
        foreach (int i in m_boughtBooks)
        {
            bw.Write(i);
        }

        bw.Write(m_unlockedWords.Count);
        foreach (string i in m_unlockedWords)
        {
            bw.Write(i);
        }

        bw.Write(m_highestLevelCompleteForGame.Count);
        foreach (KeyValuePair<string, int> kvp in m_highestLevelCompleteForGame)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
		
		bw.Write(m_unlockedLetters.Count);
		foreach (string i in m_unlockedLetters)
		{
			bw.Write(i);
		}
		
		bw.Write(m_highestLevelCompleteForApp.Count);
		foreach (KeyValuePair<string, int> kvp in m_highestLevelCompleteForApp)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
		
		bw.Write(m_starsEarnedForHighestLevel.Count);
		foreach (KeyValuePair<string, int> kvp in m_starsEarnedForHighestLevel)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
		
		bw.Write(m_coins.Count);
		foreach (KeyValuePair<string, int> kvp in m_coins)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }
		
		bw.Write(m_unlockableItems.Count);
		foreach (KeyValuePair<string, bool> kvp in m_unlockableItems)
		{
			bw.Write(kvp.Key);
			bw.Write(kvp.Value);
		}
		
		bw.Write(m_hasEverWonCoin);

        ds.Save(newData);

        bw.Close();
        newData.Close();
    }

    public void SelectBookId(int bookId)
    {
        m_currentlySelectedBook = bookId;
    }

    public int GetBookId()
    {
		//return 52;
        return m_currentlySelectedBook;
    }

    public bool IsBookBought(int bookId)
    {
        return m_boughtBooks.Contains(bookId);
    }

    public void SetBookPurchased(int bookId)
    {
        m_boughtBooks.Add(bookId);
        Save();
    }

    public void UnlockWord(string word)
    {
        m_unlockedWords.Add(word);
        Save();
    }

    public bool IsWordUnlocked(string word)
    {
        return m_unlockedWords.Contains(word);
    }

    public int GetNumberUnlockedWords()
    {
        return m_unlockedWords.Count;
    }
	
	public void UnlockLetter(string letter)
    {
        m_unlockedLetters.Add(letter);
        Save();
    }
	
	public bool IsLetterUnlocked(string letter)
    {
        return m_unlockedLetters.Contains(letter);
    }

    public int GetNumberUnlockedLetters()
    {
        return m_unlockedLetters.Count;
    }

    public void SetNumPlayers(int numPlayers)
    {
        m_currentlySelectedNumPlayers = numPlayers;
    }

    public int GetNumPlayers()
    {
        return m_currentlySelectedNumPlayers;
    }

    public void SetDifficulty(int difficulty)
    {
        m_currentlySelectedDifficulty = difficulty;
    }

    public int GetDifficulty()
    {
        return m_currentlySelectedDifficulty;
    }

}
