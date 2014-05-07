using UnityEngine;
using System.Collections;

// TODO: Find if anywhere else in project uses PlayBlinkRandomly script, if not then delete it
public class PipMainMenu : MonoBehaviour {
	[SerializeField]
	private UIAtlas[] m_atlases;
	

	// Use this for initialization
	IEnumerator Start () 
	{
		while (true)
        {
            GetComponent<SimpleSpriteAnim>().PlayAnimation("OFF_" + RandomizeAtlas().ToString());
            yield return new WaitForSeconds(Random.Range(2.0f, 6.0f));
            GetComponent<SimpleSpriteAnim>().PlayAnimation("ON_" + RandomizeAtlas().ToString());
            yield return new WaitForSeconds(0.9f);
        }
	}
	
	int RandomizeAtlas ()
	{
		int atlasIndex = Random.Range(0,2);
		UISprite sprite = GetComponent<UISprite>() as UISprite;
		sprite.atlas = m_atlases[atlasIndex];
		sprite.spriteName = "pip_front_positive_" + (atlasIndex + 1).ToString() + "0001";
		
		return atlasIndex;
	}
	
	
}
