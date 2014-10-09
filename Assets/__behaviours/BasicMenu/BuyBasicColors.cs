using UnityEngine;
using System.Collections;

public class BuyBasicColors : Singleton<BuyBasicColors>
{
    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;
    [SerializeField]
    private EventRelay m_buyColor;
    [SerializeField]
    private EventRelay m_buyAllColors;
    [SerializeField]
    private EventRelay m_restoreBuys;
    [SerializeField]
    private UILabel m_buyColorLabel;
    [SerializeField]
    private TweenBehaviour m_restoringMoveable;
    [SerializeField]
    private UILabel m_countdownLabel;
    
    string m_buyColorLabelText = "Buy {0}\n£1.99";
    
    ColorInfo.PipColor m_pipColor;
    
    bool m_canBuy = true;

    void Start()
    {
        m_buyColor.SingleClicked += OnClickButton;
        m_buyAllColors.SingleClicked += OnClickButton;
        m_restoreBuys.SingleClicked += OnClickButton;
    }
    
    public void On(ColorInfo.PipColor myPipColor)
    {
        m_pipColor = myPipColor;

        m_canBuy = true;

        m_buyColorLabel.text = string.Format(m_buyColorLabelText, m_pipColor.ToString());
        
        m_tweenBehaviour.On();
    }
    
    public void Off()
    {
        if (m_canBuy)
        {
            m_canBuy = false;
            m_tweenBehaviour.Off();
        }
    }
    
    void OnClickButton(EventRelay relay)
    {
#if UNITY_IPHONE
        if (m_canBuy)
        {
            m_canBuy = false;
            StopCoroutine("ResetCanBuy");
            StartCoroutine("ResetCanBuy");
            
            BuyManager.Instance.Resolved += OnBuyResolve;
            
            if (relay == m_buyColor)
            {
                BuyManager.Instance.BuyColor(m_pipColor);
            } 
            else if(relay == m_buyAllColors)
            {
                BuyManager.Instance.BuyBundle(true);
            }
            else
            {
                StartCoroutine(BuyManager.Instance.RestorePurchases(m_restoringMoveable, m_countdownLabel));
            }
        }
#endif
    }
    
    IEnumerator ResetCanBuy()
    {
        yield return new WaitForSeconds(0.75f);
        m_canBuy = true;
    }
    
    void OnBuyResolve(bool successful)
    {
        BuyManager.Instance.Resolved -= OnBuyResolve;
        m_tweenBehaviour.Off();

        (BasicMenuNavigation.Instance as BasicMenuNavigation).RefreshLocks();
    }
}
