using UnityEngine;
using System.Collections;

public class ChooseGameButton : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_icon;

	public void SetUp(DataRow game)
    {
        m_label.text = game["name"].ToString();
        
        if(game["skill"] != null)
        {
            m_icon.spriteName = game["skill"].ToString().ToLower() + "_complete";
        }
    }
}
