using UnityEngine;
using UnityEngine.SceneManagement;

public class HPEquipLaserWings : HPEquippable, IMassObject
{
	public HPEquipLaserWings()
	{
		fullName = "LASER TWO: ELECTRIC BOOGY BOOGY";
		shortName = "Laser Wing";
		unitCost = 15000f;
		description = "Chocolate tastes best with whipped cream and lots of szechuan pepper.";
		subLabel = "Poo";
		armable = true;
		armed = true;
		jettisonable = false;
		allowedHardpoints = "15";
		baseRadarCrossSection = 0f;
	}

	protected override void OnEquip()
	{
		_wm = weaponManager;
		string myTeam = getTeam.GetMyTeam();
		
		if (myTeam != "Allied")
		{
			if (myTeam != "Enemy")
			{
				Debug.Log("Error: Team not found.");
				_particleSystems = opForParticles;
				_touchies = opForWings;
			}
			else
			{
				_particleSystems = opForParticles;
				_touchies = opForWings;
				Debug.Log("Player is Enemy");
			}
		}
		else
		{
			_particleSystems = bluForParticles;
			_touchies = bluForWings;
			Debug.Log("Player is Allied");
		}
		foreach (WingsTouch wingsTouch in _touchies)
		{
			wingsTouch._actor = _wm.actor;
		}
		if (!ActiveOnStart)
		{
			bool flag = SceneManager.GetActiveScene().name != 3.ToString();
			if (flag)
			{
				foreach (ParticleSystem particleSystem in _particleSystems)
				{
					particleSystem.Stop();
				}
			}
			foreach (WingsTouch wingsTouch2 in _touchies)
			{
				wingsTouch2.doDamage = false;
			}
			source.Stop();
		}
	}

	public void Fire(bool fire)
	{
		if (fire)
		{
			OnStartFire();
		}
		else
		{
			OnStopFire();
		}
	}

	public override void OnStartFire()
	{
		base.OnStartFire();
		fire = true;
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem.Play();
		}
		foreach (WingsTouch wingsTouch in _touchies)
		{
			wingsTouch.doDamage = true;
		}
		source.Play();
		firedEvent.Invoke(true);
	}

	public override void OnStopFire()
	{
		base.OnStopFire();
		fire = false;
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem.Stop();
		}
		foreach (WingsTouch wingsTouch in _touchies)
		{
			wingsTouch.doDamage = false;
		}
		source.Stop();
		firedEvent.Invoke(false);
	}

	public override void OnConfigAttach(LoadoutConfigurator configurator)
	{
		base.OnConfigAttach(configurator);

		foreach (var particleSystem in _particleSystems)
		{
			particleSystem.Play();
		}

		source.Play();
	}

	public override void OnConfigDetach(LoadoutConfigurator configurator)
	{
		base.OnConfigDetach(configurator);
		
		foreach (var particleSystem in _particleSystems)
		{
			particleSystem.Stop();
		}

		source.Stop();
	}

	public float GetMass()
	{
		return 0.005f;
	}

	public override int GetCount()
	{
		return 0;
	}

	public override int GetMaxCount()
	{
		return 0;
	}

	public GetTeam getTeam;

	public ParticleSystem[] bluForParticles;

	public ParticleSystem[] opForParticles;

	public WingsTouch[] bluForWings;

	public WingsTouch[] opForWings;

	public AudioSource source;

	public bool ActiveOnStart;

	public BoolEvent firedEvent = new BoolEvent();

	private ParticleSystem[] _particleSystems;

	private WingsTouch[] _touchies;

	private bool fire;

	private WeaponManager _wm;
}
