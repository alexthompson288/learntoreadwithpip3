using UnityEngine;
using System.Collections;

public class BankToggleCase : MonoBehaviour 
{
    void Start()
    {
        gameObject.SetActive(GameManager.Instance.dataType == "alphabet");
    }

	void OnClick()
    {
        BankButton[] buttons = Object.FindObjectsOfType(typeof(BankButton)) as BankButton[];
        foreach (BankButton button in buttons)
        {
            button.ToggleCase();
        }
    }
}
