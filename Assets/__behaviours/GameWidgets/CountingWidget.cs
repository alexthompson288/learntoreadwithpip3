using UnityEngine;
using System.Collections;

public class CountingWidget : GameWidget 
{
    [SerializeField]
    private UISprite[] m_countingSprites;
    [SerializeField]
    private string[] m_countingSpriteNames;

    public override void SetUp(DataRow number)
    {
        m_data = number;

        int value = System.Convert.ToInt32(number ["value"]);

        string spriteName = m_countingSpriteNames [Random.Range(0, m_countingSpriteNames.Length)];

        for (int i = 0; i < m_countingSprites.Length; ++i)
        {
            m_countingSprites[i].spriteName = spriteName;
            m_countingSprites[i].gameObject.SetActive(i < value);
        }

        SetUpBackground();
    }
}
