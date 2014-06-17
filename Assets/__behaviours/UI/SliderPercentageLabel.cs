using UnityEngine;
using System.Collections;

public class SliderPercentageLabel : MonoBehaviour 
{
    [SerializeField]
    private UISlider m_slider;
    [SerializeField]
    private UILabel m_label;

    void Awake()
    {
        m_slider.onChange.Add(new EventDelegate(this, "OnSliderUpdate"));
    }

	public void OnSliderUpdate()
    {
        m_label.text = Mathf.RoundToInt((m_slider.value * 100)).ToString() + "%";
    }
}
