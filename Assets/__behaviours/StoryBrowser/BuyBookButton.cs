using UnityEngine;
using System.Collections;

public class BuyBookButton : MonoBehaviour 
{
    [SerializeField]
    private NewStoryBrowserBookButton m_bookButton;

    void OnClick()
    {
        m_bookButton.Buy();
    }
}
