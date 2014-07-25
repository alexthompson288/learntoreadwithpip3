using UnityEngine;
using System.Collections;

public class StartGames : MonoBehaviour 
{
    [SerializeField]
    private string[] m_gameNames;

	void OnClick()
    {
        GameManager.Instance.Reset();
        GameManager.Instance.AddGames(m_gameNames);
        GameManager.Instance.StartGames();
    }
}
