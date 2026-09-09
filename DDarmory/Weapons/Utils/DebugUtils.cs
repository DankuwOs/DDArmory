using UnityEngine;

namespace DebugUtils;

public class DebugUtils
{
    private static Material _lineRendererMaterial;
    
    public static Material LineRendererMaterial
    {
        get
        {
            if (_lineRendererMaterial == null)
            {
                _lineRendererMaterial = new Material(Shader.Find("MF-Standard (Specular setup)"));
            }
            return _lineRendererMaterial;
        }
    }
    
    public static DebugObject CreateDebugObject(float lifetime = -1)
    {
        var obj = new GameObject();
        
        obj.AddComponent<FloatingOriginTransform>();
        
        var debugObject = obj.AddComponent<DebugObject>();
        if (lifetime > 0)
            Object.Destroy(obj, lifetime);
        return debugObject;
    }

    public static DebugObject CreateDebugObject(Vector3 position, float lifetime = -1)
    {
        var obj = new GameObject();
        
        obj.AddComponent<FloatingOriginTransform>();
        
        var debugObject = obj.AddComponent<DebugObject>();
        debugObject.transform.position = position;
        if (lifetime > 0)
            Object.Destroy(obj, lifetime);
        return debugObject;
    }

    public static DebugObject CreateDebugObject(Transform parent, float lifetime = -1)
    {
        var obj = new GameObject();
        
        obj.AddComponent<FloatingOriginTransform>();
        
        var debugObject = obj.AddComponent<DebugObject>();
        debugObject.transform.parent = parent;
        if (lifetime > 0)
            Object.Destroy(obj, lifetime);
        return debugObject;
    }

    public static DebugObject.PossiblyMortalText CreateTextObject(Vector3 position, string text, float lifetime = -1)
    {
        var debugObject = CreateDebugObject(position, lifetime);
        var possiblyMortalText = debugObject.AddText(text);
        return possiblyMortalText;
    }

    public static DebugObject.PossiblyMortalText CreateTextObject(Vector3 position, string text, Color color, float lifetime = -1)
    {
        var debugObject = CreateDebugObject(position, lifetime);
        var possiblyMortalText = debugObject.AddText(text, color);
        return possiblyMortalText;
    }

    public static DebugObject.PossiblyMortalText CreateTextObject(Transform parent, string text, float lifetime = -1)
    {
        var debugObject = CreateDebugObject(parent, lifetime);
        var possiblyMortalText = debugObject.AddText(text);
        return possiblyMortalText;
    }

    public static DebugObject.PossiblyMortalText CreateTextObject(Transform parent, string text, Color color, float lifetime = -1)
    {
        var debugObject = CreateDebugObject(parent, lifetime);
        var possiblyMortalText = debugObject.AddText(text, color);
        return possiblyMortalText;
    }

    public static LineRenderer CreateLineRenderer(Vector3 startPos, Vector3 endPos, Transform parent = null, bool worldSpace = true, float lifetime = -1)
    {
        DebugObject debugObject = parent ? CreateDebugObject(parent, lifetime) : CreateDebugObject(Vector3.zero, lifetime);
        var possiblyMortalText = debugObject.AddLine(startPos, endPos, worldSpace);
        return possiblyMortalText;
    }

    public static LineRenderer CreateLineRenderer(Vector3 startPos, Vector3 endPos, Color color, Transform parent = null, bool worldSpace = true, float lifetime = -1)
    {
        DebugObject debugObject = parent ? CreateDebugObject(parent, lifetime) : CreateDebugObject(Vector3.zero, lifetime);
        var possiblyMortalText = debugObject.AddLine(startPos, endPos, color, worldSpace);
        return possiblyMortalText;
    }
}