using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace DDArmory.Weapons.SatelliteGun;

public class NeverCulledObject : MonoBehaviour
{
    private Mesh _mesh;

    private void Awake()
    {
        var mFilter = GetComponent<MeshFilter>();
        _mesh = mFilter.mesh;
        Camera.onPreCull += PreCull;
    }

    private void OnDestroy()
    {
        Camera.onPreCull -= PreCull;
    }

    private void PreCull(Camera cam)
    {
        if (_mesh == null || !gameObject.activeInHierarchy)
            return;

        var bounds = _mesh.bounds;
        float boundsDistance = (cam.farClipPlane - cam.nearClipPlane) / 2 + cam.nearClipPlane;
        Vector3 newCenter = transform.InverseTransformPoint(cam.transform.position + (cam.transform.forward * boundsDistance));
        bounds.center = newCenter;
        bounds.size = Vector3.one * 1000f;

        _mesh.bounds = bounds;
    }
}