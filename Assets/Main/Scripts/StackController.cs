using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace PROJECT_STACK_RUNNER
{
	public class StackController : MonoBehaviour
	{

		#region Singleton

		private static readonly object s_Lock = new object();
		private static StackController s_instance;

		public static StackController Instance
		{
			get
			{
				lock(s_Lock)
				{
					if(s_instance == null) s_instance = (StackController)FindObjectOfType(typeof(StackController));

					return s_instance;
				}
			}
		}

		#endregion

		[Header("Spawn Points")]
		[SerializeField] private Transform leftSpawnPoint;
		[SerializeField] private Transform rightSpawnPoint;
		[SerializeField] [Space] private Stack stackPrefab;
		[SerializeField] private Vector3 startPos = new Vector3(0,-.25f,0f);
		[SerializeField] private Material[] stackMaterials;
		[Space] public List<Stack> stacks = new List<Stack>();


		private Vector3 _localScale;
		private Vector3 NextPos => startPos + new Vector3(0,0,_localScale.z * stacks.Count);
		private ObjectPool<Stack> _stackPool;
		public ObjectPool<Stack> ReplacePool;

		private void Awake()
		{
			_localScale = stackPrefab.transform.localScale;
			_stackPool = new ObjectPool<Stack>
			(
				() => Instantiate(stackPrefab),
				stack =>
				{
					stack.gameObject.SetActive(true);
					stack.rnd.sharedMaterial = stackMaterials[stacks.Count % stackMaterials.Length];
					stacks.Add(stack);
				},
				stack =>
				{
					stack.gameObject.SetActive(false);
					stack.transform.localScale = stackPrefab.transform.localScale;
					stacks.Remove(stack);
				},null,true,20,100
			);
			ReplacePool = new ObjectPool<Stack>
			(
				() => Instantiate(stackPrefab),
				stack =>
				{
					stack.gameObject.SetActive(true);
					stack.rnd.sharedMaterial = stackMaterials[(stacks.Count - 1) % stackMaterials.Length];
					stack.rb.isKinematic = false;
				},
				stack =>
				{
					stack.gameObject.SetActive(false);
					stack.transform.localScale = stackPrefab.transform.localScale;
					stack.rb.isKinematic = true;
					stack.rb.velocity = Vector3.zero;
					stack.rb.angularVelocity = Vector3.zero;
				},null,false,10,20
			);
		}

		private void Start()
		{
			SpawnStack(NextPos,false);
			SpawnStack(NextPos,true);
		}

		private void Update()
		{
			if(!Input.GetMouseButtonDown(0)) return;
			if(!CutStack()) {}
			else
			{
				SpawnStack(NextPos,true);
				Player.Instance.Move(stacks[^2].transform.position);
			}
		}

		private bool CutStack()
		{
			Stack currentStack = stacks[^1];
			Stack lastStack = stacks[^2];
			currentStack.Stop();

			if(currentStack.LeftMostCenter > lastStack.RightMostCenter || currentStack.RightMostCenter < lastStack.LeftMostCenter)
			{
				Fail();
				return false;
			}
			Transform currentStackTransform = currentStack.transform;
			Vector3 pos = currentStackTransform.position;
			Vector3 scale = currentStackTransform.localScale;
			float diff;

			diff = currentStack.LeftMostCenter - lastStack.LeftMostCenter;

			if(diff < 0)
			{
				diff = lastStack.LeftMostCenter - currentStack.LeftMostCenter;

				Stack replaceStack = ReplacePool.Get();
				Transform replaceTransform = replaceStack.transform;

				replaceStack.gameObject.SetActive(true);
				replaceTransform.position = new Vector3(lastStack.LeftMostCenter - diff * .5f,pos.y,pos.z);
				replaceTransform.localScale = new Vector3(diff,scale.y,scale.z);
				replaceStack.rb.isKinematic = false;

				currentStackTransform.localScale = new Vector3(currentStackTransform.localScale.x - diff,scale.y,scale.z);
				currentStackTransform.position = new Vector3(lastStack.LeftMostCenter + currentStackTransform.localScale.x * .5f,pos.y,pos.z);
			}

			diff = currentStack.RightMostCenter - lastStack.RightMostCenter;

			if(diff > 0)
			{
				Stack replaceStack = ReplacePool.Get();
				Transform replaceTransform = replaceStack.transform;

				replaceStack.gameObject.SetActive(true);
				replaceTransform.position = new Vector3(lastStack.RightMostCenter + diff * .5f,pos.y,pos.z);
				replaceTransform.localScale = new Vector3(diff,scale.y,scale.z);
				replaceStack.rb.isKinematic = false;

				currentStackTransform.localScale = new Vector3(currentStackTransform.localScale.x - diff,scale.y,scale.z);
				currentStackTransform.position = new Vector3(lastStack.RightMostCenter - currentStackTransform.localScale.x * .5f,pos.y,pos.z);
			}

			return true;
		}

		private void Fail() {}

		private Stack SpawnStack(Vector3 spawnPos,bool shouldMove)
		{
			Stack stack = _stackPool.Get();
			if(shouldMove) RandomMove(stack,spawnPos);
			else stack.transform.position = spawnPos;
			return stack;
		}

		private void RandomMove(Stack stack,Vector3 spawnPos)
		{
			int random = Random.Range(1,3);
			bool isLeft = random % 2 == 0;
			(Vector3,Vector3) points = (leftSpawnPoint.position,rightSpawnPoint.position);
			if(!isLeft) (points.Item1,points.Item2) = (points.Item2,points.Item1);
			stack.transform.position = spawnPos + points.Item1;
			stack.MoveX(points.Item2.x,2);
		}
	}
}