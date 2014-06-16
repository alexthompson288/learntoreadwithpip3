using UnityEngine;
using System.Collections;

public class VoyageSkipButton : MonoBehaviour 
{
	void OnClick()
	{
        GameManager.Instance.CompleteGame();
	}
}
