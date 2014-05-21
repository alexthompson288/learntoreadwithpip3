using UnityEngine;
using System.Collections;

namespace Wingrove
{
    public class InstantiateOnMissing : MonoBehaviour
    {
        [SerializeField]
        private string m_prefabName; // TODO: Delete m_prefabName. This is used only for logging in XCode if the prefab is null
        [SerializeField]
        private GameObject m_prefabToInstantiate;

        void Awake()
        {
            if (m_prefabToInstantiate != null)
            {
                if (GameObject.Find(m_prefabToInstantiate.name) == null)
                {
                    GameObject newInstance = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_prefabToInstantiate, null);
                    newInstance.name = m_prefabToInstantiate.name;
                }
            } 
            else
            {
                Debug.Log("PREFAB MISSING: " + m_prefabName);
            }
        }
    }

}