using UnityEngine;

namespace DDArmory.Weapons.Utils
{
    public class PIDFixer : MonoBehaviour
    {
        public FlightAssist assist;

        public void SetPitchAngVel(float value) => assist.pitchAngVel = value;
        public void SetYawAngVel(float value) => assist.yawAngVel = value;
        public void SetRollAngVel(float value) => assist.rollAngVel = value;
        
        public void SetPitchPIDP(float value) => assist.pitchPID.kp = value;
        public void SetPitchPIDI(float value) => assist.pitchPID.ki = value;
        public void SetPitchPIDD(float value) => assist.pitchPID.kd = value;
        public void SetPitchPIDIMin(float value) => assist.pitchPID.iMin = value;
        public void SetPitchPIDIMax(float value) => assist.pitchPID.iMax = value;
        
        public void SetYawPIDP(float value) => assist.yawPID.kp = value;
        public void SetYawPIDI(float value) => assist.yawPID.ki = value;
        public void SetYawPIDD(float value) => assist.yawPID.kd = value;
        public void SetYawPIDIMin(float value) => assist.yawPID.iMin = value;
        public void SetYawPIDIMax(float value) => assist.yawPID.iMax = value;
        
        public void SetRollPIDP(float value) => assist.rollPID.kp = value;
        public void SetRollPIDI(float value) => assist.rollPID.ki = value;
        public void SetRollPIDD(float value) => assist.rollPID.kd = value;
        public void SetRollPIDIMin(float value) => assist.rollPID.iMin = value;
        public void SetRollPIDIMax(float value) => assist.rollPID.iMax = value;
    }
}