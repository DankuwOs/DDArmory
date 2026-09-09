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
            _light = GetComponent<Light>();
            return _light;

        }
    }

    private void Awake()
    {
        if (OverCloud.instance == null)
            enabled = false;

        m_Material = new Material(Shader.Find("OverCloud/FogLight"));
        m_Material.renderQueue = 3000;
        m_Material.SetFloat("_Atten", 1);
    }
}