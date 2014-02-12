using UnityEngine;
using System.Collections;

namespace Wingrove
{

    public class InstantiateOnMissing : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_prefabToInstantiate;

        void Awake()
        {
            if (GameObject.Find(m_prefabToInstantiate.name) == null)
            {
                GameObject newInstance = SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_prefabToInstantiate, null);
                newInstance.name = m_prefabToInstantiate.name;
            }
        }
    }

}