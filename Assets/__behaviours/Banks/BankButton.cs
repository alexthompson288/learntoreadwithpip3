﻿using UnityEngine;
using System.Collections;

public class BankButton : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UITexture m_texture;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private UISprite m_scoreSprite;

    //DataRow m_data;

    int m_id = -1;
    string m_labelText = "";

    public void SetUp(DataRow data)
    {
        //m_data = data;

        bool isPhoneme = GameManager.Instance.dataType == "phonemes";

        string labelTextAttribute = isPhoneme ? "phoneme" : "word";
        m_labelText = data [labelTextAttribute].ToString();
        m_label.text = m_labelText;

        if (m_texture != null)
        {
            bool hasSetTexture = false;

            if(isPhoneme)
            {
                string imageFilename =
                    System.String.Format("Images/mnemonics_images_png_250/{0}_{1}",
                                  data["phoneme"],
                                  data["mneumonic"].ToString().Replace(" ", "_"));

                Texture2D tex = Resources.Load<Texture2D>(imageFilename);

                if(tex != null)
                {
                    m_texture.mainTexture = tex;
                    hasSetTexture = true;
                }
            }

            m_texture.gameObject.SetActive(hasSetTexture);
        }

        m_id = System.Convert.ToInt32(data ["id"]);

        Refresh();
    }

    public void SetUp(string labelText)
    {
        m_texture.gameObject.SetActive(false);

        m_labelText = labelText;
        m_label.text = m_labelText;

        Refresh();
    }

    public void Refresh()
    {
        bool isAlphabet = GameManager.Instance.dataType == "alphabet";

        if (isAlphabet)
        {
            m_scoreSprite.gameObject.SetActive(BankInfo.Instance.IsAnswer(m_labelText));
        } 
        else
        {
            m_scoreSprite.gameObject.SetActive(BankInfo.Instance.IsAnswer(m_id));
        }

        if (m_scoreSprite.gameObject.activeInHierarchy)
        {
            if (isAlphabet)
            {
                m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(m_labelText) ? "level_choice_earth_a" : "level_choice_castle_a";
            } 
            else
            {
                //m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(id) ? "bank_correct" : "bank_incorrect";
                m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(m_id) ? "level_choice_earth_a" : "level_choice_castle_a";
            }
        }
    }
}