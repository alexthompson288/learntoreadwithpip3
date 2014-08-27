﻿using UnityEngine;
using System.Collections;

public class BankToggleCase : MonoBehaviour 
{
    IEnumerator Start()
    {
        yield return StartCoroutine(BankIndexCoordinator.WaitForSetDataType());
        gameObject.SetActive(BankIndexCoordinator.Instance.GetDataType() == "alphabet");
    }

	void OnPress(bool isDown)
    {
        if (!isDown)
        {
            BankButton[] buttons = Object.FindObjectsOfType(typeof(BankButton)) as BankButton[];
            foreach (BankButton button in buttons)
            {
                button.ToggleCase();
            }
        }
    }
}
