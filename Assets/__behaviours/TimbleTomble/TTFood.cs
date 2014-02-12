using UnityEngine;
using System.Collections;

public class TTFood : MonoBehaviour {

	// Use this for initialization
	void OnPress (bool press) 
	{
		Debug.Log("TTFood.OnPress(" + press + ")");
		if(!press)
		{
			StartCoroutine(Off ());
		}
	}

	IEnumerator Off()
	{
		iTween.ScaleTo(gameObject, Vector3.zero, 0.5f);

		yield return new WaitForSeconds(0.5f);

		TTCoordinator.Instance.DestroyPoppedSprite(gameObject);
	}
}
