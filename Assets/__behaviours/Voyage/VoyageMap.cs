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
    [SerializeField]
    private GameObject m_sessionButtonPrefab;
    [SerializeField]
    private UIGrid m_sessionButtonGrid;

    public ColorInfo.PipColor moduleColor
    {
        get
        {
            return m_moduleColor;
        }
    }

    DataRow m_moduleData = null;


    public void SetUp()
    {
        Debug.Log("Looking for module color: " + m_moduleColor.ToString());
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programmodules WHERE colour='" + m_moduleColor.ToString() + "'");

        if (dt.Rows.Count > 0)
        {
            m_moduleData = dt.Rows[0];
        }

        string dataType = m_moduleData ["modulereward"].ToString();
        Debug.Log("module dataType: " + dataType);

        for (int i = 0; i < 16; ++i)
        {
            GameObject newSessionButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionButtonPrefab, m_sessionButtonGrid.transform);

            string sessionNumString = System.String.Format("{0}00{1}0", (int)m_moduleColor, i + 1);
            int sessionNum = System.Convert.ToInt32(sessionNumString);
            newSessionButton.GetComponent<VoyageSessionButton>().SetUp(m_moduleColor,  sessionNum, dataType);
        }
    }

    void Awake()
    {
        m_worldMapButton.OnSingleClick += OnClickWorldMapButton;

        foreach (ClickEvent click in m_moduleMapButtons)
        {
            click.OnSingleClick += OnClickModuleMapButton;
        }

        Color color = ColorInfo.GetColor(m_moduleColor);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }
    }

    void Start()
    {
        //GameObject newMedal = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_medalPrefab, m_medalParent);
    }

	void OnClickWorldMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.ReturnToWorldMap();
    }

    void OnClickModuleMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_moduleColor + click.GetInt());
    }
}
