using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

public class HPEquipEngine : HPEquippable, IMassObject
{
    public float mass;

    [Header("Engine")] 
    public EngineSpecifications specifications;
    
    public List<GameObject> thrustTransforms = new();

    [Header("Animation")]
    public List<Animator> engineAnimators = new();

    public string stateName;

    public int layer = 0;

    [Header("Effects")] 
    [Tooltip("0 Based")]
    public List<int> particleFxEngine = new();
    public List<EngineEffects.EngineParticleFX> particleFx = new();
    
    [Tooltip("0 Based")]
    public List<GameObject> emissiveMeshParents = new();


    private VehicleMaster _vm;

    private int _vanillaLayer;

    protected List<ModuleEngine> _engines = new();
    private List<Transform> _vanillaExhaustTf = new();
    private List<Transform> _vanillaFxTf = new();
    private List<MeshRenderer> _emissiveRenderers = new();
    private List<List<MeshRenderer>> _vanillaRenderers = new();
    private List<List<EngineEffects.EngineParticleFX>> _vanillaFx = new();
    
    private EngineSpecifications _vanillaSpecifications;

    public override void OnEquip()
    {
        Initialize();
    }

    public override void OnUnequip()
    {
        UnEquip();
    }

    /*public override void OnDestroy()
    {
        UnEquip();
    }*/

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        
        if (configurator.uiOnly)
            return;
        
        Initialize(configurator);
        
        configurator.CalculateTotalThrust();
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        if (configurator.uiOnly)
            return;
        
        UnEquip(configurator);
        
        configurator.CalculateTotalThrust();
    }

    public virtual void Initialize(LoadoutConfigurator configurator = null)
    {
        var wm = configurator ? configurator.wm : weaponManager;

        _vm = wm.vm;

        for (int i = 0; i < _vm.engines.Length; i++)
        {
            var engine = _vm.engines[i];
            if (specifications)
            {
                // Save for unequip
                _vanillaSpecifications = ScriptableObject.CreateInstance<EngineSpecifications>();
                
                engine.specs = _vanillaSpecifications;
                engine.SaveToEngineSpecs();
                engine.specs = specifications;
                engine.LoadFromEngineSpecs();
            }

            _engines.Add(engine);
            
            _vanillaExhaustTf.Add(engine.thrustTransform);
            engine.thrustTransform = thrustTransforms[i].transform;
            
            if (particleFx.Count == 0)
            {
                _vanillaFxTf.Add(engine.engineEffects.transform.parent);
                engine.engineEffects.transform.SetParent(thrustTransforms[i].transform);
                
            }


            // Animator Stuff
            var nozzle = engine.GetComponentInChildren<JetNozzleAnimator>();
            var nozzleTraverse = Traverse.Create(nozzle);
            
            
            nozzleTraverse.Field("animator").SetValue(engineAnimators[i]);
            nozzleTraverse.Field("stateNameHash").SetValue(Animator.StringToHash(stateName));
            
            _vanillaLayer = nozzle.layer;
            nozzle.layer = layer;

            // Effects
            if (emissiveMeshParents[i])
            {
                var renderers = emissiveMeshParents[i].GetComponentsInChildren<MeshRenderer>();
                
                _emissiveRenderers.AddRange(renderers);
                var emissionAnimator = engine.GetComponentInChildren<EngineEmissionAnimator>();
                _vanillaRenderers.Add(emissionAnimator.meshRenderers.ToList());
                emissionAnimator.meshRenderers = renderers.ToArray();

                // I think you need both for it to work
                emissionAnimator.Awake();
                emissionAnimator.Start();
            }

            if (particleFx.Count > i)
            {
                _vanillaFx.Add(engine.engineEffects.particleEffects.ToList());
                foreach (var engineEffectsParticleEffect in engine.engineEffects.particleEffects)
                {
                    engineEffectsParticleEffect.particleSystem.gameObject.SetActive(false);
                }

                var fx = new List<EngineEffects.EngineParticleFX>();
                for (int count = 0; count < particleFxEngine.Count; count++)
                {
                    if (particleFxEngine[count] == i)
                    {
                        fx.Add(particleFx[count]);
                        particleFx[count].particleSystem.gameObject.SetActive(true);
                        
                        particleFx[count].Init();
                        particleFx[count].Evaluate(0f);
                    }
                }
                
                engine.engineEffects.particleEffects = fx.ToArray();
            }
        }
    }

    public virtual void UnEquip(LoadoutConfigurator configurator = null)
    {
        if (_engines == null || _engines.Count == 0)
            return;

        var wm = configurator ? configurator.wm : weaponManager;
        if (!wm || !wm.vm)
            return;
        
        _vm = wm.vm;
        
        for (int i = 0; i < _vm.engines.Length; i++)
        {
            var engine = _vm.engines[i];
            if (specifications)
            {
                engine.specs = _vanillaSpecifications;
                engine.LoadFromEngineSpecs();
            }
            engine.thrustTransform = _vanillaExhaustTf[i];
            
            if (particleFx.Count == 0)
            {
                engine.engineEffects.transform.SetParent(_vanillaFxTf[i]);
            }
            
            // Animator Stuff
            var nozzle = engine.GetComponentInChildren<JetNozzleAnimator>();
            
            nozzle.layer = _vanillaLayer;
            nozzle.Awake();
            
            // Effects
            if (_emissiveRenderers.Count > 0)
            {
                var emissionAnimator = engine.GetComponentInChildren<EngineEmissionAnimator>();

                emissionAnimator.meshRenderers = _vanillaRenderers[i].ToArray();
                
                emissionAnimator.Awake();
                emissionAnimator.Start();
            }

            if (particleFx.Count > i)
            {
                engine.engineEffects.particleEffects = _vanillaFx[i].ToArray();
                foreach (var engineEffectsParticleEffect in engine.engineEffects.particleEffects)
                {
                    engineEffectsParticleEffect.particleSystem.gameObject.SetActive(true);
                }
            }
        }
    }
    
    

    public float GetMass()
    {
        return mass;
    }
}