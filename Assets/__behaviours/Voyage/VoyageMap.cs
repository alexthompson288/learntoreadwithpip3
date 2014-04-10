using UnityEngine;
using System.Collections;
using Wingrove;

public class VoyageMap : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_worldMapButton;
    [SerializeField]
    private UIWidget[] m_widgetsToColor;
    [SerializeField]
    private ColorInfo.PipColor m_moduleColor;
    [SerializeField]
    private GameObject m_medalPrefab;
    [SerializeField]
    private Transform m_medalParent;

    void Awake()
    {
        m_worldMapButton.OnSingleClick += OnClickWorldMapButton;

        Color color = ColorInfo.Instance.GetColor(m_moduleColor);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }
    }

    void Start()
    {
        GameObject newMedal = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_medalPrefab, m_medalParent);
    }

	void OnClickWorldMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.ReturnToWorldMap();
    }
}
