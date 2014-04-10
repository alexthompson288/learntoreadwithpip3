using UnityEngine;
using System.Collections;

public class VoyageMapButton : MonoBehaviour 
{
    [SerializeField]
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private UISprite m_background;

    void Start()
    {
        m_background.color = ColorInfo.Instance.GetColor(m_color);
    }

	void OnClick()
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color);
    }
}
