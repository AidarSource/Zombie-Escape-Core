using Sandbox;
using System;

[Library( "dm_rifle", Title = "AK74" )]
[Hammer.EditorModel( "weapons/ak74/w_ak74.vmdl" )]
partial class AK74 : Weapon
{
	public override string ViewModelPath => "weapons/ak74/v_ak74.vmdl";

	//public override AmmoType AmmoType => AmmoType.Rifle;
	public override float PrimaryRate => 10.0f;
	public override int ClipSize => 30;
	public override float ReloadTime => 2.0f;
	public override int Bucket => 3;
	//public override float Spread => 0.1f;
	//public override bool CanRicochet => true;
	public override Vector3 AimPosition => new( 11.4f, 11.5f, -25 );

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
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

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
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 0.5f, 0.5f );
		}

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}

}
