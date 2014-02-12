using UnityEngine;
using System.Collections;

public class TransitionToChallengeMenu : MonoBehaviour 
{

	void OnClick()
	{
		Debug.Log(name + " - TransitionToChallengeMenu.OnClick()");
		TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
		if (ts != null)
		{
			string sceneToTransitionTo = "NewVoyage";

			switch(SkillProgressInformation.Instance.GetCurrentSkill())
			{
			case "ReadingLevel":
				sceneToTransitionTo = "NewChallengeMenu";
				break;
			case "SoundsLevel":
				sceneToTransitionTo = "NewChallengeMenuLetters";
				break;
			case "KeywordsLevel":
				sceneToTransitionTo = "NewChallengeMenuKeywords";
				break;
			case "SpellingLevel":
				sceneToTransitionTo = "NewChallengeMenuSpelling";
				break;
			default:
				break;
			}

			ts.ChangeLevel(sceneToTransitionTo, true);
		}
		else
		{
			Application.LoadLevel("NewVoyage");
		}
	}
}
