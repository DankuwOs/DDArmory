using OC;
using UnityEngine;

namespace DDArmory.Weapons.Utils;

public class FogLightFixed : OverCloudFogLight
{
    public override Light light
    {
        get
        {
            if (_light != null) return _light;
            Debug.Log($"light null :~(");
            _light = GetComponent<Light>();
            Debug.Log($"light {_light != null} :~(?");
            return _light;

        }
    }

    private void Awake()
    {
        if (OverCloud.instance == null)
            enabled = false;

        m_Material = new Material(Shader.Find("OverCloud/FogLight"));
        m_Material.SetFloat("_Atten", 1);
    }
}