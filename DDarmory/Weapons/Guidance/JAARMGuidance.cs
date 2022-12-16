using UnityEngine;

// Token: 0x0200000C RID: 12
public class JAARMGuidance : MissileGuidanceUnit
{
    // Token: 0x0600003D RID: 61 RVA: 0x00003330 File Offset: 0x00001530
    public override Vector3 GetGuidedPoint()
    {
        var vector = gpsPoint.worldPosition + Vector3.up * height;
        var normalized = (transform.position - vector).normalized;
        var b = normalized * loiterRadius;
        var a = Vector3.Cross(Vector3.ProjectOnPlane(normalized, Vector3.up), Vector3.up);
        var b2 = a * simDistance;
        var vector2 = vector + b2 + b;
        vector2.y = gpsPoint.worldPosition.y + height;
        var flag = debugPoint;
        if (flag)
        {
            _renderer.gameObject.SetActive(true);
            _renderer2.gameObject.SetActive(true);
            _renderer = DebugLinesManager.LineRendererToPoint(transform.position, vector2, _renderer);
            _renderer2 = DebugLinesManager.LineRendererToPoint(vector2, gpsPoint.worldPosition, _renderer2);
        }

        return vector2;
    }

    // Token: 0x04000045 RID: 69
    public GPSTarget gpsPoint;

    // Token: 0x04000046 RID: 70
    public float loiterRadius = 8000f;

    // Token: 0x04000047 RID: 71
    public float height = 3000f;

    // Token: 0x04000048 RID: 72
    public float simDistance = 10f;

    // Token: 0x04000049 RID: 73
    [Header("Debug")] public bool debugPoint;

    // Token: 0x0400004A RID: 74
    public LineRenderer _renderer;

    // Token: 0x0400004B RID: 75
    public LineRenderer _renderer2;
}