using UnityEngine;
using System.Collections;

public class PictureDisplay : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private string m_dataType;

    DataRow m_data;

    public void On(string dataType, DataRow data)
    {
        m_data = data;

        Debug.Log("PictureDisplay.dataType: " + dataType);
        Debug.Log("PictureDisplay.data: " + m_data);

        m_pictureTexture.mainTexture = DataHelpers.GetPicture(dataType, m_data);

        Debug.Log("texture: " + m_pictureTexture.mainTexture);

        m_pictureTexture.gameObject.SetActive(m_pictureTexture.mainTexture != null);

        if (m_pictureTexture.mainTexture != null)
        {
            m_tweenBehaviour.On();
        }
    }

    public void Off()
    {
        m_tweenBehaviour.Off();
    }
}
