using UnityEngine;

public class BiggingMissile : BurstMissile
{
    public override void BPU_FixedUpdate()
    {
        base.BPU_FixedUpdate();

        if (fired && !detonated)
        {
            transform.localScale += Vector3.one * biggingFrame;
            explodeRadius += biggingFrame;
            explodeDamage += biggingFrame;
        }
    }

    [Header("Bigging")] public float biggingFrame;
}