using UnityEngine;
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
		[SerializeField][Space] private Stack stackPrefab;
	}
}