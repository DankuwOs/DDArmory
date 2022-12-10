using UnityEngine;

public class FunkyLines : MonoBehaviour
{
    public LineRenderer lineRenderer;

    [Tooltip("Damage per fixedupdate tick")]
    public float damage = 20f;

    [HideInInspector]
    public bool positionsSet;

    [HideInInspector] public Vector3 pos;

    [HideInInspector] public Actor tgt;

    [HideInInspector] public Actor myActor;
    
    private void Update()
    {
        if (!positionsSet)
        {
            return;
        }
        lineRenderer.SetPositions(new[]
        {
            pos,
            transform.InverseTransformPoint(tgt.position)
        });
    }

    private int _i;
    
    private void FixedUpdate()
    {
        _i++;
        if (!positionsSet || _i <= 5)
        {
            
            return;
        }
        
        if (!tgt.alive)
            Destroy(gameObject, 3f);
        _i = 0;
        tgt.health.Damage(damage, pos, Health.DamageTypes.Impact, myActor);
    }
}