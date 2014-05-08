using UnityEngine;
using System.Collections;

public class PipPadStars : Singleton<PipPadStars> 
{
    [SerializeField]
    private Color m_goldColor;
    [SerializeField]
    private Color m_redColor;

    UISprite[] m_starSprites;

    void Start()
    {
        m_starSprites = GetComponentsInChildren<UISprite>() as UISprite[];
        MakeStarsGold();
    }

    public void MakeStarsGold()
    {
        foreach (UISprite star in m_starSprites)
        {
            star.color = m_goldColor;
        }
    }

    public void MakeStarsRed()
    {
        foreach (UISprite star in m_starSprites)
        {
            star.color = m_redColor;
        }
    }
}
