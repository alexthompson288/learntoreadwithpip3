using UnityEngine;
using System.Collections;

public class SillySensibleShowPad : MonoBehaviour {

    void OnClick()
    {
        SillySensibleCoordinator.Instance.ShowPadForWord();
    }
}
