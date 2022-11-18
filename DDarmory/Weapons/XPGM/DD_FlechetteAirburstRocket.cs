using System.Collections;
using UnityEngine;

public class DD_FlechetteAirburstRocket : Rocket
{
	private void Start()
	{
		if (!flechetteGun.actor)
		{
			flechetteGun.actor = this.sourceActor;
		}
	}

	public override void Fire(Actor sourceActor)
	{
		base.Fire(sourceActor);
		StartCoroutine(FiredRoutine());
	}

	private IEnumerator FiredRoutine()
	{
		flechetteGun.actor = this.sourceActor;
		while (true)
		{
			RaycastHit raycastHit;
			bool flag = !Physics.Raycast(rayTf.position, rayTf.forward, out raycastHit, distance);
			if (!flag)
			{
				break;
			}
			yield return null;
		}
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem ps = particleSystems[i];
			ps.gameObject.transform.SetParent(null);
			ps.transform.position = transform.position;
			ps.transform.rotation = transform.rotation;
			ps.Emit((int)ps.emission.GetBurst(0).count.constant);
			ps = null;
		}
		yield return new WaitForSeconds(delay);
		flechetteGun.SetFire(true);
		while (flechetteGun.currentAmmo > 0)
		{
			yield return null;
		}
		Detonate();
	}

	public ParticleSystem[] particleSystems;

	public Gun flechetteGun;

	public float distance = 100f;

	public float delay = 0.1f;

	public Transform rayTf;
}
