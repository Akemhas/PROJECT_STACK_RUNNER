using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class ResetTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if(other.TryGetComponent(out Stack stack))
			{
				if(stack.IsReplace) StackController.Instance.ReplacePool.Release(stack);
				else StackController.Instance.StackPool.Release(stack);
			}
		}
	}
}