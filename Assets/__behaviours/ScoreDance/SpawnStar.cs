using UnityEngine;
using System.Collections;
using Wingrove;

public class SpawnStar : MonoBehaviour {
	[SerializeField]
	private GameObject m_starPrefab;
	[SerializeField]
	private Transform m_starTweenTo;
	[SerializeField]
	private float m_starOnTweenDuration = 0.5f;
	[SerializeField]
	private string m_starSpriteName = "star_active_1024px";
	//private string m_starSpriteName = "level_choice_star_active";
	
	
	IEnumerator Start () 
	{
		//yield return new WaitForSeconds(0.5f);
		
		//WingroveAudio.WingroveRoot.Instance.PostEvent("NEW_STAR");
		
		if(SessionInformation.Instance.GetDifficulty() == SessionInformation.Instance.GetHighestLevelCompletedForApp())
		{
			yield return new WaitForSeconds(1f);
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEARS");
			
			GameObject newStar = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_starPrefab, transform);
			
			newStar.GetComponent<ParticleSystem>().enableEmission = true;
			
			newStar.GetComponent<UISprite>().spriteName = m_starSpriteName;
			
			Debug.Log("spriteName: " + newStar.GetComponent<UISprite>().spriteName);
			
			newStar.transform.localScale = Vector3.one * 5;
			iTween.ScaleFrom(newStar, Vector3.zero, m_starOnTweenDuration);
			
			iTween.PunchRotation(newStar, new Vector3(0f, 0f, 360f), m_starOnTweenDuration);
	        iTween.ShakePosition(newStar, Vector3.one * 0.02f, m_starOnTweenDuration);
			
			yield return new WaitForSeconds(m_starOnTweenDuration + 0.4f);
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
			//iTween.PunchRotation(newStar, new Vector3(0f, 0f, 180f), 0.4f);
			iTween.ShakeRotation(newStar, new Vector3(0f, 0f, 30f), 0.4f);
			
			yield return new WaitForSeconds(0.2f);
			
			
			iTween.MoveTo(newStar, m_starTweenTo.position, 1.2f);
		}
	}
	
}
