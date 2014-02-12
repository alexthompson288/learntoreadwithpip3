using UnityEngine;
using System.Collections;
using Wingrove;

public class TTPip : Singleton<TTPip> 
{
	[SerializeField]
	SimpleSpriteAnim m_animBehaviour;
	[SerializeField]
	UISprite m_sprite;

	UIAtlas[] m_idleAnims;
	UIAtlas[] m_tickleAnims;

	bool m_playedFirstAnimation = false;

	public void UpdateAnimBehaviour(TTSettings settings)
	{
		StopAllCoroutines();

		if(m_animBehaviour != null)
		{
			Destroy(m_animBehaviour.gameObject);
		}

		m_animBehaviour = SpawningHelpers.InstantiateUnderWithIdentityTransforms(settings.m_animGo, transform).GetComponent<SimpleSpriteAnim>() as SimpleSpriteAnim;
		m_animBehaviour.SetSprite(m_sprite);
		m_animBehaviour.OnAnimFinish += OnAnimFinish;

		m_idleAnims = settings.m_idleAnims;
		m_tickleAnims = settings.m_tickleAnims;
		
		if(settings.m_transitionAnim != null && m_playedFirstAnimation)
		{
			Debug.Log("Playing transition");
			m_sprite.atlas = settings.m_transitionAnim;
			m_sprite.spriteName = m_sprite.atlas.GetListOfSprites()[0];
			m_animBehaviour.PlayAnimation("TRANSITION");
		}
		else
		{
			StartCoroutine(PlayIdle());
		}

		m_playedFirstAnimation = true;
	}

	IEnumerator PlayIdle ()
	{
		while (true)
		{
			RandomizeIdle();
			yield return new WaitForSeconds(Random.Range(2.0f, 6.0f));
			m_animBehaviour.PlayAnimation("ON_" + RandomizeIdle().ToString());
			yield return new WaitForSeconds(0.9f);
		}
	}

	int RandomizeIdle ()
	{
		int atlasIndex = Random.Range(0, m_idleAnims.Length);
		m_sprite.atlas = m_idleAnims[atlasIndex];
		m_sprite.spriteName = m_sprite.atlas.GetListOfSprites()[0];
		m_animBehaviour.PlayAnimation("OFF_" + atlasIndex.ToString());
		
		return atlasIndex;
	}

	void OnAnimFinish(string animName)
	{
		StartCoroutine(PlayIdle());
	}

	public void OnTickle()
	{
		Debug.Log("OnTickle");
		if(m_tickleAnims.Length > 0)
		{
			StopAllCoroutines();

			int atlasIndex = Random.Range(0, m_tickleAnims.Length);
			if(m_tickleAnims[atlasIndex] != null)
			{
				m_sprite.atlas = m_tickleAnims[atlasIndex];
				m_sprite.spriteName = m_sprite.atlas.GetListOfSprites()[0];
				m_animBehaviour.PlayAnimation("TICKLE_" + atlasIndex.ToString());
			}
			else
			{
				StartCoroutine(PlayIdle());
			}
		}
	}
}
