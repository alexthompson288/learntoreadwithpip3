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
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private GameObject m_medalPrefab;
    [SerializeField]
    private Transform m_medalParent;
    [SerializeField]
    private GameObject m_sessionButtonPrefab;
    [SerializeField]
    private UIGrid m_sessionButtonGrid;
    [SerializeField]
    private SimpleSpriteAnim[] m_delayedSpriteAnims;

    // We set the color in the editor instead of the moduleId. This is because of human error: When creating a new map, it is easier to select the correct color from a dropdown menu than to enter the correct moduleId from a wide array of options

    public ColorInfo.PipColor color
    {
        get
        {
            return m_color;
        }
    }

    public int moduleId
    {
        get
        {
            return DataHelpers.GetModuleId(m_color);
        }
    }

    public IEnumerator Start()
    {
        DataRow module = DataHelpers.GetModule(m_color);
        string dataType = module != null ? module ["modulereward"].ToString() : "Custom";
        Debug.Log("module dataType: " + dataType);

        DataTable sessionsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId + " ORDER BY number");
        foreach (DataRow session in sessionsTable.Rows)
        {
            GameObject newSessionButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionButtonPrefab, m_sessionButtonGrid.transform);
            newSessionButton.GetComponent<VoyageSessionButton>().SetUp(session, m_color, dataType);
        }

        yield return new WaitForSeconds(1.5f);

        foreach (SimpleSpriteAnim anim in m_delayedSpriteAnims)
        {
            anim.PlayAnimation("ON");
        }
    }

    void Awake()
    {
        m_worldMapButton.OnSingleClick += OnClickWorldMapButton;

        foreach (ClickEvent click in m_moduleMapButtons)
        {
            click.OnSingleClick += OnClickModuleMapButton;
        }

        Color color = ColorInfo.GetColor(m_color);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }
    }

	void OnClickWorldMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.ReturnToWorldMap();
    }

    void OnClickModuleMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color + click.GetInt());
    }
}
