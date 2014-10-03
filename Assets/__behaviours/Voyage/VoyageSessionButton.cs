using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoyageSessionButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private RotateConstantly m_rotationBehaviour;

    DataRow m_session;

    public void SetUp(DataRow session, ColorInfo.PipColor color, string dataType)
    {
        m_session = session;

        int sessionId = System.Convert.ToInt32(m_session["id"]);

        /*
        if (OldVoyageInfo.Instance.HasCompletedSession(sessionId))
        {
            m_background.spriteName = OldVoyageInfo.Instance.GetSessionBackground(sessionId);
            m_background.MakePixelPerfect();
        }
        */

        m_background.spriteName = OldVoyageInfo.Instance.GetSessionBackground(sessionId);

        m_rotationBehaviour.enabled = OldVoyageInfo.Instance.HasCompletedSession(sessionId);


        //m_background.MakePixelPerfect();

        bool hasSetLabel = false;

        if (dataType != "Custom")
        {
            string table = DataHelpers.GetTable(dataType);
            string linkingTable = DataHelpers.GetLinkingTable(dataType);
            string linkingAttribute = DataHelpers.GetLinkingAttribute(dataType);
            
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery(System.String.Format("select * from {0} INNER JOIN {1} ON {2}={1}.id WHERE programsession_id={3}", 
            new System.Object[]
            {
                linkingTable,
                table,
                linkingAttribute,
                sessionId
            }));
            
            if (dt.Rows.Count > 0)
            {
                string targetAttribute = DataHelpers.GetTargetAttribute(dataType);
                
                foreach (DataRow row in dt.Rows)
                {
                    if (row [targetAttribute] != null && row [targetAttribute].ToString() == "t")
                    {
                        m_label.text = DataHelpers.GetLabelText(row);
                        hasSetLabel = true;
                    }
                }
            }
        }

        if(!hasSetLabel)
        {
            m_label.gameObject.SetActive(false);
        }
    }

    void OnClick()
    {
        VoyageCoordinator.Instance.StartGames(m_session.GetInt("id"));
        //VoyageSessionBoard.Instance.On(m_session);
    }
}
