using UnityEngine;
using System.Collections;

public class DataDisplay : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UISprite m_pictureSprite;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private bool m_showPicture = true;
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private Texture2D m_defaultTexture;

    string m_dataType;
    DataRow m_data;

    public void SetShowPicture(bool showPicture)
    {
        m_showPicture = showPicture;
    }

    public void On(string dataType, DataRow data)
    {
        Debug.Log("DataDisplay.On(): " + m_showPicture);
        m_dataType = dataType;
        m_data = data;

        SetDisplaysActive();

        if (m_showPicture)
        {
            if(m_dataType == "numbers")
            {
                m_pictureSprite.spriteName = DataHelpers.GetSpriteName(m_dataType, data);

                if(!System.String.IsNullOrEmpty(m_pictureSprite.spriteName))
                {
                    m_tweenBehaviour.On();
                }
            }
            else
            {
                m_pictureTexture.mainTexture = DataHelpers.GetPicture(m_dataType, m_data);

                if (m_pictureTexture.mainTexture == null)
                {
                    m_pictureTexture.mainTexture = m_defaultTexture;
                }

                m_tweenBehaviour.On();
            }
        } 
        else
        {
            m_label.text = DataHelpers.GetLabelText(m_dataType, m_data);
            m_tweenBehaviour.On();
        }
    }

    void SetDisplaysActive()
    {
        m_label.gameObject.SetActive(!m_showPicture);
        m_pictureSprite.gameObject.SetActive(m_showPicture && m_dataType == "numbers");
        m_pictureTexture.gameObject.SetActive(m_showPicture && m_dataType != "numbers");
    }

    public void Off()
    {
        m_tweenBehaviour.Off();
    }

    void OnClick()
    {
        if (!m_audioSource.isPlaying)
        {
            AudioClip clip = DataHelpers.GetLongAudio(m_dataType, m_data);

            if (clip != null)
            {
                m_audioSource.clip = clip;
                m_audioSource.Play();
            }
        }
    }
}
