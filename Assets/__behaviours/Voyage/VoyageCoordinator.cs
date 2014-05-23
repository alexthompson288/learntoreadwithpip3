using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class VoyageCoordinator : Singleton<VoyageCoordinator> 
{
    [SerializeField]
    private GameObject m_movingCamera;
    [SerializeField]
    private float m_cameraTweenDuration;
    [SerializeField]
    private Transform m_mapLocationParent;
    [SerializeField]
    private Transform m_worldMapParent;
    [SerializeField]
    private Transform m_worldMapLocation;
    [SerializeField]
    private GameObject[] m_moduleMapPrefabs;
    [SerializeField]
    private Transform m_moduleMapParent;
    [SerializeField]
    private Transform m_moduleMapLocation;
    [SerializeField]
    private GameObject m_worldMapPrefab;
    [SerializeField]
    private GameObject m_worldMap;
    [SerializeField]
    private int m_horizontalMapDistance = 1024;
    [SerializeField]
    private AudioSource m_welcomeVocalSource;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_debugNoSpawn;
#endif

    VoyageMap m_currentModuleMap = null;
    public ColorInfo.PipColor currentColor
    {
        get
        {
            return m_currentModuleMap != null ? m_currentModuleMap.color : ColorInfo.PipColor.Pink;
        }
    }

    int m_sessionId;
    public void SetSessionId(int sessionId)
    {
        Debug.Log("SetSessionId: " + sessionId);
        m_sessionId = sessionId;
    }

    int m_sectionId;
    public void SetSectionId(int sectionId)
    {
        m_sectionId = sectionId;
    }

    int CompareModuleMapColor(GameObject a, GameObject b)
    {
        VoyageMap mapA = a.GetComponent<VoyageMap>() as VoyageMap;
        VoyageMap mapB = b.GetComponent<VoyageMap>() as VoyageMap;
        
        if (mapA.color < mapB.color)
        {
            return -1;
        } 
        else if (mapA.color > mapB.color)
        {
            return 1;
        } 
        else
        {
            return 0;
        }
    }

    IEnumerator Start()
    {
        // Sort the module maps by color. When moving between module maps, we move by array index
        System.Array.Sort(m_moduleMapPrefabs, CompareModuleMapColor);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        // When debugging in editor, we don't always want to spawn a map
        bool spawnMap = true;

#if UNITY_EDITOR
        spawnMap = !m_debugNoSpawn;
#endif

        // spawnMap is always true outside the editor
        if (spawnMap)
        {
            bool hasSpawnedModuleMap = false;

            // If VoyageInfo has a bookmark then we might want to spawn in a module map instead of the world map
            if (VoyageInfo.Instance.hasBookmark)
            {
                Debug.Log("Instantiate module map");

                int moduleId = VoyageInfo.Instance.currentModuleId;

                for(int i = 0; i < m_moduleMapPrefabs.Length; ++i)
                {
                    if(m_moduleMapPrefabs[i].GetComponent<VoyageMap>().moduleId == moduleId)
                    {
                        m_movingCamera.transform.position = m_moduleMapLocation.position;
                        InstantiateModuleMap(i);
                        hasSpawnedModuleMap = true;

                        // If we have not completed the current session then call the SessionBoard
                        /*
                        if(!VoyageInfo.Instance.HasCompletedSession(VoyageInfo.Instance.currentSessionId))
                        {
                            Debug.Log("Session Not Complete");

                            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE id=" + VoyageInfo.Instance.currentSessionId);
                            if(dt.Rows.Count > 0)
                            {
                                VoyageSessionBoard.Instance.On(dt.Rows[0]);
                            }
                            else
                            {
                                Debug.LogError("Could not find sessions with id: " + VoyageInfo.Instance.currentSessionId);
                            }
                        }
                        else
                        {
                            Debug.Log("Session Complete");
                        }
                        */
                    }
                }
            } 

            // If we have not spawned a module map then spawn the world map instead
            if(!hasSpawnedModuleMap)
            {
                Debug.Log("Instantiate world map");
                InstantiateWorldMap();
            }

            VoyageInfo.Instance.DestroyBookmark();
        }

        yield return new WaitForSeconds(0.5f);

        m_welcomeVocalSource.Play();
    }

    void TweenCamera(Vector3 newPosition)
    {
        iTween.MoveTo(m_movingCamera, newPosition, m_cameraTweenDuration * 2);
    }

    void InstantiateWorldMap()
    {
        if (m_worldMap != null) // Defensive check: Should NEVER execute
        {
            Destroy(m_worldMap);
        }
        
        m_worldMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_worldMapPrefab, m_worldMapParent);
        m_worldMap.transform.position = m_worldMapLocation.position;
    }

    public void ReturnToWorldMap()
    {
        InstantiateWorldMap();
        StartCoroutine(ReturnToWorldMapCo());
    }

    IEnumerator ReturnToWorldMapCo()
    {
        TweenCamera(m_worldMapLocation.position);

        WingroveAudio.WingroveRoot.Instance.PostEvent("AMBIENCE_STOP");

        yield return new WaitForSeconds(m_cameraTweenDuration + 0.1f);

        if (m_currentModuleMap != null) // Defensive check: Should ALWAYS execute
        {
            Destroy(m_currentModuleMap.gameObject);
        }
    }

    void InstantiateModuleMap(int mapIndex)
    {
        GameObject mapPrefab = FindModuleMapPrefab(mapIndex);

        if (mapPrefab != null)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("AMBIENCE_STOP");

            // If we already have a current module map, the new map must spawn either to the left or right of the current one
            if (m_currentModuleMap != null)
            {
                int modifier = mapIndex < (int)(m_currentModuleMap.color) ? -1 : 1;
                
                Vector3 locationParentPos = m_mapLocationParent.localPosition;
                locationParentPos.x += (m_horizontalMapDistance * modifier);
                
                m_mapLocationParent.localPosition = locationParentPos;
            }

            // Spawn the newMap
            GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(mapPrefab, m_moduleMapParent);
            newMap.transform.position = m_moduleMapLocation.position;
            
            m_currentModuleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;
        }
    }

    public void MoveToModuleMap(int mapIndex)
    {
        if (FindModuleMapPrefab(mapIndex) != null)
        {
            InstantiateModuleMap(mapIndex);
            StartCoroutine(MoveToModuleMapCo());
        }
    }
    
    IEnumerator MoveToModuleMapCo()
    {
        //WingroveAudio.WingroveRoot.Instance.PostEvent("MUSIC_STOP");
        TweenCamera(m_moduleMapLocation.position);
        
        yield return new WaitForSeconds(m_cameraTweenDuration);
        
        if (m_worldMap != null)
        {
            Destroy(m_worldMap);
        }

        VoyageMap[] maps = Object.FindObjectsOfType(typeof(VoyageMap)) as VoyageMap[];
        foreach (VoyageMap map in maps)
        {
            if (map != m_currentModuleMap)
            {
                Destroy(map.gameObject);
            }
        }
    }

    GameObject FindModuleMapPrefab(int mapIndex)
    {
        GameObject correctMap = null;

        foreach (GameObject map in m_moduleMapPrefabs)
        {
            if((int)(map.GetComponent<VoyageMap>().color) == mapIndex)
            {
                correctMap = map;
                break;
            }
        }

        return correctMap;
    }

    int m_minimumDataCount = 6;

    void AddExtraData(List<DataRow> dataPool, List<DataRow> extraDataPool)
    {
        if ((int)m_currentModuleMap.color > 0)
        {
            int safetyCounter = 0;
            while (dataPool.Count < m_minimumDataCount && safetyCounter < 100)
            {
                DataRow data = extraDataPool [Random.Range(0, extraDataPool.Count)];

                if (!dataPool.Contains(data))
                {
                    dataPool.Add(data);
                }

                ++safetyCounter;
            }
        }
    }

    public void StartGames(int sessionId)
    {
        m_sessionId = sessionId;

        EnviroManager.Instance.SetEnvironment((int)(m_currentModuleMap.color));
        VoyageInfo.Instance.CreateBookmark(m_currentModuleMap.moduleId, m_sessionId, 0);

        DataTable sectionsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + m_sessionId);

        if (sectionsTable.Rows.Count > 0)
        {
            foreach(DataRow section in sectionsTable.Rows)
            {
                DataRow game = DataHelpers.FindGameForSection(section);

                if(game != null)
                {
                    GameManager.Instance.AddGame(game);
                }
            }

            GameManager.Instance.AddGame("NewSessionComplete");

            // Set return scene
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // Set data
            int previousModuleId = DataHelpers.GetPreviousModuleId(m_currentModuleMap.color);
            
            SqliteDatabase db = GameDataBridge.Instance.GetDatabase(); // Locally store the database because we're going to call it a lot
            
            // Phonemes
            DataTable dt = db.ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + m_sessionId);
            if(dt.Rows.Count > 0)
            {
                List<DataRow> phonemePool = dt.Rows;
                
                if(phonemePool.Count < m_minimumDataCount)
                {
                    AddExtraData(phonemePool, DataHelpers.GetModulePhonemes(previousModuleId));
                }
                
                GameManager.Instance.AddData("phonemes", phonemePool);
                GameManager.Instance.AddTargetData("phonemes", phonemePool.FindAll(x => x["is_target_phoneme"] != null && x["is_target_phoneme"].ToString() == "t"));
            }
            
            // Words/Keywords
            dt = db.ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE programsession_id=" + m_sessionId);
            if(dt.Rows.Count > 0)
            {
                // Words
                List<DataRow> words = dt.Rows.FindAll(word => (word["tricky"] == null || word["tricky"].ToString() == "f") && (word["nondecodeable"] == null || word["nondecodeable"].ToString() == "f"));
                
                if(words.Count < m_minimumDataCount)
                {
                    AddExtraData(words, DataHelpers.GetModuleWords(previousModuleId));
                    
                }
                
                GameManager.Instance.AddData("words", words);
                GameManager.Instance.AddTargetData("words", words.FindAll(x => x["is_target_word"] != null && x["is_target_word"].ToString() == "t"));
                
                
                // Keywords
                List<DataRow> keywords = dt.Rows.FindAll(word => (word["tricky"] != null && word["tricky"].ToString() == "t") || (word["nondecodeable"] != null && word["nondecodeable"].ToString() == "t"));
                
                if(keywords.Count < m_minimumDataCount)
                {
                    AddExtraData(keywords, DataHelpers.GetModuleKeywords(previousModuleId));
                }
                
                GameManager.Instance.AddData("keywords", keywords);
                GameManager.Instance.AddTargetData("keywords", keywords.FindAll(x => x["is_target_word"] != null && x["is_target_word"].ToString() == "t"));
            }
            
            // Quiz Questions and Captions
            dt = db.ExecuteQuery("select * from datasentences WHERE programsession_id=" + m_sessionId);
            
            GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x["correctsentence"] != null && x["correctsentence"].ToString() == "t"));
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x["quiz"] != null && x["quiz"].ToString() == "t"));
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("AMBIENCE_STOP");
            
            GameManager.Instance.StartGames();
        }
    }

    public void StartGame(DataRow section)
    {
        m_sectionId = System.Convert.ToInt32(section ["id"]);

        EnviroManager.Instance.SetEnvironment((int)(m_currentModuleMap.color));
        VoyageInfo.Instance.CreateBookmark(m_currentModuleMap.moduleId, m_sessionId, m_sectionId);

        Debug.Log(System.String.Format("Voyage: {0} - {1} - {2}", (int)(m_currentModuleMap.color), m_sessionId, m_sectionId));

        DataRow game = DataHelpers.FindGameForSection(section);

        if (game != null)
        {
            string dbGameName = game["name"].ToString();
            string sceneName = GameLinker.Instance.GetSceneName(dbGameName);
            
            // Set scenes
            // If the player has more than one section remaining in this session, then they will not have completed at the end of the game
            if(VoyageInfo.Instance.GetNumRemainingSections(m_sessionId) > 1)
            {
                GameManager.Instance.AddGame(game);
            }
            else
            {
                GameManager.Instance.AddGame(game);
                GameManager.Instance.AddGame("NewSessionComplete");
            }
        }
    }
}
