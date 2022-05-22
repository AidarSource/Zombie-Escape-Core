using Sandbox;
using System;

[Library( "dm_rifle", Title = "AK74" )]
[EditorModel( "weapons/ak74/w_ak74.vmdl" )]
partial class AK74 : Weapon
{
	public override string ViewModelPath => "weapons/ak74/v_ak74.vmdl";

	//public override AmmoType AmmoType => AmmoType.Rifle;
	public override float PrimaryRate => 10.0f;
	public override int ClipSize => 30;
	public override float ReloadTime => 1.4f;
	public override int Bucket => 3;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/ak74/w_ak74.vmdl" );
		AmmoClip = 30;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if(AvailableAmmo() > 0)
			{
				Reload();
			}

			return;
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "ak74.shot" );

		//
		// Shoot the bullets
		//
		Rand.SetSeed( Time.Tick );
		ShootBullet( 0.02f, 1.5f, 5.0f, 3.0f );

	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			// Need to implement ScreenShake method
			//new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 0.5f, 0.5f );
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 2 ); // TODO this is shit
		anim.SetAnimParameter( "aimat_weight", 1.0f );
	}

}
