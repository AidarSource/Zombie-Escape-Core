using Sandbox;
using Sandbox.UI;
using System;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual AmmoType AmmoType => AmmoType.Pistol;
	[Net, Predicted]
	public int AmmoClip { get; set; }
	public virtual float ReloadTime => 3.0f;
	public virtual int ClipSize => 12;
	private float ZombieKnockback = 300.0f;
	private float ZombieOnAirKnockback = 5.0f;
	public virtual int Bucket => 1;
	public virtual int BucketWeight => 10;
	public virtual int AmmoMax => -1;
	public int AmmoCount { get; set; } = 10;
	public bool IsAiming { get; set; }
	public virtual Vector3 AimPosition => new( 10, 10, 10 );
	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	public int AvailableAmmo()
	{
		var owner = Owner as ZePlayer;
		if ( owner == null ) return 0;
		//return owner.AmmoCount( AmmoType );
		return AmmoCount;
	}

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.EnableAutoSleeping = false;

		AmmoCount = AmmoMax;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;

		IsReloading = false;
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;

		if ( AmmoClip >= ClipSize )
			return;

		TimeSinceReload = 0;

		if ( Owner is ZePlayer player )
		{
			//if ( player.AmmoCount( AmmoType ) <= 0 )
			//	return;
			if ( AmmoCount <= 0 && AmmoMax != -1 )
				return;
		}

		IsReloading = true;

		(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );

		StartReloadEffects();
	}

	public override void AttackSecondary()
	{
		IsAiming = !IsAiming;
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;

		if(AmmoMax == -1)
		{
			AmmoClip = ClipSize;
		}
		else
		{
			var amount = Math.Min( AmmoCount, ClipSize - AmmoClip );
			AmmoCount -= amount;

			AmmoClip += amount;
		}

		//if( Owner is ZePlayer player )
		//{
		//	var ammo = player.TakeAmmo( AmmoType, ClipSize - AmmoClip );
		//	if ( ammo == 0 )
		//		return;

		//	AmmoClip += ammo;
		//}
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );
	}

	public bool TakeAmmo( int amount )
	{
		if ( AmmoClip < amount )
			return false;

		AmmoClip -= amount;
		return true;
	}

	[ClientRpc]
	public virtual void DryFire()
	{
		// CLICK
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		//if ( Owner != null ) return false;

		//if (user.Inventory is Inventory inventory )
		//{
		//	return inventory.CanAdd( this );
		//}

		//return true;

		if ( AmmoClip > 0 ) return true;
		if ( AmmoMax == -1 ) return true;
		return AvailableAmmo() > 0;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if (IsLocalPawn)
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;


		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}

			// temporary knock back
			if ( tr.Entity.GetType() == typeof( ZePlayer ) )
			{
				if ( tr.Entity.GroundEntity == null )
				{
					Log.Info( GroundEntity + "2" );
					tr.Entity.Velocity = forward * (100 * ZombieOnAirKnockback);
					DebugOverlay.ScreenText( ZombieOnAirKnockback.ToString() );
					//tr.Entity.Health = 70000;
					return;
				}

				DebugOverlay.ScreenText( 2, ZombieKnockback.ToString() );
				tr.Entity.Velocity = forward * ZombieKnockback;
				//tr.Entity.Health = 700000;
			}

			//DebugOverlay.ScreenText( tr.Entity.GetType(), 1.0f );
			Log.Info( tr.Entity.GetType() );
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		ShootBullet( Owner.EyePos, Owner.EyeRot.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		var pos = Owner.EyePos;
		var dir = Owner.EyeRot.Forward;

		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( pos, dir, spread, force / numBullets, damage, bulletSize );
		}
	}

	public virtual void MeleeStrike( float damage, float force )
	{
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;
		var MeleeDistance = 80;
		foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * MeleeDistance, 10f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;
			tr.Surface.DoBulletImpact( tr );
			if ( !IsServer ) continue;
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 110 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );
				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}
}
