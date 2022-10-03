using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class FinishTrigger : MonoBehaviour
	{
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
				sc.canWalk = true;
				sc.SpawnStack(sc.NextPos,false);
				sc.SpawnStack(sc.NextPos,true);
				Player.Instance.Move(sc.stacks[^2].transform.position);
			}
		}
	}
}