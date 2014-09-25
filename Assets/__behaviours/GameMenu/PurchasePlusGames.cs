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

    int m_gameId;

    EventRelay m_currentRelay;

    bool m_canPurchase = true;

    void Start()
    {
        m_purchaseGame.SingleClicked += OnClickPurchase;
        m_purchaseAllGames.SingleClicked += OnClickPurchase;
    }

    public void On(int myGameId)
    {
        m_gameId = myGameId;

        m_tweenBehaviour.On();
    }

    void OnClickPurchase(EventRelay relay)
    {
        if (m_canPurchase)
        {
            m_canPurchase = false;
            StopCoroutine("ResetCanPurchase");
            StartCoroutine("ResetCanPurchase");

            if (m_currentRelay == m_purchaseGame)
            {
                D.Log("Purchasing game");
                BuyManager.Instance.BuyGame(m_gameId);
            } 
            else
            {
                D.Log("Purchasing all games");
                BuyManager.Instance.BuyBundle(BuyManager.BundleType.AllGames);
            }
        }
    }

    IEnumerator ResetCanPurchase()
    {
        yield return new WaitForSeconds(0.75f);
        m_canPurchase = true;
    }
}
