using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoyageSessionButton : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private UILabel m_label;

    ColorInfo.PipColor m_color;
    int m_sessionNum;

    public void SetUp(ColorInfo.PipColor color, int sessionNum, string dataType)
    {
        m_color = color;
        m_sessionNum = sessionNum;

        if (VoyageInfo.Instance.HasCompletedSession(sessionNum))
        {
            m_background.spriteName = VoyageInfo.Instance.GetSessionBackground(sessionNum);
        }

        bool hasSetLabel = false;

        if (dataType != "Custom")
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE number=" + sessionNum);

            if (dt.Rows.Count > 0)
            {
                int sessionId = System.Convert.ToInt32(dt.Rows [0] ["id"]);

                string table = DataHelpers.GetTable(dataType);
                string linkingTable = DataHelpers.GetLinkingTable(dataType);
                string linkingAttribute = DataHelpers.GetLinkingAttribute(dataType);

                dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
                    System.String.Format("select * from {0} INNER JOIN {1} ON {2}={1}.id WHERE programsession_id={3}", 
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
                            string textAttribute = DataHelpers.GetTextAttribute(dataType);

                            m_label.text = row [textAttribute].ToString();

                            hasSetLabel = true;
                        }
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
        VoyageSessionBoard.Instance.On(m_color, m_sessionNum);
    }
}
