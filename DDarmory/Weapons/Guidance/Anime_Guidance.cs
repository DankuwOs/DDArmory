using UnityEngine;

public class Anime_Guidance : MissileGuidanceUnit
{
    protected override void OnBeginGuidance()
    {
        base.OnBeginGuidance();
        _startTime = Time.time;
        _distance = Vector3.Distance(transform.position, missile.estTargetPos);
        _startDistance = _distance / 5f;
    }

    public override Vector3 GetGuidedPoint()
    {
        var vector = Missile.GetLeadPoint(missile.estTargetPos, missile.estTargetVel, transform.position,
            missile.rb.velocity, missile.maxLeadTime);
        var num = Vector3.Distance(transform.position, vector);
        var vector2 = transform.right * (_startDistance * swirlyCurve.Evaluate(num / _distance));
        vector2 = Quaternion.AngleAxis(swirlySpeed * Time.time - _startTime, transform.forward) * vector2;
        vector += vector2;
        Debug.Log(vector);
        return vector;
    }

    public AnimationCurve swirlyCurve;

    public float swirlySpeed;

    private float _startTime;

    private float _startDistance;

    private float _distance;
}