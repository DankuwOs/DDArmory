using System.IO;
using System.Text;
using UnityEngine;
using Valve.Newtonsoft.Json;

[ExecuteAlways]
public class DebugWingUtils : MonoBehaviour
{
    public Wing[] leftWings;

    public Wing[] rightWings;

    private Wing _wing;

    private Wing _symWing;

    private bool _selectedWing;

    public float YDist = 30f;
    
    private Vector2 TextOffset = new Vector2(20, 0 * 0.8f);

    private Vector2 XOffset = new Vector2(20, 0);

    public Vector2 Size = new Vector2(130, 40);

    public Vector2 BoxSize = new Vector2(150, 600);

    public Vector2 BoxOffset = new Vector2(10f, 10f);

    private void Awake()
    {
        if (leftWings.Length < 1)
            leftWings = GetComponentsInChildren<Wing>(true);
    }

    private void OnGUI()
    {
        var y = YDist;
        var size = _selectedWing ? BoxSize : new Vector2(BoxSize.x, YDist * (leftWings.Length + 1 ) * 2);
        GUI.Box(new Rect(BoxOffset, size), "Wing Test");
        y += YDist * 2;
        if (_selectedWing)
        {
            if (GUI.Button(new Rect(new Vector2(XOffset.x, y), Size), "Back"))
            {
                _wing = null;
                _selectedWing = false;
                return;
            }

            y += YDist;
            
            // usePointVelocity
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Use Point Velocity");
            y += YDist * 0.5f;
            _wing.usePointVelocity = GUI.Toggle(new Rect(new Vector2(XOffset.x, y), Size), _wing.usePointVelocity,
                GUIContent.none);
            _symWing.usePointVelocity = _wing.usePointVelocity;
            y += YDist;
            // liftCoefficient
            
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Lift Coefficient");
            y += YDist * 0.5f;
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), _wing.liftCoefficient.ToString());
            y += YDist * 0.8f;
            _wing.liftCoefficient = GUI.HorizontalSlider(new Rect(new Vector2(XOffset.x, y), Size), _wing.liftCoefficient, 0f, 0.8f);
            _symWing.liftCoefficient = _wing.liftCoefficient;
            y += YDist;
            // dragCoefficient
            
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Drag Coefficient");
            y += YDist * 0.5f;
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), _wing.dragCoefficient.ToString());
            y += YDist * 0.8f;
            _wing.dragCoefficient = GUI.HorizontalSlider(new Rect(new Vector2(XOffset.x, y), Size), _wing.dragCoefficient, 0f, 0.8f);
            _symWing.dragCoefficient = _wing.dragCoefficient;
            y += YDist;
            // liftArea
            
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Lift Area");
            y += YDist * 0.5f;
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), _wing.liftArea.ToString());
            y += YDist * 0.8f;
            _wing.liftArea = GUI.HorizontalSlider(new Rect(new Vector2(XOffset.x, y), Size), _wing.liftArea, 0f, 20f);
            _symWing.liftArea = _wing.liftArea;
            y += YDist;
            if (_wing.aeroProfile)
            // aeroProfile
            {
                // buffetMagnitude
            
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Buffet Magnitude");
                y += YDist * 0.5f;
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), _wing.aeroProfile.buffetMagnitude.ToString());
                y += YDist * 0.8f;
                _wing.aeroProfile.buffetMagnitude = GUI.HorizontalSlider(new Rect(new Vector2(XOffset.x, y), Size), _wing.aeroProfile.buffetMagnitude, 0f, 20f);
                _symWing.aeroProfile.buffetMagnitude = _wing.aeroProfile.buffetMagnitude;
                y += YDist;
                // buffetTimeFactor
            
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Buffet Magnitude");
                y += YDist * 0.5f;
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), _wing.aeroProfile.buffetTimeFactor.ToString());
                y += YDist * 0.8f;
                _wing.aeroProfile.buffetTimeFactor = GUI.HorizontalSlider(new Rect(new Vector2(XOffset.x, y), Size), _wing.aeroProfile.buffetTimeFactor, 0f, 20f);
                _symWing.aeroProfile.buffetTimeFactor = _wing.aeroProfile.buffetTimeFactor;
                y += YDist;
                
                // perpLiftVector
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Perpendicular Lift Vector");
                y += YDist * 0.8f;
                _wing.aeroProfile.perpLiftVector = GUI.Toggle(new Rect(new Vector2(XOffset.x, y), Size), _wing.aeroProfile.perpLiftVector,
                    GUIContent.none);
                _symWing.aeroProfile.perpLiftVector = _wing.aeroProfile.perpLiftVector;
                
                y += YDist;
                // mirroredCurves
                GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Mirrored Curves");
                y += YDist * 0.8f;
                _wing.aeroProfile.mirroredCurves = GUI.Toggle(new Rect(new Vector2(XOffset.x, y), Size), _wing.aeroProfile.mirroredCurves,
                    GUIContent.none);
                _symWing.aeroProfile.mirroredCurves = _wing.aeroProfile.mirroredCurves;
                
                y += YDist;
            }
            // useBuffet
            GUI.Label(new Rect(new Vector2(TextOffset.x, y), Size), "Use Buffet");
            y += YDist * 0.8f;
            _wing.useBuffet = GUI.Toggle(new Rect(new Vector2(XOffset.x, y), Size), _wing.useBuffet,
                GUIContent.none);
            _symWing.useBuffet = _wing.useBuffet;
                
            y += YDist;
            
            
        }
        else
        {
            for (var index = 0; index < leftWings.Length; index++)
            {
                var wing = leftWings[index];
                
                if (GUI.Button(new Rect(new Vector2(XOffset.x, y), Size), wing.gameObject.name))
                {
                    _wing = wing;
                    _symWing = rightWings[index];
                    _selectedWing = true;
                }

                y += YDist;
            }

            y += YDist * 3;
            if (GUI.Button(new Rect(new Vector2(XOffset.x, y), Size), "Save Wings"))
            {
                SaveWingSettings();
            }
        }
    }


    private void SaveWingSettings()
    {
        StringBuilder builder = new StringBuilder();
        StringWriter stringWriter = new StringWriter(builder);
        
        using (JsonWriter writer = new JsonTextWriter(stringWriter))
        {
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            writer.WritePropertyName("Wings");
            writer.WriteStartArray();
            foreach (var wing in leftWings)
            {
                writer.WriteStartObject();
                
                writer.WritePropertyName(wing.name);
                {
                    writer.WriteStartArray();
                    writer.WriteStartObject();
                    {
                        // pointVel
                        writer.WritePropertyName("usePointVelocity");
                        writer.WriteValue(wing.usePointVelocity);

                        // liftCoeff
                        writer.WritePropertyName("liftCoefficient");
                        writer.WriteValue(wing.liftCoefficient);

                        // dragCoeff
                        writer.WritePropertyName("dragCoefficient");
                        writer.WriteValue(wing.dragCoefficient);

                        // liftArea
                        writer.WritePropertyName("liftArea");
                        writer.WriteValue(wing.liftArea);

                        // aeroProfile
                        var aeroProfile = wing.aeroProfile;
                        if (aeroProfile)
                        {
                            writer.WritePropertyName("aeroProfile");

                            writer.WriteStartArray();
                            {
                                writer.WriteStartObject();
                                // buffetMagnitute
                                writer.WritePropertyName("buffetMagnitude");
                                writer.WriteValue(aeroProfile.buffetMagnitude);

                                // buffetTimeFactor
                                writer.WritePropertyName("buffetTimeFactor");
                                writer.WriteValue(aeroProfile.buffetTimeFactor);

                                // perpLiftVector
                                writer.WritePropertyName("perpLiftVector");
                                writer.WriteValue(aeroProfile.perpLiftVector);

                                // mirroredCurves
                                writer.WritePropertyName("mirroredCurves");
                                writer.WriteValue(aeroProfile.mirroredCurves);
                                writer.WriteEndObject();
                            }
                            writer.WriteEndArray();
                        }
                        // useBuffet
                        writer.WritePropertyName("useBuffet");
                        writer.WriteValue(wing.useBuffet);
                    }
                    writer.WriteEndObject();
                    writer.WriteEndArray();
                }
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        File.WriteAllText(@$"D:\Games\Steam [Software no games here]\steamapps\common\VTOL VR\VTOLVR_ModLoader\projects\My Mods\DDArmory\Builds\Wings.json", stringWriter.ToString());
    }
}