using UnityEngine;

// Token: 0x02000015 RID: 21
public class WingsTouch : MonoBehaviour
{
	public void Start()
	{
		_rayOrigin = transform;
	}

	private void FixedUpdate()
	{
		bool flag = !doDamage || !_actor.alive;
		if (!flag)
		{
			Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
			RaycastHit[] array = Physics.SphereCastAll(ray, radius, maxDist, layerMask: LayerMask.GetMask("Hitbox"));
			if (array.Length > 0)
			{
				foreach (RaycastHit raycastHit in array)
				{
					Hitbox component = raycastHit.collider.GetComponent<Hitbox>();
					bool flag3 = !component;
					if (!flag3)
					{
						Health health = component.health;
						bool flag4 = health == null;
						if (flag4)
						{
							break;
						}
						float distance = raycastHit.distance;
						float num = damage * curve.Evaluate(distance / maxDist);
						bool flag5 = num < health.minDamage;
						if (!flag5)
						{
							health.Damage(num, health.transform.position, Health.DamageTypes.Impact, _actor);
						}
					}
				}
			}
		}
	}
	public AnimationCurve curve;

	public float maxDist;

	public float damage;

	public float radius;
	
	[HideInInspector]
	public bool doDamage;

	[HideInInspector]
	public Actor _actor;

	private Transform _rayOrigin;
}
