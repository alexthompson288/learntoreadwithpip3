using UnityEngine;
using System.Collections;

public class AlphabetRoomToggleCaseButton : MonoBehaviour {

	void OnClick()
	{
		AlphabetRoomGrid.Instance.ToggleCase();
	}
}
