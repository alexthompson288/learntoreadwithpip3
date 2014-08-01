using UnityEngine;
using System.Collections;

public class LabelTimer : Timer 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private int m_decimalPlaces = 0;

    public void SetDecimalPlaces(int myDecimalPlaces)
    {
        m_decimalPlaces = myDecimalPlaces;
    }

    protected override void Refresh()
    {
        // TODO: User m_decimal places instead of forcing an integer
        m_label.text = Mathf.RoundToInt(m_timeRemaining).ToString();
    }
}
