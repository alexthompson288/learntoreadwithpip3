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

    public void SetDataType(string dataType)
    {
        m_dataType = dataType;
    }

    public void On(DataRow data)
    {
        On(data, m_dataType);
    }

    public void On(DataRow data, string dataType)
    {
        m_data = data;

        m_pictureTexture.mainTexture = DataHelpers.GetPicture(dataType, m_data);

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
