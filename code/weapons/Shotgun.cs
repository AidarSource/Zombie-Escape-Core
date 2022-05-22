using Sandbox;



[Library( "weapon_shotgun", Title = "Shotgun" )]
[Spawnable]
partial class Shotgun : Weapon
{
	public override string ViewModelPath => "weapons/pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 3.0f;
	public override float SecondaryRate => 2.0f;
	public override float ReloadTime => 0.5f;
	public override int ClipSize => 2;
	public override int Bucket => 3;

	//[Net, Predicted]
	//public bool StopReloading { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/pumpshotgun/rust_pumpshotgun.vmdl" );
		AmmoClip = 2;
	}

	//public override void Simulate( Client owner )
	//{
	//	base.Simulate( owner );

	//	if ( IsReloading && (Input.Pressed( InputButton.Attack1 ) || Input.Pressed( InputButton.Attack2 )) )
	//	{
	//		StopReloading = true;
	//	}
	//}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}

			return;
		}

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_pumpshotgun.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullets( 10, 0.1f, 10.0f, 9.0f, 3.0f );
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "rust_pumpshotgun.shootdouble" );

		//
		// Shoot the bullets
		//
		ShootBullets( 20, 0.4f, 20.0f, 8.0f, 3.0f );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParameter( "fire", true );

		if ( IsLocalPawn )
		{
			// Need to implement ScreenShake method
			//new Sandbox.ScreenShake.Perlin( 1.0f, 1.5f, 2.0f );
		}
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire_double", true );
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			// Need to implement ScreenShake method
			//new Sandbox.ScreenShake.Perlin( 3.0f, 3.0f, 3.0f );
		}
	}


	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}
