using Sandbox;

[Library( "weapon_ak47", Title = "AK47" )]
[Spawnable]
partial class AK47 : Weapon
{
	public override string ViewModelPath => "weapons/callofduty/vmdl/ak47/arms/ak47.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;

	public override int ClipSize => 30;
	public override float ReloadTime => 4.0f;
	public override int Bucket => 1;

	public override float bobbing_X => 0.1f;
	public override float bobbing_Y => 0.1f;
	public override float bobbing_Z => 0.1f;

	public override float ak47_Waves_Z => 0.1f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/callofduty/vmdl/ak47/arms/ak47_w.vmdl" );
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

		(Owner as AnimatedEntity)?.SetAnimParameter( "fire", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_smg.shoot" );
		//PlaySound( "pistolsound" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.1f, 1.5f, 1.0f, 3.0f );
	}

	public override void AttackSecondary()
	{
		// Grenade lob
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
			//new Sandbox.ScreenShake.Perlin( 0.5f, 1.0f, 0.4f, 0.7f );
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
