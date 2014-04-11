﻿using UnityEngine;
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

    VoyageMap m_currentModuleMap = null;
    public VoyageMap currentModuleMap
    {
        get
        {
            return m_currentModuleMap;
        } 
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());


        if (VoyageInfo.Instance.hasCurrent)
        {
            Debug.Log("Instantiate module map");
            m_movingCamera.transform.position = m_moduleMapLocation.position;
            InstantiateModuleMap(VoyageInfo.Instance.currentModule);
            
            VoyageSessionBoard.Instance.On(VoyageInfo.Instance.currentColor, VoyageInfo.Instance.currentSessionNum);
        } 
        else
        {
            Debug.Log("Instantiate world map");
            InstantiateWorldMap();
        }

        VoyageInfo.Instance.SetCurrentLocationNull();

        /*
        Debug.Log("Instantiate module map");
        m_movingCamera.transform.position = m_moduleMapLocation.position;
        InstantiateModuleMap(0);

        Debug.Log("VoyageSessionBoard: " + VoyageSessionBoard.Instance);
        VoyageSessionBoard.Instance.On(ColorInfo.PipColor.Pink, 1000010);
        */
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
        iTween.MoveTo(m_movingCamera, m_worldMapLocation.position, m_cameraTweenDuration);

        yield return new WaitForSeconds(m_cameraTweenDuration + 0.1f);

        if (m_currentModuleMap != null) // Defensive check: Should ALWAYS execute
        {
            Destroy(m_currentModuleMap.gameObject);
        }
    }

    void InstantiateModuleMap(int mapIndex)
    {
        if (mapIndex > -1 && mapIndex < m_moduleMapPrefabs.Length)
        {
            if (m_currentModuleMap != null)
            {
                int modifier = mapIndex < m_currentModuleMap.mapIndex ? -1 : 1;
                
                Vector3 locationParentPos = m_mapLocationParent.localPosition;
                locationParentPos.x += (m_horizontalMapDistance * modifier);
                
                m_mapLocationParent.localPosition = locationParentPos;
            }
            
            GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_moduleMapPrefabs [mapIndex], m_moduleMapParent);
            newMap.transform.position = m_moduleMapLocation.position;
            
            m_currentModuleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;
            
            m_currentModuleMap.SetUp(mapIndex);
        }
    }

    public void MoveToModuleMap(int mapIndex)
    {
        if (mapIndex > -1 && mapIndex < m_moduleMapPrefabs.Length)
        {
            InstantiateModuleMap(mapIndex);
            StartCoroutine(MoveToModuleMapCo());
        }
    }
    
    IEnumerator MoveToModuleMapCo()
    {
        iTween.MoveTo(m_movingCamera, m_moduleMapLocation.position, m_cameraTweenDuration);
        
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

    /*
    public void MoveToModuleMap(int mapIndex)
    {
        if (mapIndex > -1 && mapIndex < m_moduleMapPrefabs.Length)
        {
            StartCoroutine(MoveToModuleMapCo(mapIndex));
        }
    }

    IEnumerator MoveToModuleMapCo(int mapIndex)
    {
        if (m_currentModuleMap != null)
        {
            int modifier = mapIndex < m_currentModuleMap.mapIndex ? -1 : 1;

            Vector3 locationParentPos = m_mapLocationParent.localPosition;
            locationParentPos.x += (m_horizontalMapDistance * modifier);

            m_mapLocationParent.localPosition = locationParentPos;
        }

        GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_moduleMapPrefabs [mapIndex], m_moduleMapParent);
        newMap.transform.position = m_moduleMapLocation.position;
        
        VoyageMap moduleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;

        moduleMap.SetUp(mapIndex);

        iTween.MoveTo(m_movingCamera, m_moduleMapLocation.position, m_cameraTweenDuration);

        yield return new WaitForSeconds(m_cameraTweenDuration);

        if (m_worldMap != null)
        {
            Destroy(m_worldMap);
        }

        if (m_currentModuleMap != null)
        {
            Destroy(m_currentModuleMap.gameObject);
        }

        m_currentModuleMap = moduleMap;
    }
    */
}
