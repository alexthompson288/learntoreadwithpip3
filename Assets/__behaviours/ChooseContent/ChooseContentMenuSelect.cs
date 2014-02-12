using UnityEngine;
using System.Collections;

public class ChooseContentMenuSelect : MonoBehaviour 
{
	enum Menu
	{
		letters,
		words,
		keywords
	}

	[SerializeField]
	private Menu menu;

	void OnClick () 
	{
		switch(menu)
		{
		case Menu.letters:
			ChooseContentCoordinator.Instance.SetActiveLetters(true);
			ChooseContentCoordinator.Instance.SetActiveWords(false);
			ChooseContentCoordinator.Instance.SetActiveKeywords(false);
			break;
		case Menu.words:
			ChooseContentCoordinator.Instance.SetActiveLetters(false);
			ChooseContentCoordinator.Instance.SetActiveWords(true);
			ChooseContentCoordinator.Instance.SetActiveKeywords(false);
			break;
		case Menu.keywords:
			ChooseContentCoordinator.Instance.SetActiveLetters(false);
			ChooseContentCoordinator.Instance.SetActiveWords(false);
			ChooseContentCoordinator.Instance.SetActiveKeywords(true);
			break;
		}
	}

	/*
	void OnClick () 
	{
		switch(menu)
		{
		case Menu.letters:
			StartCoroutine(ChooseContentCoordinator.Instance.DisplayLetters());
			break;
		case Menu.words:
			StartCoroutine(ChooseContentCoordinator.Instance.DisplayWords());
			break;
		case Menu.keywords:
			StartCoroutine(ChooseContentCoordinator.Instance.DisplayKeywords());
			break;
			break;
		}
	}
	*/
}
