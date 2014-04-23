using UnityEngine;
using System.Collections;

public class CharacterPopper : MonoBehaviour {

    [SerializeField]
    private AnimationClip[] m_anims;

    public void PopCharacter(int index = -1)
    {
        if (!animation.isPlaying)
        {
            if(index == -1)
            {
                index = Random.Range(0, m_anims.Length);
            }

            animation.Play(m_anims[index].name);
        }
		else
		{
			Debug.Log("Animation is already playing");
		}
    }
}
