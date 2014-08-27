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
    private GameObject m_sessionButtonPrefab;
    [SerializeField]
    private UIGrid m_sessionButtonGrid;
    [SerializeField]
    private AudioSource m_locationNameAudioSource;
    [SerializeField]
    private GameObject m_pipPrefab;
    [SerializeField]
    private Transform m_pipLocation;

    static PipAnim m_pipAnim;

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

    public static void DestroyPip()
    {
        if (m_pipAnim != null)
        {
            Destroy(m_pipAnim.gameObject);
        }
    }


    IEnumerator Start()
    {
        ////////D.Log("VoyageMap.Start()");

        m_locationNameAudioSource.Play();

        StartCoroutine(GameDataBridge.WaitForDatabase());

        m_worldMapButton.SingleClicked += OnClickWorldMapButton;

        int buttonToDisable = 0;
        if (m_color == ColorInfo.PipColor.Pink)
        {
            buttonToDisable = -1;
        } 
        //else if (m_color == ColorInfo.PipColor.Orange)
        else if (m_color == ColorInfo.PipColor.Blue)
        {
            buttonToDisable = 1;
        }
        
        foreach (ClickEvent click in m_moduleMapButtons)
        {
            click.SingleClicked += OnClickModuleMapButton;

            if(click.GetInt() == buttonToDisable)
            {
                click.gameObject.SetActive(false);
            }
        }
        
        Color color = ColorInfo.GetColor(m_color);
        foreach (UIWidget widget in m_widgetsToColor)
        {
            widget.color = color;
        }

        DataRow module = DataHelpers.GetModule(m_color);
        string dataType = module != null ? module ["modulereward"].ToString() : "Custom";
        ////////D.Log("module dataType: " + dataType);

        DataTable sessionsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId + " ORDER BY number");
        foreach (DataRow session in sessionsTable.Rows)
        {
            GameObject newSessionButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_sessionButtonPrefab, m_sessionButtonGrid.transform);
            newSessionButton.GetComponent<VoyageSessionButton>().SetUp(session, m_color, dataType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        yield return null;

        m_sessionButtonGrid.Reposition();
        
        if (m_pipAnim == null)
        {
            ////////D.Log("Spawning Pip");
            Transform pipParent = VoyageCoordinator.Instance.GetPipParent();
            m_pipAnim = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, pipParent).GetComponent<PipAnim>() as PipAnim;
            
            yield return null;
            
            m_pipAnim.transform.position = m_pipLocation.position;
        }
        else
        {
            ////////D.Log("Found Pip");
            m_pipAnim.MoveToPos(m_pipLocation.position);
            //StartCoroutine(m_pipAnim.MoveToPosCo(m_pipLocation.position));
        }
    }


    IEnumerator PlayDelayedAnimation(SimpleSpriteAnim anim, float delay)
    {
        ////////D.Log(anim.transform.parent.name + " delay: " + delay);

        yield return new WaitForSeconds(delay);

        anim.PlayAnimation("ON");

        yield return new WaitForSeconds(0.2f);

        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
    }

	void OnClickWorldMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.ReturnToWorldMap();
    }

    void OnClickModuleMapButton(ClickEvent click)
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color + click.GetInt(), false);
    }

    /*
    void Awake()
    {
        //////D.Log("VoyageMap.Awake()");

        if (m_pipAnim == null)
        {
            //////D.Log("Spawning Pip");
            Transform pipParent = VoyageCoordinator.Instance.GetPipParent();

            //////D.Log("PRE - " + pipParent.transform.position);
            //////D.Log("GLO - " + m_pipLocation.position);
            //////D.Log("LOC - " + m_pipLocation.localPosition);

            Vector3 globalPos = m_pipLocation.TransformPoint(m_pipLocation.position);
            //////D.Log("globalPos: " + globalPos);

            pipParent.transform.position = m_pipLocation.position;

            //////D.Log("POST - " + pipParent.transform.position);

            m_pipAnim = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, pipParent).GetComponent<PipAnim>() as PipAnim;

            //m_pipAnim = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, VoyageCoordinator.Instance.GetPipPanelTransform()).GetComponent<PipAnim>() as PipAnim;
            //m_pipAnim.transform.position = m_pipLocation.position;

            //m_pipAnim = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pipPrefab, m_pipLocation).GetComponent<PipAnim>() as PipAnim;
            //m_pipAnim.transform.parent = VoyageCoordinator.Instance.GetPipPanelTransform();
        }
        else
        {
            //////D.Log("Found Pip");
            //m_pipAnim.transform.parent = m_pipLocation;
            StartCoroutine(m_pipAnim.MoveToPos(m_pipLocation.position));
        }
    }
     */ 
}
