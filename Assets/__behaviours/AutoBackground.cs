using UnityEngine;
using System.Collections;

public class AutoBackground : MonoBehaviour {
	
	public enum BackgroundSelectionType
	{
		MainMenu,
		Primary,
		Secondary
	}
	
	[SerializeField]
	private BackgroundSelectionType m_selectionType;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		while(!SettingsHolder.Instance)
		{
			yield return null;
		}
		
		Texture2D replacementTexture = null;
		PipGameBuildSettings pgbs = ((PipGameBuildSettings)(SettingsHolder.Instance.GetSettings()));
		if ( pgbs != null )
		{
			switch(m_selectionType)
			{
				case BackgroundSelectionType.MainMenu:
					replacementTexture = pgbs.m_mainMenuBackground;
					break;
				case BackgroundSelectionType.Primary:
					replacementTexture = pgbs.m_primaryBackground;
					break;
				case BackgroundSelectionType.Secondary:
					replacementTexture = pgbs.m_secondaryBackground;
					break;
			}
		}
		
		if ( replacementTexture != null )
		{
			GetComponent<UITexture>().mainTexture = replacementTexture;
		}
	}
}
