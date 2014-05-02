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
}
