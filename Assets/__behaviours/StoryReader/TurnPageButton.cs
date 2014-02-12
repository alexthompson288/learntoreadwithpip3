using UnityEngine;
using System.Collections;

public class TurnPageButton : MonoBehaviour
{

    [SerializeField]
    private bool m_turnBack;

    void OnClick()
    {
        StoryReaderLogic.Instance.TurnPage(m_turnBack);
    }
}
