using UnityEngine;
using System.Collections;

public class PictureDisplay : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private bool m_showPicture = true;
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private string m_dataType;

    DataRow m_data;

    public void SetShowPicture(bool showPicture)
    {
        m_showPicture = showPicture;
        m_pictureTexture.gameObject.SetActive(m_showPicture);
        m_label.gameObject.SetActive(!m_showPicture);
    }

    public void On(string dataType, DataRow data)
    {
        m_data = data;

        Debug.Log("PictureDisplay.dataType: " + dataType);
        Debug.Log("PictureDisplay.data: " + m_data);

        if (m_showPicture && dataType != "numbers") // TODO: != "numbers" is a temporary conditional because there is currently no number art
        {
            m_pictureTexture.mainTexture = DataHelpers.GetPicture(dataType, m_data);

            Debug.Log("texture: " + m_pictureTexture.mainTexture);

            m_pictureTexture.gameObject.SetActive(m_pictureTexture.mainTexture != null);

            if (m_pictureTexture.mainTexture != null)
            {
                m_tweenBehaviour.On();
            }
        } 
        else
        {
            m_pictureTexture.gameObject.SetActive(false);
            m_label.text = DataHelpers.GetLabelText(dataType, m_data);
            m_tweenBehaviour.On();
        }
    }

    public void Off()
    {
        m_tweenBehaviour.Off();
    }
}
