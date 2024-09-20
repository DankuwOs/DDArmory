using UnityEngine;

namespace DDArmory.Weapons.EMP;

public class EMPJammer : RadarJammer
{
    [Header("Debug")] public Vector3 transmitDirection;

    public Vector3 recieverPos;

    public float origDot;

    public float fixedDot;

    private void OnDrawGizmosSelected()
    {
        var dir = transmitDirection;
        var recPos = transform.TransformPoint(recieverPos);

        origDot = Vector3.Dot(recPos - transform.position, dir);

        var rot = Quaternion.FromToRotation(recPos - transform.position, dir);

        var fixedRecPos = transform.position + rot * (recPos - transform.position);
        
        fixedDot = Vector3.Dot(fixedRecPos - transform.position, dir);
        

        Gizmos.matrix = transform.localToWorldMatrix;
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Vector3.zero, transform.InverseTransformPoint(recPos));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero, dir);
    }
}