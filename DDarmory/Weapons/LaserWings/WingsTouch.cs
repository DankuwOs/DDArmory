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
			var ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
			var array = Physics.SphereCastAll(ray, radius, maxDist, layerMask: LayerMask.GetMask("Hitbox"));
			if (array.Length <= 0) return;
			
			foreach (var raycastHit in array)
			{
				var component = raycastHit.collider.GetComponent<Hitbox>();
				
				if (!component) continue;
				
				var health = component.health;
				var flag4 = health == null;
				if (flag4)
				{
					break;
				}
				var distance = raycastHit.distance;
				var num = damage * curve.Evaluate(distance / maxDist);
				var flag5 = num < health.minDamage;
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
