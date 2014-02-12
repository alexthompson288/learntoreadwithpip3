using UnityEngine;
using System.Collections;

public class PlaySimpleSpriteAnim : MonoBehaviour 
{
	[SerializeField]
	private SimpleSpriteAnim m_simpleSpriteAnim;

	// Use this for initialization
	void Start () 
	{
		m_simpleSpriteAnim.PlayAnimation(m_simpleSpriteAnim.GetAnimNames()[0]);
	}
}
