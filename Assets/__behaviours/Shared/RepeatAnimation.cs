using UnityEngine;
using System.Collections;

public class RepeatAnimation : MonoBehaviour 
{
    [SerializeField]
    private SimpleSpriteAnim m_spriteAnim;
    [SerializeField]
    private string[] m_animNames = new string [] { "ON" };
    [SerializeField]
    private Vector2 m_delayRange = new Vector2(3f, 6f);

    void Start()
    {
        if (m_spriteAnim == null)
        {
            m_spriteAnim = GetComponent<SimpleSpriteAnim>() as SimpleSpriteAnim;
        }

        if (m_spriteAnim != null)
        {
            StartCoroutine(PlayAnim());
        }
    }

	void OnAnimFinish(string animName)
    {
        // Subscribe/Unsubscribe from event every time so that this is not accidentally called if the SimpleSpriteAnim finishes a looping static 1-frame animation
        m_spriteAnim.OnAnimFinish -= OnAnimFinish;

        StartCoroutine(PlayAnim());
    }

    IEnumerator PlayAnim()
    {
        yield return new WaitForSeconds(Random.Range(m_delayRange.x, m_delayRange.y));

        // Subscribe/Unsubscribe from event every time so that this is not accidentally called if the SimpleSpriteAnim finishes a looping static 1-frame animation
        m_spriteAnim.OnAnimFinish += OnAnimFinish;

        m_spriteAnim.PlayAnimation(m_animNames [Random.Range(0, m_animNames.Length)]);
    }
}
