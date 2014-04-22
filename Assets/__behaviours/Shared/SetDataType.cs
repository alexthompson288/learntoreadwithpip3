using UnityEngine;
using System.Collections;

public class SetDataType : MonoBehaviour 
{
    [SerializeField]
    private string m_dataType;

    void OnClick()
    {
        GameManager.Instance.SetDataType(m_dataType);
    }
}
