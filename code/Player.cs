﻿using Sandbox;
using Sandbox.UI;
using System;

partial class ZePlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;
	private bool IsZombie = false;
	[Net, Predicted] public ICamera MainCamera { get; set; }

	public ICamera LastCamera { get; set; } 

	// Set Inventory to Player object
	public ZePlayer()
	{
		Inventory = new Inventory( this );

	}

	public override void Spawn()
	{
		MainCamera = new FirstPersonCamera();
		LastCamera = MainCamera;

		base.Spawn();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		// Use WalkController for movement (you can make your own PlayerController for 100% control)
		Controller = new WalkController();

		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		Animator = new StandardPlayerAnimator();


		// Use FirstPersonCamera (you can make your own Camera for 100% control)
		MainCamera = LastCamera;
		Camera = MainCamera;

		// Auto bhop = true
		(Controller as WalkController).AutoJump = true;


		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;



		Inventory.Add( new Pistol(), true );
		Inventory.Add( new SMG() );
		Inventory.Add( new Knife() );

		base.Respawn();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Show current item in Inventory
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;


		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		// Change Camera perspective with 'C' button
		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera is not FirstPersonCamera )
			{
				Camera = new FirstPersonCamera();
			}
			else
			{
				Camera = new ThirdPersonCamera();
			}
		}


		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if (GroundEntity != null)
		{
			DebugOverlay.ScreenText( "Ground" );
		}

		// Press two times space for Noclip
		//if ( Input.Released( InputButton.Jump ) )
		//{
		//	if ( timeSinceJumpReleased < 0.3f )
		//	{
		//		Game.Current?.DoPlayerNoclip( cl );
		//	}

		//	timeSinceJumpReleased = 0;
		//}
	}

	Client cl;
	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		LastDamage = info;

		if (!IsZombie && info.Weapon is Knife)
		{
			IsZombie = true;
			Sound.FromEntity( "zm_infect", this );
			this.RenderColor = new Color32( (byte)(105 + Rand.Int( 20 )), (byte)(174 + Rand.Int( 20 )), (byte)(59 + Rand.Int( 20 )), 255 );
		}



		// hack - hitbox 0 is head
		// we should be able to get this from somewhere
		if ( info.HitboxIndex == 0 )
		{
			info.Damage *= 2.0f;
		}

		base.TakeDamage( info );

		if ( info.Attacker is ZePlayer attacker && attacker != this )
		{
			// Note - sending this only to the attacker!
			//attacker.DidDamage( To.Single( attacker ), info.Position, info.Damage, Health.LerpInverse( 100, 0 ) );

			TookDamage( To.Single( this ), info.Weapon.IsValid() ? info.Weapon.Position : info.Attacker.Position );
		}
	}

	[ClientRpc]
	public void TookDamage( Vector3 pos )
	{
		DamageIndicator.Current?.OnHit( pos );
	}


}

