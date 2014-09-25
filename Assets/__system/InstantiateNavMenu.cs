using UnityEngine;
using System.Collections;
using Wingrove;

public class InstantiateNavMenu : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_readingMenu;
    [SerializeField]
    private GameObject m_mathsMenu;
    [SerializeField]
    private GameObject m_plusMenu;

    IEnumerator Start()
    {
        yield return null; // Wait a frame to give other classes time to set the programme

        GameObject prefabToInstantiate = null;

        string programme = GameManager.Instance.programme;

        if (programme == ProgrammeInfo.basicReading)
        {
            prefabToInstantiate = m_readingMenu;
        }
        else if (programme == ProgrammeInfo.basicMaths)
        {
            prefabToInstantiate = m_mathsMenu;
        }
        else if (programme == ProgrammeInfo.plusMaths || programme == ProgrammeInfo.plusReading)
        {
            prefabToInstantiate = m_plusMenu;
        }

        if (prefabToInstantiate != null)
        {
            GameObject newInstance = SpawningHelpers.InstantiateUnderWithPrefabTransforms(prefabToInstantiate, null);
            newInstance.name = prefabToInstantiate.name;
        }
    }
}
