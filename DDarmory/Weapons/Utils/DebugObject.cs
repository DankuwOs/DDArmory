using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUtils;

public class DebugObject : MonoBehaviour
{
    public class PossiblyMortalText
    {
        
        public string text;
        public float theoreticalTimeLeft;
        public bool trulyMortal;

        public PossiblyMortalText(string text, Color color, float theoreticalTimeLeft)
        {
            this.text = text;
            this.theoreticalTimeLeft = theoreticalTimeLeft;
        }
    }
    
    public readonly List<PossiblyMortalText> texts = new();
    
    public PossiblyMortalText AddText(string text, float lifetime = -1)
    {
        return AddText(text, Color.white, lifetime);
    }
    
    public PossiblyMortalText AddText(string text, Color color, float lifetime = -1)
    {
        var mortalText = new PossiblyMortalText(text: text, color: color, theoreticalTimeLeft: lifetime);
        mortalText.trulyMortal = lifetime > 0;
        texts.Add(mortalText);
        return mortalText;
    }

    public LineRenderer AddLine(Vector3 startPosition, Vector3 endPosition, bool worldSpace = true, float lifetime = -1)
    {
        return AddLine(startPosition, endPosition, Color.white, worldSpace, lifetime);
    }

    public LineRenderer AddLine(Vector3 startPosition, Vector3 endPosition, Color color, bool worldSpace = true, float lifetime = -1)
    {
        var lineRendererObj = new GameObject();
        lineRendererObj.transform.SetParent(transform, false);
        var lineRendererComponent = lineRendererObj.AddComponent<LineRenderer>();
        

        lineRendererComponent.material = DebugUtils.LineRendererMaterial;
        lineRendererComponent.material.color = color;

        lineRendererComponent.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        
        lineRendererComponent.startColor = Color.white;
        lineRendererComponent.endColor = Color.white;
        
        lineRendererComponent.startWidth = 0.5f;
        lineRendererComponent.endWidth = 0.5f;
        
        lineRendererComponent.positionCount = 2;
        lineRendererComponent.SetPosition(0, startPosition);
        lineRendererComponent.SetPosition(1, endPosition);

        // always use world space IDIOT (unless specified otherwisey
        lineRendererComponent.useWorldSpace = worldSpace;
        
        if (lifetime > 0)
            Destroy(lineRendererObj, lifetime);
        
        return lineRendererComponent;
    }

    private void OnGUI()
    {
        if (!Camera.current) return;
        var screenPos = Camera.current.WorldToScreenPoint(transform.position);
        screenPos.y = Screen.height - screenPos.y;
        
        var rect = new Rect(screenPos.x, screenPos.y, 300, 300);
        
        GUILayout.BeginArea(rect);
        GUILayout.BeginVertical();
        GUILayout.Label("PISS");
        
        // Make sure the texts are always stacked directly on top of each other.
        for (int i = texts.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(texts[i].text);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void Update()
    {
        foreach (var possiblyMortalText in texts.ToArray())
        {
            if (possiblyMortalText.trulyMortal && (possiblyMortalText.theoreticalTimeLeft -= Time.deltaTime) < 0)
                texts.Remove(possiblyMortalText);
        }
    }
}