using UnityEngine;

public class WingsTouch : MonoBehaviour
{
	public void Start()
	{
		_rayOrigin = transform;
	}

	private void Update()
	{
		if (doDamage && _actor.alive)
		{
			Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
			RaycastHit[] array = Physics.SphereCastAll(ray, radius, maxDist, layerMask: LayerMask.GetMask("Hitbox"));
			if (array.Length <= 0) return;
			
			foreach (RaycastHit raycastHit in array)
			{
				Hitbox component = raycastHit.collider.GetComponent<Hitbox>();
				
				if (!component) continue;
				
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
