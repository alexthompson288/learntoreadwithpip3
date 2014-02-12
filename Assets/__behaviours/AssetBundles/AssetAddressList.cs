using UnityEngine;
using System.Collections;

namespace LifeboatCommon
{

    public class AssetAddressList : ScriptableObject
    {

        [System.Serializable]
        class AssetDetails
        {
            [SerializeField]
            private string m_assetName;
            [SerializeField]
            private string[] m_assetURLs;
        }

        [SerializeField]
        private AssetDetails[] m_assetDetails;

    }

}