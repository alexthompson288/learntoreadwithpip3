using UnityEngine;
using System.Collections;

public class StartGames : MonoBehaviour 
{
    [SerializeField]
    private string[] m_gameNames;

	void OnPress(bool isDown)
    {
        if (!isDown)
        {
            GameManager.Instance.Reset();
            GameManager.Instance.AddGames(m_gameNames);
            GameManager.Instance.StartGames();
        }
    }
}
