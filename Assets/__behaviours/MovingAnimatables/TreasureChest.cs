using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreasureChest : MonoBehaviour 
{
    [SerializeField]
    private GameObject[] m_bubblePrefabs;
    [SerializeField]
    private Transform[] m_bubbleLocators;
    [SerializeField]
    private Vector2 m_numBubbleRange;
    [SerializeField]
    private SimpleSpriteAnim m_spriteAnim;
    [SerializeField]
    private float m_startDelay;
    [SerializeField]
    private float m_openDelay;
    [SerializeField]
    private float m_closeDelay;


    IEnumerator Start()
    {
        m_numBubbleRange.x = Mathf.Clamp(m_numBubbleRange.x, 0, m_numBubbleRange.x);
        m_numBubbleRange.y = Mathf.Clamp(m_numBubbleRange.y, m_numBubbleRange.y, m_bubbleLocators.Length);

        m_spriteAnim.OnAnimFinish += OnAnimFinish;

        yield return new WaitForSeconds(m_startDelay);

        m_spriteAnim.PlayAnimation("OPEN");
    }

    void OnAnimFinish(string animName)
    {
        if (animName == "OPEN")
        {
            StartCoroutine(Close());
        } 
        else if (animName == "CLOSE")
        {
            StartCoroutine(Open());
        }
    }

    IEnumerator Close()
    {
        int numBubbles = Random.Range((int)m_numBubbleRange.x, (int)(m_numBubbleRange.y + 1)); // +1 to y because Range excludes upper bound for ints
        HashSet<Transform> locators = new HashSet<Transform>();
        while (locators.Count < numBubbles)
        {
            locators.Add(m_bubbleLocators[Random.Range(0, m_bubbleLocators.Length)]);
        }

        foreach (Transform locator in locators)
        {
            GameObject newBubble = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bubblePrefabs[Random.Range(0, m_bubblePrefabs.Length)], locator);
            float bubbleScale = Random.Range(0.5f, 1f);
            newBubble.transform.localScale = new Vector3(bubbleScale, bubbleScale, bubbleScale);
            newBubble.GetComponentInChildren<SingleSplineFollower>().SetSpeedModifier(Random.Range(0.85f, 1.15f));
        }

        yield return new WaitForSeconds(m_closeDelay);
        m_spriteAnim.PlayAnimation("CLOSE");
    }

    IEnumerator Open()
    {
        yield return new WaitForSeconds(m_openDelay);
        m_spriteAnim.PlayAnimation("OPEN");
    }
}
