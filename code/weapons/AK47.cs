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

		if ( AmmoClip == 0 )
		{
			Reload();
		}
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
