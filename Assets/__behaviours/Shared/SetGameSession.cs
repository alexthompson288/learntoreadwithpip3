using UnityEngine;
using System.Collections;

public class SetGameSession : MonoBehaviour 
{
    [SerializeField]
    private Game.Session m_sessionType;

    void OnClick()
    {
        Game.SetSession(m_sessionType);
    }
}
