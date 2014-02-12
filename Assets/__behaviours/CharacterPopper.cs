using UnityEngine;
using System.Collections;

public class CharacterPopper : MonoBehaviour {

    [SerializeField]
    private AnimationClip[] m_anims;

    public void PopCharacter()
    {
        if (!animation.isPlaying)
        {
            animation.Play(m_anims[Random.Range(0, m_anims.Length)].name);
        }
		else
		{
			Debug.Log("Animation is already playing");
		}
    }
}
