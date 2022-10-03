using PROJECT_STACK_RUNNER.Collectibles;
using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class FinishTrigger : MonoBehaviour
	{
		[SerializeField] private Collectible[] collectiblePrefabs;
		[SerializeField] private int spawnAmount = 6;
		private void OnTriggerEnter(Collider other)
		{
			if(other.TryGetComponent(out Player player))
			{
				var sc = StackController.Instance;
				sc.canWalk = false;
				int count = sc.stacks.Count;
				for(int i = 0; i < count; i++)
				{
					var stack = sc.stacks[0];
					sc.StackPool.Release(stack);
				}

				var finishLine = sc.SpawnFinishLine();
				finishLine.transform.position += new Vector3(0,0,2.15f);
				sc.startPos = transform.position + new Vector3(0,-.25f,2.15f);

				for(int i = 0; i < spawnAmount; i++)
				{
					Instantiate(collectiblePrefabs[Random.Range(0,collectiblePrefabs.Length)],
						transform.position + new Vector3(Random.Range(-.5f,.5f),.45f,(i + 1) * 4.5f)
						,Quaternion.identity,null);
				}

				sc.canWalk = true;
				sc.SpawnStack(sc.NextPos,false);
				sc.SpawnStack(sc.NextPos,true);
				Player.Instance.Move(sc.stacks[^2].transform.position);
			}
		}
	}
}