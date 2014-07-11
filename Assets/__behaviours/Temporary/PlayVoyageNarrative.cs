using UnityEngine;
using System.Collections;

public class PlayVoyageNarrative : MonoBehaviour 
{
	void OnClick()
    {
        Handheld.PlayFullScreenMovie("Videos/pip_narrative_intro.mp4");
    }
}
