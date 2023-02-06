using UnityEngine;
using UnityEngine.Serialization;

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
    
    [FormerlySerializedAs("curve")] 
    public AnimationCurve liftAreaCurve;

    public AnimationCurve dragCoefficientCurve;

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
        
        wing.dragCoefficient = dragCoefficientCurve.Evaluate(sweep);
        wing.SetLiftArea(liftAreaCurve.Evaluate(sweep));
    }
}