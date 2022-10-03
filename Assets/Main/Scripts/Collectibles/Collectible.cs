using System.Collections;
using UnityEngine;
namespace PROJECT_STACK_RUNNER.Collectibles
{
	[RequireComponent(typeof(Collider))]
	public abstract class Collectible : MonoBehaviour,ICollectible
	{
		public int value;
		[SerializeField] protected Transform modelHolder;
		[SerializeField] private float destroyDuration = .35f;
		[SerializeField] protected ParticleSystem collectParticle;
		protected bool Interactable = true;
		public void Collect()
		{
			if(!Interactable) return;
			Interactable = false;
			var particle = Instantiate(collectParticle,transform.position,collectParticle.transform.rotation,null);
			particle.Play(true);
			StartCoroutine(DestroyRoutine());
		}

		private IEnumerator DestroyRoutine()
		{
			float elapsedTime = 0;
			Vector3 startScale = modelHolder.transform.localScale;

			while(elapsedTime < destroyDuration)
			{
				yield return null;
				elapsedTime += Time.deltaTime;

				modelHolder.transform.localScale = Vector3.Lerp(startScale,Vector3.zero,elapsedTime / destroyDuration);
			}
			gameObject.SetActive(false);
		}
	}
}