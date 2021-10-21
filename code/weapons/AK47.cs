using Sandbox;
using System;

[Library( "ak47", Title = "AK-47" )]
[Hammer.EditorModel( "models/weapons/ak47/ak47.vmdl" )]
partial class AK47 : Weapon
{
	public override string ViewModelPath => "models/weapons/ak47/v_ak47.vmdl";

	public override float PrimaryRate => 9.0f;
	public override float SecondaryRate => 1.0f;
	public override int ClipSize => 30;
	public override float ReloadTime => 2.0f;
	public override int Bucket => 2;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/weapons/ak47/ak47.vmdl" );
		AmmoClip = 30;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = .5f;

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
		PlaySound( "ak47.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.02f, 1.5f, 1.0f, 3.0f );

	}

	public override void AttackSecondary()
	{
		//ShootEffects();
		PlaySound( "rust_boneknife.attack" );
		MeleeStrike( 10, 1.5f );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin();
		}

		(Owner as AnimEntity).SetAnimBool( "b_jump", true );
		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
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
