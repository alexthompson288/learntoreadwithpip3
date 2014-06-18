using UnityEngine;
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

    bool m_isUpper;

    string m_dataType;

    public void SetUp(string myDataType, DataRow data)
    {
        m_dataType = myDataType;

        //m_data = data;

        bool isPhoneme = m_dataType == "phonemes";

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

    public void SetUp(string myDataType, string labelText)
    {
        m_dataType = myDataType;

        if (m_texture != null)
        {
            m_texture.gameObject.SetActive(false);
        }

        m_labelText = labelText;
        m_label.text = m_labelText.ToUpper();

        m_isUpper = true;

        if (m_dataType == "alphabet")
        {
            //Vector3 labelPos = m_label.transform.localPosition;
            //labelPos.x = 20;
            //m_label.transform.localPosition = labelPos;
        }

        Refresh();
    }

    public void ToggleCase()
    {
        m_label.text = m_isUpper ? m_label.text.ToLower() : m_label.text.ToUpper();

        m_isUpper = !m_isUpper;
    }

    public void Refresh()
    {
        bool isAlphabet = m_dataType == "alphabet";

        if (isAlphabet)
        {
            m_scoreSprite.gameObject.SetActive(BankInfo.Instance.IsAnswer(m_labelText));
        } 
        else
        {
            m_scoreSprite.gameObject.SetActive(BankInfo.Instance.IsAnswer(m_id, m_dataType));
        }

        if (m_scoreSprite.gameObject.activeInHierarchy)
        {
            if (isAlphabet)
            {
                m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(m_labelText) ? "correct" : "incorrect";
            } 
            else
            {
                m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(m_id, m_dataType) ? "correct" : "incorrect";
            }
        }
    }
}
