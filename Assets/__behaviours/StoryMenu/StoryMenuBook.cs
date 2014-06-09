using UnityEngine;
using System.Collections;

public class StoryMenuBook : MonoBehaviour 
{
    public delegate void BookEventHandler(StoryMenuBook book);
    public event BookEventHandler Clicked;

    [SerializeField]
    private UITexture m_storyPicture;
    [SerializeField]
    private UIDragPanelContents m_dragPanelContents;
 
    DataRow m_data;

    public DataRow GetData()
    {
        return m_data;
    }

    public void SetUp(DataRow dataRow, UIDraggablePanel draggablePanel)
    {
        m_data = dataRow;

        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from storypages where story_id=" + System.Convert.ToInt32(m_data ["id"]));

        if (dt.Rows.Count > 0)
        {
            m_storyPicture.mainTexture = DataHelpers.GetPicture("storypages", dt.Rows[0]);
        }

        m_dragPanelContents.draggablePanel = draggablePanel;
    }

    public IEnumerator Off()
    {
        float tweenDuration = 0.25f;
        iTween.ScaleTo(gameObject, Vector3.zero, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration);
        
        Destroy(gameObject);
    }

    void OnClick()
    {
        if (Clicked != null)
        {
            Clicked(this);
        }
    }
}
