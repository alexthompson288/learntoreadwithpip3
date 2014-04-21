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


    DataRow m_data;

    public void SetUp(DataRow data)
    {
        m_data = data;

        bool isPhoneme = GameManager.Instance.dataType == "phonemes";

        string labelText = isPhoneme ? "phoneme" : "word";
        m_label.text = data[labelText].ToString();

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

        CheckCorrect(System.Convert.ToInt32(m_data ["id"]));
    }

    public void SetUp(int id, string labelText)
    {
        m_label.text = labelText;

        CheckCorrect(id);
    }

    public void CheckCorrect(int id)
    {
        m_scoreSprite.gameObject.SetActive(BankInfo.Instance.IsAnswer(id));

        if (m_scoreSprite.gameObject.activeInHierarchy)
        {
            //m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(id) ? "bank_correct" : "bank_incorrect";
            m_scoreSprite.spriteName = BankInfo.Instance.IsCorrect(id) ? "level_choice_earth_a" : "level_choice_farm_a";
        }
    }
}
