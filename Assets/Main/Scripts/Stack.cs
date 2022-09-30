using System.Collections;
using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class Stack : MonoBehaviour
	{
		public Renderer rnd;

		private Coroutine _moveRoutine;

		private void OnValidate()
		{
			rnd = GetComponentInChildren<Renderer>();
		}

		public void MoveX(float endTarget,float duration,int loopTime)
		{
			if(_moveRoutine != null) StopCoroutine(_moveRoutine);
			_moveRoutine = StartCoroutine(MoveRoutine(endTarget,duration,loopTime));
		}

		private IEnumerator MoveRoutine(float endTarget,float duration,int loopTime)
		{
			float elapsedTime = 0;
			Vector3 startPos = transform.position;
			Vector3 targetPos = new Vector3(endTarget,startPos.y,startPos.z);
			while(elapsedTime < duration)
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				transform.position = Vector3.Lerp(startPos,targetPos,elapsedTime / duration);
			}
			_moveRoutine = null;
		}
	}
}