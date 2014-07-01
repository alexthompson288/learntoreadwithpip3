using UnityEngine;
using System.Collections;
using Wingrove;

public class InstantiateNavMenu : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_readingMenu;
    [SerializeField]
    private GameObject m_mathsMenu;

    void Start()
    {
        GameObject prefabToInstantiate = null;

        switch (GameManager.programme)
        {
            case "Reading1":
                prefabToInstantiate = m_readingMenu;
                break;
            case "Maths1":
                prefabToInstantiate = m_mathsMenu;
                break;
        }

        if (prefabToInstantiate != null)
        {
            GameObject newInstance = SpawningHelpers.InstantiateUnderWithPrefabTransforms(prefabToInstantiate, null);
            newInstance.name = prefabToInstantiate.name;
        }
    }
}
