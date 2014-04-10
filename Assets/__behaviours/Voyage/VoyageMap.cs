using UnityEngine;
using System.Collections;
using Wingrove;

public class VoyageMap : MonoBehaviour 
{
    [SerializeField]
    private ClickEvent m_worldMapButton;
    [SerializeField]
    private ClickEvent[] m_moduleMapButtons;
    [SerializeField]
    private UIWidget[] m_widgetsToColor;
    [SerializeField]
    private ColorInfo.PipColor m_moduleColor;
    [SerializeField]
    private GameObject m_medalPrefab;
    [SerializeField]
    private Transform m_medalParent;

    int m_mapIndex;
    public int mapIndex
    {
        get
        {
            return m_mapIndex;
        }
    }

    public ColorInfo.PipColor moduleColor
    {
        get
        {
            return m_moduleColor;
        }
    }

    public void SetUp(int index)
    {
        m_mapIndex = index;

        VoyageSessionButton[] points = GetComponentsInChildren<VoyageSessionButton>() as VoyageSessionButton[];

        for (int i = 0; i < points.Length; ++i)
        {
            string sessionNum = System.String.Format("{0}0000{1}0", index + 1, i + 1);
            points[i].SetUp(m_moduleColor, System.Convert.ToInt32(sessionNum));
        }
    }

    void Awake()
    {
        m_worldMapButton.OnSingleClick += OnClickWorldMapButton;

        foreach (ClickEvent click in m_moduleMapButtons)
        {
            click.OnSingleClick += OnClickModuleMapButton;
        }

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

    void OnClickModuleMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.MoveToModuleMap(m_mapIndex + click.GetInt());
    }
}
