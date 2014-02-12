using UnityEngine;
using System.Collections;

namespace WingroveAudio
{
    [AddComponentMenu("WingroveAudio/Parameter Modifier")]
	public class ParameterModifier : MonoBehaviour {
			
		public string m_parameter;
		
		public AnimationCurve m_volumeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0,1.0f), new Keyframe(1,1.0f) } );
		public AnimationCurve m_pitchCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0,1.0f), new Keyframe(1,1.0f) } );
		
		
		public float GetVolumeMultiplier(GameObject linkedObject)
		{		
			return m_volumeCurve.Evaluate(
				WingroveRoot.Instance.GetParameterForGameObject(m_parameter, linkedObject));
		}
		
		public float GetPitchMultiplier(GameObject linkedObject)
		{
			return m_pitchCurve.Evaluate(
				WingroveRoot.Instance.GetParameterForGameObject(m_parameter, linkedObject));
		}	
		
	}

}