using UnityEngine;
using System.Collections;

public class OverwriteLogin : MonoBehaviour 
{
    void OnClick()
    {
        LoginInfo.Instance.Overwrite();
    }
}
