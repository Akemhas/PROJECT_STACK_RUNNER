using UnityEngine;

namespace PROJECT_STACK_RUNNER
{
	public class ResetTrigger : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if(other.TryGetComponent(out Stack stack))
			{
				StackController.Instance.ReplacePool.Release(stack);
			}
		}
	}
}