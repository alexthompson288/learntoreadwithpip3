using UnityEngine;
using System.Collections;

public class ShoppingListPlayer : PlusGamePlayer
{
    DataRow m_currentData;

    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }

    public IEnumerator ClearQuestion()
    {
        yield return null;
    }
}
