using UnityEngine;
using System.Collections;

public class SelectSkinButton : MonoBehaviour {

    [SerializeField]
    private int m_selectSkin;

    void OnClick()
    {
        SplatGameCoordinator.Instance.SelectSkin(m_selectSkin);
    }
}
