using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FunkMissile : MonoBehaviour
{
    public Missile burstMissile;

    public float delay;

    public float radius = 8000f;

    public float fov = 45;

    public MeshFilter baller;

    public List<MissileFairing> fairings;

    public GameObject lineRendererObj;

    public UnityEvent OnFunk = new();

    private Mesh _ballerMesh;

    private List<Tuple<Vector3, Vector3>> _vertices = new();

    private Actor _actor;

    public void Awake()
    {
        _ballerMesh = baller.mesh;

        for (var i = 0; i < _ballerMesh.vertexCount; i++)
        {
            var vertexPos = _ballerMesh.vertices[i];
            var vertexNorm = _ballerMesh.normals[i];

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
        for (int i = 0; i < fairings.Count; i++)
        {
            fairings[i].Jettison();
            await Task.Delay(50);
        }
        
        await Task.Delay(TimeSpan.FromSeconds(delay));
        
        var targetedActors = new List<Actor>();

        var tgts = new List<Actor>();


        Actor.GetActorsInRadius(transform.position, radius, burstMissile.actor.team, TeamOptions.OtherTeam, tgts);

        foreach (var (pos, normal) in _vertices)
        {
            Actor tgtActor = null;

            var foundTarget = false;

            foreach (var actor in tgts)
            {
                var dir = (actor.position - pos).normalized;
                var angle = Vector3.Angle(normal, dir.normalized);

                if (angle > fov / 2 || targetedActors.Contains(actor)) continue;

                tgtActor = actor;
                foundTarget = true;
                break;
            }

            if (!foundTarget)
            {
                FakeLine(pos, normal);
                continue;
            }

            targetedActors.Add(tgtActor);


            var laserGameObject = Instantiate(lineRendererObj, baller.transform, true);
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

    public void FakeLine(Vector3 pos, Vector3 normal)
    {
        if (Random.Range(0,15) != 7)
            return;
        
        var laserGameObject = Instantiate(lineRendererObj, baller.transform, true);
        laserGameObject.SetActive(true);
        
        var funkyLines = laserGameObject.GetComponent<FunkyLines>();
        
        // very hastily booled up
        funkyLines.stopFixedUpdate = true;
        funkyLines.fake = true;
        
        var lineRenderer = funkyLines.lineRenderer;
        
        lineRenderer.SetPositions(new[]
        {
            pos,
            transform.InverseTransformPoint(normal * Random.Range(300,2000))
        });

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
        
        Destroy(laserGameObject, Random.Range(2f, 10f));
    }
}