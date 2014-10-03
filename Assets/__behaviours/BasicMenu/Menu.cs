using UnityEngine;
using System.Collections;

public class Menu : Singleton<Menu>
{
    [SerializeField]
    protected GameObject m_callButton;

    public void HideCallButton()
    {
        m_callButton.SetActive(false);
    }

    public virtual void On() {}
}
