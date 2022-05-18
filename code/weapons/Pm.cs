using Sandbox;
using System;

[Library( "dm_rifle", Title = "Pistol" )]
[Hammer.EditorModel( "weapons/ak74/w_ak74.vmdl" )]
partial class PM : Weapon
{
	public override string ViewModelPath => "weapons/pm/v_pm.vmdl";

	//public override AmmoType AmmoType => AmmoType.Rifle;
	public override float PrimaryRate => 10.0f;
	public override int ClipSize => 12;
	public override float ReloadTime => 2.0f;
	public override int Bucket => 3;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/pm/w_pm.vmdl" );
		AmmoClip = 12;
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

		(Owner as AnimEntity).SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "pm_shoot" );

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

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

}
