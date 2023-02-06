using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class FunkyLines : MonoBehaviour
{
    public LineRenderer lineRenderer;

    [Tooltip("Damage per fixedupdate tick")]
    public float damage = 20f;

    [HideInInspector] public bool positionsSet;

    [HideInInspector] public Vector3 pos;

    [HideInInspector] public Actor tgt;

    [HideInInspector] public Vector3 tgtPosFake;

    [HideInInspector] public Actor myActor;

    [HideInInspector] public bool stopFixedUpdate;

    [HideInInspector] public bool fake;

    private void Update()
    {
        if (!positionsSet) return;

        var position = fake ? tgtPosFake : tgt.position;
        
        lineRenderer.SetPositions(new[]
        {
            pos,
            transform.InverseTransformPoint(position)
        });
    }

    private int _i;

    private void FixedUpdate()
    {
        if (stopFixedUpdate)
            return;
        
        _i++;
        if (!positionsSet || _i <= 5) return;

        if (!tgt.alive)
        {
            stopFixedUpdate = true;
            Destroy(gameObject, 3f);
        }

        _i = 0;
        tgt.health.Damage(damage, pos, Health.DamageTypes.Impact, myActor);
    }
}