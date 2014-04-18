using UnityEngine;
using System.Collections;
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
    private GameObject m_footprintPrefab;

#if UNITY_EDITOR
    [SerializeField]
    private bool m_debugNoSpawn;
#endif

    public GameObject footprintPrefab
    {
        get
        {
            return m_footprintPrefab;
        }
    }

    VoyageMap m_currentModuleMap = null;

    IEnumerator Start()
    {
        Debug.Log("Pink: " + (int)ColorInfo.PipColor.Pink);

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        bool spawnMap = true;

#if UNITY_EDITOR
        spawnMap = !m_debugNoSpawn;
#endif

        if (spawnMap)
        {
            if (VoyageInfo.Instance.hasBookmark)
            {
                Debug.Log("Instantiate module map");
                m_movingCamera.transform.position = m_moduleMapLocation.position;
                InstantiateModuleMap(VoyageInfo.Instance.currentModule);

                if(!VoyageInfo.Instance.HasCompletedSession(VoyageInfo.Instance.currentSessionNum))
                {
                    VoyageSessionBoard.Instance.On(m_currentModuleMap.moduleColor, VoyageInfo.Instance.currentSessionNum);
                }
            } 
            else
            {
                Debug.Log("Instantiate world map");
                InstantiateWorldMap();
            }

            VoyageInfo.Instance.DestroyBookmark();
        }

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

        yield return new WaitForSeconds(m_cameraTweenDuration + 0.1f);

        if (m_currentModuleMap != null) // Defensive check: Should ALWAYS execute
        {
            Destroy(m_currentModuleMap.gameObject);
        }
    }

    void InstantiateModuleMap(int moduleNumber)
    {
        GameObject mapPrefab = FindModuleMap(moduleNumber);

        if (mapPrefab != null)
        {
            if (m_currentModuleMap != null)
            {
                int modifier = moduleNumber < (int)(m_currentModuleMap.moduleColor) ? -1 : 1;
                
                Vector3 locationParentPos = m_mapLocationParent.localPosition;
                locationParentPos.x += (m_horizontalMapDistance * modifier);
                
                m_mapLocationParent.localPosition = locationParentPos;
            }


            //GameObject newMapPrefab = FindModuleMap(
            GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(mapPrefab, m_moduleMapParent);
            newMap.transform.position = m_moduleMapLocation.position;
            
            m_currentModuleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;
            
            m_currentModuleMap.SetUp();
        }
    }

    public void MoveToModuleMap(int moduleNumber)
    {
        if (FindModuleMap(moduleNumber) != null)
        {
            InstantiateModuleMap(moduleNumber);
            StartCoroutine(MoveToModuleMapCo());
        }
    }
    
    IEnumerator MoveToModuleMapCo()
    {
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

    public void CreateBookmark(int sectionId)
    {
        VoyageInfo.Instance.CreateBookmark((int)(m_currentModuleMap.moduleColor), VoyageSessionBoard.Instance.sessionNum, sectionId);
    }

    GameObject FindModuleMap(int programmoduleId)
    {
        GameObject correctMap = null;

        foreach (GameObject map in m_moduleMapPrefabs)
        {
            if((int)(map.GetComponent<VoyageMap>().moduleColor) == programmoduleId)
            {
                correctMap = map;
                break;
            }
        }

        return correctMap;
    }

    void TweenCamera(Vector3 newPosition)
    {
        /*
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", newPosition);
        tweenArgs.Add("time", m_cameraTweenDuration);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.MoveTo(m_movingCamera, tweenArgs);
        */

        iTween.MoveTo(m_movingCamera, newPosition, m_cameraTweenDuration * 2);
    }
}
