using UnityEngine;
using System.Collections;

namespace Wingrove
{

    public static class SpawningHelpers
    {
        public static GameObject InstantiateUnderWithPrefabTransforms(GameObject prefab, Transform transform)
        {
            GameObject newObject = (GameObject)GameObject.Instantiate(prefab);
            newObject.transform.parent = transform;
            newObject.transform.localRotation = prefab.transform.localRotation;
            newObject.transform.localPosition = prefab.transform.localPosition;
            newObject.transform.localScale = prefab.transform.localScale;
            return newObject;
        }

        public static GameObject InstantiateUnderWithIdentityTransforms(GameObject prefab, Transform transform)
        {
            GameObject newObject = (GameObject)GameObject.Instantiate(prefab, transform.position, transform.rotation);
            newObject.transform.parent = transform;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localScale = Vector3.one;
			SetLayerRecursively(newObject, transform.gameObject.layer);
            return newObject;
        }

		static void SetLayerRecursively(GameObject newObject, int newLayer)
		{
			newObject.layer = newLayer;

			foreach(Transform child in newObject.transform)
			{
				SetLayerRecursively(child.gameObject, newLayer);
			}
		}

        public static Vector3 ReTranslateFromCamera(Vector3 inPos, Camera sourceCamera, Camera targetCamera, float depth)
        {
            Vector3 sourceScreen = sourceCamera.WorldToScreenPoint(inPos);

            Ray sourceRay = targetCamera.ScreenPointToRay(sourceScreen);

            Vector3 position = (sourceRay.origin + sourceRay.direction * depth);

            return position;
        }

    }

}