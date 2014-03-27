using UnityEngine;
using System.Collections;

public class LevelMenuCustomButton : MonoBehaviour 
{
	void OnClick () 
	{
		Game.SetSession(Game.Session.Custom);
		LevelMenuCoordinator.Instance.SelectCustom();
	}
}
