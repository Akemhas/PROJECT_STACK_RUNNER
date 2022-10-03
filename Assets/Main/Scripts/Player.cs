using System;
using PROJECT_STACK_RUNNER.Collectibles;
using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class Player : MonoBehaviour
	{
		#region Singleton

		private static readonly object s_Lock = new object();
		private static Player s_instance;

		public static Player Instance
		{
			get
			{
				lock(s_Lock)
				{
					if(s_instance == null) s_instance = (Player)FindObjectOfType(typeof(Player));

					return s_instance;
				}
			}
		}

		#endregion

		[SerializeField] private CharacterController cc;
		[SerializeField] private Animator animator;
		[SerializeField] private float movementSpeed = 10;
		[SerializeField] private float stoppingDistance = .25f;

		private readonly int IsRunning = Animator.StringToHash("IsRunning");
		private bool _isMoving;
		private Vector3 _target;
		private Quaternion _targetRotation;

		private void OnValidate()
		{
			animator = GetComponentInChildren<Animator>();
			cc = GetComponent<CharacterController>();
		}

		private void OnTriggerEnter(Collider other)
		{
			if(other.TryGetComponent(out ICollectible collectible)) collectible.Collect();
		}

		private void Update()
		{
			if(!_isMoving) return;
			Vector3 position = transform.position;
			cc.SimpleMove((_target - position).normalized * movementSpeed);
			transform.rotation = Quaternion.Lerp(transform.rotation,_targetRotation,Time.deltaTime * movementSpeed);
			float remainingDistance = Vector3.Distance(_target,position);
			if(remainingDistance > stoppingDistance) return;
			_isMoving = false;
			animator.SetBool(IsRunning,_isMoving);
		}

		public void Move(Vector3 target)
		{
			_isMoving = true;
			_target = target;
			_targetRotation = Quaternion.LookRotation(_target - transform.position);
			animator.SetBool(IsRunning,_isMoving);
		}
	}
}