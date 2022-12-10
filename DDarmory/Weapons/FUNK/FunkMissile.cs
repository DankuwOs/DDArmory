using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
#pragma warning disable CS4014

public class FunkMissile : MonoBehaviour
{
    public BurstMissile burstMissile;
    
    public float delay;

    public float radius = 8000f;

    public float fov = 45;

    public MeshFilter baller;

    public GameObject lineRendererObj;
    
    public UnityEvent OnFunk = new UnityEvent();

    private Mesh _ballerMesh;

    private List<Tuple<Vector3, Vector3>> _vertices = new List<Tuple<Vector3, Vector3>>();

    private Actor _actor;

    public void Awake()
    {
        _ballerMesh = baller.mesh;

        for (int i = 0; i < _ballerMesh.vertexCount; i++)
        {
            Vector3 vertexPos = _ballerMesh.vertices[i];
            Vector3 vertexNorm =_ballerMesh.normals[i];
            
            _vertices.Add<Tuple<Vector3, Vector3>>(new Tuple<Vector3, Vector3>(vertexPos, vertexNorm));
        }
    }

    public void FUNKYTOWN()
    {
        _actor = burstMissile.actor;
        Funk();
    }
    
    private async Task Funk()
    {
        await Task.Delay(TimeSpan.FromSeconds(delay));
        var targetedActors = new List<Actor>();
        
        var tgts = new List<Actor>();
        
        
        
        Actor.GetActorsInRadius(transform.position, radius, burstMissile.actor.team, TeamOptions.OtherTeam, tgts);
        
        foreach (var (pos, normal) in _vertices)
        {
            Actor tgtActor = null;

            bool foundTarget = false;
            
            foreach (var actor in tgts)
            {
                var dir = (actor.position - pos).normalized;
                var angle = Vector3.Angle(normal, dir.normalized);

                if (angle > fov / 2 || targetedActors.Contains(actor))
                {
                    continue;
                }
                
                tgtActor = actor;
                foundTarget = true;
                break;
            }

            if (!foundTarget)
            {
                continue;
            }

            targetedActors.Add(tgtActor);
            

            var laserGameObject = Instantiate(lineRendererObj, baller.transform, worldPositionStays: true);
            laserGameObject.SetActive(true); // is enable??!??

            var funkyLines = laserGameObject.GetComponent<FunkyLines>();
            var lineRenderer = funkyLines.lineRenderer;

            funkyLines.pos = pos;

            funkyLines.tgt = tgtActor;
            funkyLines.myActor = _actor;

            var randomColor = Random.ColorHSV(0, 1, 0.7f, 1);
            

            var gradient = new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(randomColor, 0),
                    new GradientColorKey(randomColor, 1)
                }
            };

            lineRenderer.colorGradient = gradient;
            funkyLines.positionsSet = true;
            
            
            if (targetedActors.Count == tgts.Count)
                break;
            
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }

        OnFunk.Invoke();
    }
}