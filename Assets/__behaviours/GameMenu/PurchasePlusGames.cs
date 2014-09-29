using UnityEngine;
using System.Collections;

public class PurchasePlusGames : Singleton<PurchasePlusGames> 
{
    [SerializeField]
    private TweenBehaviour m_tweenBehaviour;
    [SerializeField]
    private EventRelay m_purchaseGame;
    [SerializeField]
    private EventRelay m_purchaseAllGames;
    [SerializeField]
    private EventRelay m_restorePurchases;
    [SerializeField]
    private UILabel m_purchaseGameLabel;
    [SerializeField]
    private TweenBehaviour m_restoringMoveable;
    [SerializeField]
    private UILabel m_countdownLabel;

    string m_purchaseGameLabelText = "Buy {0}\n£1.49";

    int m_gameId;

    bool m_canPurchase = true;



    void Start()
    {
        m_purchaseGame.SingleClicked += OnClickButton;
        m_purchaseAllGames.SingleClicked += OnClickButton;
        m_restorePurchases.SingleClicked += OnClickButton;
    }

    public void On(DataRow game)
    {
        m_canPurchase = true;

        m_gameId = game.GetId();

        m_purchaseGameLabel.text = string.Format(m_purchaseGameLabelText, game["labeltext"].ToString().Replace("!",""));

        m_tweenBehaviour.On();
    }

    public void Off()
    {
        if (m_canPurchase)
        {
            m_canPurchase = false;
            m_tweenBehaviour.Off();
        }
    }

    void OnClickButton(EventRelay relay)
    {
        D.Log("PurchasePlusGames.OnClickButton()");
        D.Log("Name: " + relay.name);
        if (m_canPurchase)
        {
            m_canPurchase = false;
            StopCoroutine("ResetCanPurchase");
            StartCoroutine("ResetCanPurchase");

            if (relay == m_purchaseGame)
            {
                D.Log("Purchasing game");
                BuyManager.Instance.BuyGame(m_gameId);
            } 
            else if(relay == m_purchaseAllGames)
            {
                D.Log("Purchasing all games");
                BuyManager.Instance.BuyBundle(BuyManager.BundleType.AllGames);
            }
            else
            {
                StartCoroutine(BuyManager.Instance.RestorePurchases(m_restoringMoveable, m_countdownLabel));
            }
        }
    }

    IEnumerator ResetCanPurchase()
    {
        yield return new WaitForSeconds(0.75f);
        m_canPurchase = true;
    }
}
