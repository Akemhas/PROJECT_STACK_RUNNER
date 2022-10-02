using System;
using System.Collections;
using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class Stack : MonoBehaviour
	{
		public Renderer rnd;
		public Rigidbody rb;
		private Coroutine _moveRoutine;

		private Vector3 XVector => new Vector3(transform.localScale.x,0,0);
		public float LeftMostCenter => (transform.position - XVector * .5f).x;
		public float RightMostCenter => (transform.position + XVector * .5f).x;
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere((transform.position - XVector * .5f),.25f);
			Gizmos.color = Color.green;
			Gizmos.DrawSphere((transform.position + XVector * .5f),.25f);
		}

		private void OnValidate()
		{
			rnd = GetComponentInChildren<Renderer>();
			rb = GetComponent<Rigidbody>();
		}

		public void Stop()
		{
			if(_moveRoutine != null) StopCoroutine(_moveRoutine);
		}

		public void MoveX(float endTarget,float duration)
		{
			if(_moveRoutine != null) StopCoroutine(_moveRoutine);
			_moveRoutine = StartCoroutine(MoveXRoutine(endTarget,duration));
		}

		private IEnumerator MoveXRoutine(float endTarget,float duration)
		{
			float elapsedTime = 0;
			Vector3 startPos = transform.position;
			Vector3 targetPos = new Vector3(endTarget,startPos.y,startPos.z);
			while(elapsedTime < duration)
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				transform.position = Vector3.Lerp(startPos,targetPos,InOutSine(elapsedTime / duration));

				if(elapsedTime >= duration)
				{
					elapsedTime = 0;
					startPos = transform.position;
					targetPos.x *= -1;
				}
			}
			_moveRoutine = null;
		}
		private static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;
	}
}