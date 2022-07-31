using Sandbox;
using System;

[Library( "dm_pistol", Title = "Pistol" )]
[EditorModel( "weapons/ak74/w_ak74.vmdl" )]
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

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

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

		if ( Owner == Local.Pawn )
		{
			// Need to implement ScreenShake method
			//new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 0.5f, 0.5f );
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void RenderCrosshair( in Vector2 center, float lastAttack, float lastReload )
	{
		var draw = Render.Draw2D;

		var shootEase = Easing.EaseIn( lastAttack.LerpInverse( 0.2f, 0.0f ) );
		var color = Color.Lerp( Color.Red, Color.Yellow, lastReload.LerpInverse( 0.0f, 0.4f ) );

		draw.BlendMode = BlendMode.Lighten;
		draw.Color = color.WithAlpha( 0.2f + CrosshairLastShoot.Relative.LerpInverse( 1.2f, 0 ) * 0.5f );

		var length = 10.0f - shootEase * 2.0f;
		var gap = 5.0f + shootEase * 30.0f;
		var thickness = 2.0f;

		draw.Line( thickness, center + Vector2.Left * gap, center + Vector2.Left * (length + gap) );
		draw.Line( thickness, center - Vector2.Left * gap, center - Vector2.Left * (length + gap) );

		draw.Line( thickness, center + Vector2.Up * gap, center + Vector2.Up * (length + gap) );
		draw.Line( thickness, center - Vector2.Up * gap, center - Vector2.Up * (length + gap) );
	}
}
