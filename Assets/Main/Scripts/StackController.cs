using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
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
		[SerializeField] private FinishTrigger finishPrefab;
		[SerializeField] private int stackCountToReachFinish = 15;
		[SerializeField] internal Vector3 startPos = new Vector3(0,-.25f,0f);
		[SerializeField] private float errorTolerance = .2f;
		[SerializeField] private Material[] stackMaterials;
		[SerializeField] private AudioSource audioSource;
		[Space] public List<Stack> stacks = new List<Stack>();
		private int _comboCount;

		private FinishTrigger _finishTrigger;
		private bool _isLeft;
		public bool canWalk;
		private float finishOffset;
		private Vector3 _localScale;
		public Vector3 NextPos => startPos + new Vector3(0,0,_localScale.z * stacks.Count);
		public ObjectPool<Stack> StackPool,ReplacePool;

		private void Awake()
		{
			_localScale = stackPrefab.transform.localScale;
			StackPool = new ObjectPool<Stack>
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
					stack.ResetStack();
					stacks.Remove(stack);
				},null,true,20,100
			);
			ReplacePool = new ObjectPool<Stack>
			(
				() =>
				{
					Stack replaceStack = Instantiate(stackPrefab);
					replaceStack.IsReplace = true;
					return replaceStack;
				},
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
					stack.ResetStack();
				},null,false,10,20
			);
		}

		private void Start()
		{
			finishOffset = stackPrefab.transform.localScale.z * stackCountToReachFinish;
			Vector3 finishPos = new Vector3(0,0,finishOffset - .35f);
			_finishTrigger = Instantiate(finishPrefab,finishPos,Quaternion.identity);
			SpawnStack(NextPos,false);
			SpawnStack(NextPos,true);
		}

		private void Update()
		{
			if(!Input.GetMouseButtonDown(0)) return;
			if(!canWalk) return;
			if(!CutStack())
			{
				if(!canWalk) return;
				SpawnAgain();
			}
			else
			{
				if(stacks.Count < stackCountToReachFinish)
				{
					SpawnStack(NextPos,true);
					Player.Instance.Move(stacks[^2].transform.position);
				}
				else
				{
					Player.Instance.Move(_finishTrigger.transform.position);
				}

			}
		}

		public FinishTrigger SpawnFinishLine()
		{
			Vector3 finishPos = new Vector3(0,0,_finishTrigger.transform.position.z + finishOffset - .35f);
			_finishTrigger = Instantiate(finishPrefab,finishPos,Quaternion.identity);
			return _finishTrigger;
		}

		private void SpawnAgain()
		{
			Stack currentStack = stacks[^1];
			Transform stackTransform = currentStack.transform;
			Stack replaceStack = ReplacePool.Get();
			Transform replaceTransform = replaceStack.transform;
			replaceTransform.position = stackTransform.position;
			replaceTransform.localScale = stackTransform.localScale;
			replaceStack.gameObject.SetActive(true);
			replaceStack.Fall();

			StackPool.Release(currentStack);
			SpawnStack(NextPos);
		}

		private bool CutStack()
		{
			Stack currentStack = stacks[^1];
			Stack lastStack = stacks[^2];
			currentStack.Stop();

			if(currentStack.LeftMostCenter > lastStack.RightMostCenter || currentStack.RightMostCenter < lastStack.LeftMostCenter) return false;

			Transform currentStackTransform = currentStack.transform;
			Vector3 pos = currentStackTransform.position;
			Vector3 scale = currentStackTransform.localScale;

			float leftDiff = currentStack.LeftMostCenter - lastStack.LeftMostCenter;

			if(Mathf.Abs(leftDiff) <= errorTolerance)
			{
				_comboCount++;
				var iLerp = Mathf.InverseLerp(0,15,_comboCount);
				audioSource.pitch = Mathf.Lerp(.5f,1.25f,iLerp);
				audioSource.Play();
				currentStackTransform.localScale = lastStack.transform.localScale;
				currentStackTransform.position = new Vector3(lastStack.transform.position.x,pos.y,pos.z);
			}
			else
			{
				_comboCount = 0;
			}

			if(leftDiff < 0)
			{
				leftDiff = lastStack.LeftMostCenter - currentStack.LeftMostCenter;

				Stack replaceStack = ReplacePool.Get();
				Transform replaceTransform = replaceStack.transform;

				replaceStack.gameObject.SetActive(true);
				replaceTransform.position = new Vector3(lastStack.LeftMostCenter - leftDiff * .5f,pos.y,pos.z);
				replaceTransform.localScale = new Vector3(leftDiff,scale.y,scale.z);
				replaceStack.Fall();

				currentStackTransform.localScale = new Vector3(currentStackTransform.localScale.x - leftDiff,scale.y,scale.z);
				currentStackTransform.position = new Vector3(lastStack.LeftMostCenter + currentStackTransform.localScale.x * .5f,pos.y,pos.z);
			}

			float rightDiff = currentStack.RightMostCenter - lastStack.RightMostCenter;

			if(rightDiff > 0)
			{
				Stack replaceStack = ReplacePool.Get();
				Transform replaceTransform = replaceStack.transform;

				replaceStack.gameObject.SetActive(true);
				replaceTransform.position = new Vector3(lastStack.RightMostCenter + rightDiff * .5f,pos.y,pos.z);
				replaceTransform.localScale = new Vector3(rightDiff,scale.y,scale.z);
				replaceStack.Fall();

				currentStackTransform.localScale = new Vector3(currentStackTransform.localScale.x - rightDiff,scale.y,scale.z);
				currentStackTransform.position = new Vector3(lastStack.RightMostCenter - currentStackTransform.localScale.x * .5f,pos.y,pos.z);
			}

			if(currentStackTransform.localScale.x < errorTolerance)
			{
				StackPool.Release(currentStack);
				currentStack.Fall();
				canWalk = false;
				SceneManager.LoadSceneAsync(0);
				return false;
			}

			return true;
		}

		public Stack SpawnStack(Vector3 spawnPos,bool shouldMove)
		{
			Stack stack = StackPool.Get();
			if(shouldMove)
			{
				stack.transform.localScale = stacks[^2].transform.localScale;
				RandomMove(stack,spawnPos);
			}
			else stack.transform.position = spawnPos;
			return stack;
		}

		public Stack SpawnStack(Vector3 spawnPos)
		{
			Stack stack = StackPool.Get();
			stack.transform.localScale = stacks[^2].transform.localScale;
			var position = leftSpawnPoint.position;
			float dist = Vector3.Distance(stacks[^1].transform.position,position);
			(Vector3,Vector3) points = dist > Mathf.Abs(position.x)
				? (leftSpawnPoint.position,rightSpawnPoint.position)
				: (rightSpawnPoint.position,leftSpawnPoint.position);
			stack.transform.position = spawnPos + points.Item1;
			stack.MoveX(points.Item2.x,2);
			return stack;
		}

		private void RandomMove(Stack stack,Vector3 spawnPos)
		{
			int random = Random.Range(1,3);
			_isLeft = random % 2 == 0;
			(Vector3,Vector3) points = (leftSpawnPoint.position,rightSpawnPoint.position);
			if(!_isLeft) (points.Item1,points.Item2) = (points.Item2,points.Item1);
			stack.transform.position = spawnPos + points.Item1;
			stack.MoveX(points.Item2.x,2);
		}
	}
}