using Sandbox;
using System;

[Library( "dm_knife", Title = "Knife" )]
[EditorModel( "weapons/machete/machate.vmdl" )]
partial class Knife : Weapon
{
	//public override string ViewModelPath => "weapons/rust_boneknife/v_rust_boneknife.vmdl";
	public override string ViewModelPath => "weapons/machete/v_machete.vmdl";
	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 0.3f;

	public virtual int MeleeDistance => 100;//80;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/machete/machate.vmdl" );
	}
	public override void MeleeStrike( float damage, float force )
	{
		var forward = Owner.EyeRotation.Forward;
		forward = forward.Normal;
		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * MeleeDistance, 10f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;
			tr.Surface.DoBulletImpact( tr );
			if ( !IsServer ) continue;
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 120 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );
				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	public override void Reload()
	{
		// do nothing
	}

	public override void AttackPrimary()
	{
		//ShootEffects();
		PlaySound( "rust_boneknife.attack" );
		MeleeStrike( 30, 1.5f );

		if ( IsLocalPawn )
		{
			// Need to implement ScreenShake method
			//new Sandbox.ScreenShake.Perlin();
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 4 );
		anim.SetAnimParameter( "aimat_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_attack", 2.0f );
		anim.SetAnimParameter( "holdtype_handedness", 1 );
		anim.SetAnimParameter( "holdtype_pose", 0f );
	}

}
