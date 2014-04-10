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

    VoyageMap m_currentModuleMap = null;

    public void ReturnToWorldMap()
    {
        StartCoroutine(ReturnToWorldMapCo());
    }

    IEnumerator ReturnToWorldMapCo()
    {
        if (m_worldMap != null) // Defensive check: Should NEVER execute
        {
            Destroy(m_worldMap);
        }

        m_worldMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_worldMapPrefab, m_worldMapParent);
        m_worldMap.transform.position = m_worldMapLocation.position;

        iTween.MoveTo(m_movingCamera, m_worldMapLocation.position, m_cameraTweenDuration);

        yield return new WaitForSeconds(m_cameraTweenDuration + 0.1f);

        if (m_currentModuleMap != null) // Defensive check: Should ALWAYS execute
        {
            Destroy(m_currentModuleMap.gameObject);
        }
    }

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

    /*
    void OnClickMapButton(ClickEvent mapButton)
    {
        int mapId = mapButton.GetInt();

        GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_moduleMapPrefabs [mapId], m_moduleMapLocation);

        m_currentModuleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;

        iTween.MoveTo(m_movingCamera, m_moduleMapLocation.position, m_cameraTweenDuration);
    }
    */


}
