using UnityEngine;
using System.Collections;

public class ChangeLoginExpiration : MonoBehaviour 
{
    void OnClick()
    {
        LoginInfo.Instance.MakeExpirationYesterday();
    }
}
