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

        switch (GameManager.Instance.programme)
        {
            case "Reading1":
                prefabToInstantiate = m_readingMenu;
                break;
            case "Maths1":
                prefabToInstantiate = m_mathsMenu;
                break;
            case "Plus":
            case "Reading2":
            case "Maths2":
                prefabToInstantiate = m_plusMenu;
                break;
        }

        if (prefabToInstantiate != null)
        {
            GameObject newInstance = SpawningHelpers.InstantiateUnderWithPrefabTransforms(prefabToInstantiate, null);
            newInstance.name = prefabToInstantiate.name;
        }
    }
}
