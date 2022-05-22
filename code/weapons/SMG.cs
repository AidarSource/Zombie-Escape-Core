using Sandbox;

[Library( "weapon_smg", Title = "SMG" )]
[Spawnable]
partial class SMG : Weapon
{
	public override string ViewModelPath => "weapons/smg/v_rust_smg.vmdl";
	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	
	public override int ClipSize => 30;
	public override float ReloadTime => 2.5f;
	public override int Bucket => 1;
	//public override int BucketWeight => 150;
	//public override AmmoType AmmoType => AmmoType.SMG;
	//public override int AmmoMax => -1;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/smg/rust_smg.vmdl" );
		AmmoClip = 30;
	}

	public override void CreateViewModel()
	{
		base.CreateViewModel();
		ViewModelEntity.SetModel( ViewModelPath );
		ViewModelEntity.SetAnimGraph( "weapons/smg/v_smg.vanmgrph" );
		//ViewModelEntity.RenderColor = new Color32( (byte)(105 + Rand.Int( 20 )), (byte)(174 + Rand.Int( 20 )), (byte)(59 + Rand.Int( 20 )), 255 ).ToColor();
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

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		//PlaySound( "rust_smg.shoot" );
		PlaySound( "pistolsound" );

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
			//new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.005f );
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		// Need to implement crosshair
		//CrosshairPanel?.CreateEvent( "fire" );
	}

	//public override void OnReloadFinish()
	//{
	//	IsReloading = false;

	//	TimeSincePrimaryAttack = 0;
	//	TimeSinceSecondaryAttack = 0;

	//	if ( AmmoClip >= ClipSize )
	//		return;

	//	if ( Owner is ZePlayer player )
	//	{
	//		var ammo = player.TakeAmmo( AmmoType, 1 );
	//		if ( ammo == 0 )
	//			return;

	//		AmmoClip += ammo;

	//		if ( AmmoClip < ClipSize )
	//		{
	//			Reload();
	//		}
	//		else
	//		{
	//			//FinishReload();
	//		}
	//	}
	//}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 2 ); // TODO this is shit
		anim.SetAnimParameter( "aimat_weight", 1.0f );
	}

}
