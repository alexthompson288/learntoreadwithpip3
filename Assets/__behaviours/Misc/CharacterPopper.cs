using UnityEngine;
using System.Collections;

public class CharacterPopper : MonoBehaviour 
{
    [SerializeField]
    private AnimationClip[] m_anims;
    [SerializeField]
    private string[] m_spriteNames;
    [SerializeField]
    private UISprite m_sprite;

    public void PopCharacter(int animationIndex = -1, int spriteIndex = -1)
    {
        if (!animation.isPlaying)
        {
            if(spriteIndex != -1)
            {
                m_sprite.spriteName = System.String.Format("{0}_state_b", m_spriteNames[spriteIndex]);
            }

            if(animationIndex == -1)
            {
                animationIndex = Random.Range(0, m_anims.Length);
            }

            animation.Play(m_anims[animationIndex].name);
        }
		else
		{
			Debug.Log("Animation is already playing");
		}
    }
}
