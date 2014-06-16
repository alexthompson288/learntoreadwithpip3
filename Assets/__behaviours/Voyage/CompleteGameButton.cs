using UnityEngine;
using System.Collections;

public class CompleteGameButton : MonoBehaviour 
{
	void OnClick()
	{
        GameManager.Instance.CompleteGame();
	}
}
