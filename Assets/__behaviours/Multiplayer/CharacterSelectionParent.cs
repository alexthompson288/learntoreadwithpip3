using UnityEngine;
using System.Collections;

public class CharacterSelectionParent : MonoBehaviour 
{
    public static void DisableAll()
    {
        CharacterSelectionParent[] allSelectionParents = Object.FindObjectsOfType(typeof(CharacterSelectionParent)) as CharacterSelectionParent[];

        foreach (CharacterSelectionParent selectionParent in allSelectionParents)
        {
            selectionParent.gameObject.SetActive(false);
        }
    }
}
