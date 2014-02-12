using UnityEngine;
using System.Collections;

namespace LifeboatCommon
{
    [System.Serializable]
    public class AssetReference
    {
        [SerializeField]
        string m_assetName;

        public T Load<T>() where T : Object
        {
            return AssetBundleLoader.Instance.FindAsset<T>(m_assetName);
        }
    }

}