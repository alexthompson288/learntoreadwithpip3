using UnityEngine;
using System.Collections;

public class PipPadBackButton : MonoBehaviour {

    [SerializeField]
    private PipPadBehaviour m_pipPad;

    void OnClick()
    {
        m_pipPad.Dismiss();
    }
}
