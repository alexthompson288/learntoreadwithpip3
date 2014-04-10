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
    private Transform m_worldMapLocation;
    [SerializeField]
    private ClickEvent[] m_mapButtons;
    [SerializeField]
    private GameObject[] m_moduleMapPrefabs;
    [SerializeField]
    private Transform m_moduleMapLocation;

    VoyageMap m_currentModuleMap = null;

    void Awake()
    {
        foreach (ClickEvent mapButton in m_mapButtons)
        {
            mapButton.OnSingleClick += OnClickMapButton;
        }
    }

    public void ReturnToWorldMap()
    {
        Debug.Log("ReturnToWorldMap");
        StartCoroutine(ReturnToWorldMapCo());
    }

    IEnumerator ReturnToWorldMapCo()
    {
        iTween.MoveTo(m_movingCamera, m_worldMapLocation.position, m_cameraTweenDuration);

        yield return new WaitForSeconds(m_cameraTweenDuration + 0.1f);

        Destroy(m_currentModuleMap.gameObject);
    }

    void OnClickMapButton(ClickEvent mapButton)
    {
        int mapId = mapButton.GetInt();

        GameObject newMap = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_moduleMapPrefabs [mapId], m_moduleMapLocation);

        m_currentModuleMap = newMap.GetComponent<VoyageMap>() as VoyageMap;

        iTween.MoveTo(m_movingCamera, m_moduleMapLocation.position, m_cameraTweenDuration);
    }
}
