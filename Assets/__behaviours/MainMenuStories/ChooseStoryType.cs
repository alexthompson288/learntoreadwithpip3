﻿using UnityEngine;
using System.Collections;

public class ChooseStoryType : MonoBehaviour 
{
    [SerializeField]
    ColorInfo.PipColor m_storyType;
    [SerializeField]
    private UISprite m_sprite;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private ClickEvent m_clickEvent;

    void Start()
    {
        m_sprite.color = ColorInfo.GetColor(m_storyType);
        m_label.text = ColorInfo.GetColorString(m_storyType);
    }
	
	// Update is called once per frame
	void OnClick () 
	{
#if UNITY_IPHONE
		System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
		ep.Add("StoryType", m_storyType);
		FlurryBinding.logEventWithParameters("ChooseStoryType", ep, false);
#endif

        SessionInformation.Instance.SetStoryType(ColorInfo.GetColorString(m_storyType)); // Use strings for story type just in case we want to add non-color types (eg. Classics)
	}

    void OnClick(ClickEvent click)
    {
        OnClick();
    }
}
