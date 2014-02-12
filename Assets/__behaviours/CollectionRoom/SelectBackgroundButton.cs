using UnityEngine;
using System.Collections;

public class SelectBackgroundButton : MonoBehaviour {

    [SerializeField]
    private int m_pictureChangeAmount;

    void OnClick()
    {
        CollectionRoomCoordinator.Instance.SetBackground(m_pictureChangeAmount);
    }
}
