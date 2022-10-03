using System;
using System.Collections;
using UnityEngine;
namespace PROJECT_STACK_RUNNER.Collectibles
{
	public class RotatingCollectible : Collectible
	{
		[SerializeField] private float rotationSpeed = 10;
		private Coroutine _moveRoutine;
		[SerializeField] private bool moveUpDown;
		private void FixedUpdate()
		{
			transform.rotation = Quaternion.AngleAxis(rotationSpeed,Vector3.up) * transform.rotation;
		}

		private void Start()
		{
			if(moveUpDown) MoveY(transform.position.y + .75f,2f);
		}

		public void MoveY(float endTarget,float duration)
		{
			if(_moveRoutine != null) StopCoroutine(_moveRoutine);
			_moveRoutine = StartCoroutine(MoveYRoutine(endTarget,duration));
		}

		private IEnumerator MoveYRoutine(float endTarget,float duration)
		{
			float elapsedTime = 0;
			Vector3 startPos = transform.position;
			Vector3 targetPos = new Vector3(startPos.x,endTarget,startPos.z);
			while(elapsedTime < duration)
			{
				yield return null;
				elapsedTime += Time.deltaTime;
				transform.position = Vector3.Lerp(startPos,targetPos,InOutSine(elapsedTime / duration));

				if(elapsedTime >= duration)
				{
					elapsedTime = 0;
					(startPos,targetPos) = (targetPos,startPos);
				}
			}
			_moveRoutine = null;
		}
		private static float InOutSine(float t) => (float)(Math.Cos(t * Math.PI) - 1) / -2;
	}
}