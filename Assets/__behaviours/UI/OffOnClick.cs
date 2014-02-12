using UnityEngine;
using System.Collections;

public class OffOnClick : MonoBehaviour {

    [SerializeField]
    private Transform m_targetPosition;

    void OnClick()
    {
        iTween.MoveTo(gameObject, m_targetPosition.transform.position, 2.5f);
    }
}
