using Sandbox;



[Library( "weapon_shotgun", Title = "Shotgun" )]
[Spawnable]
partial class Shotgun : Weapon
{
	public override string ViewModelPath => "weapons/pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float PrimaryRate => 3.0f;
	public override float SecondaryRate => 2.0f;
	public override float ReloadTime => 0.5f;
	public override int ClipSize => 4;
	public override int Bucket => 3;

	[Net, Predicted]
	public bool StopReloading { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/pumpshotgun/rust_pumpshotgun.vmdl" );
		AmmoClip = 2;
	}

	//public override void Simulate( Client owner )
	//{
	//	base.Simulate( owner );

	//	if ( !IsReloading )
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

			if ( AvailableAmmo() > 0 || AvailableAmmo() == -1)
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

		if ( AmmoClip == 0 )
		{
			Reload();
		}
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		if ( !TakeAmmo( 2 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 || AvailableAmmo() == -1 )
			{
				Reload();
			}

			return;
		}

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

		if ( AmmoClip == 0 )
		{
			Reload();
		}
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

	public override void OnReloadFinish()
	{
		var stop = StopReloading;

		StopReloading = false;
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is ZePlayer player )
		{
			AmmoClip += 1;

			if ( AmmoClip < ClipSize && !stop )
			{
				Reload();
			}
			else
			{
				FinishReload();
			}
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

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var color = Color.Lerp( Color.Red, Color.Orange, lastReload.LerpInverse( 0.0f, 0.4f ) );
		//var color = Color.Cyan;
		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		// center (inner ring)
		{
			var shootEase = 1 + Easing.BounceIn( lastAttack.LerpInverse( 0.3f, 0.0f ) );
			draw.Ring( center, 30 * shootEase, 26 * shootEase );
		}
		// center (outer ring)
		{
			var shootEase = 1 + Easing.BounceIn( lastAttack.LerpInverse( 0.3f, 0.0f ) );
			draw.Ring( center, 54 * shootEase, 52 * shootEase );
		}

		// outer lines
		{
			var shootEase = Easing.EaseInOut( lastAttack.LerpInverse( 0.4f, 0.0f ) );
			var length = 30.0f;
			var gap = 10.0f + shootEase * 50.0f;
			var thickness = 4.0f;
			var extraAngle = 30 * shootEase;

			draw.CircleEx( center + Vector2.Right * gap, length, length - thickness, 32, 220, 320 );
			draw.CircleEx( center - Vector2.Right * gap, length, length - thickness, 32, 40, 140 );

			draw.Color = draw.Color.WithAlpha( 0.1f );
			draw.CircleEx( center + Vector2.Right * gap * 6.6f, length, length - thickness * 0.5f, 32, 220, 320 );
			draw.CircleEx( center - Vector2.Right * gap * 6.6f, length, length - thickness * 0.5f, 32, 40, 140 );
		}
	}
}
