using UnityEngine;
using System.Collections;

public class VoyageSessionButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_background;

    ColorInfo.PipColor m_color;
    int m_sessionNum;

    public void SetUp(ColorInfo.PipColor color, int sessionNum)
    {
        m_color = color;
        m_sessionNum = sessionNum;
    }

    void OnClick()
    {
        VoyageSessionBoard.Instance.On(m_color, m_sessionNum);
    }
}
