using UnityEngine;

public class SweptWing : MonoBehaviour
{
    public enum SurfaceType
    {
        Pitch,
        Yaw,
        Roll,
        Flaps,
        Brakes,
        AoA
    }
    
    public Wing wing;
    
    public AnimationCurve curve;

    public Transform surface;

    public SurfaceType type;

    public float maxSweep = 0.05f;

    public float defaultValue;
    
    private void Start()
    {
        if (!wing)
            wing = GetComponent<Wing>();
    }

    public void SetWingArea(float sweep)
    {
        if (!wing)
            wing = GetComponent<Wing>();
        
        wing.SetLiftArea(curve.Evaluate(sweep));
    }
}