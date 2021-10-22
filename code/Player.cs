using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;


partial class ZePlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	public bool SupressPickupNotices { get; private set; }

	private int counter = 20;

	private TimeSince LocalTimeSince;

	private bool IsZombie = false;

	[Net, Predicted] public ICamera MainCamera { get; set; }

	public ICamera LastCamera { get; set; } 

	// Set Inventory to Player object
	public ZePlayer()
	{
		Inventory = new Inventory( this );
	}


	//public async void MotherZombie()
	//{
		
	//	await ((ZeCore)ZeCore.Current).UI_MotherZombie();

	//	//IsZombie = true;
	//	Tags.Add( "zombie" );


	//	Inventory.DeleteContents();


	//	await GameTask.DelaySeconds( 0.01f );
	//	Respawn();
	//	Health = 5000;

	//	((ZeCore)ZeCore.Current).Humans--;
	//	//((ZeCore)ZeCore.Current).Zombies++;



	//	Sound.FromEntity( "zm_infect", this );
	//	//this.RenderColor = new Color32( (byte)(105 + Rand.Int( 20 )), (byte)(174 + Rand.Int( 20 )), (byte)(59 + Rand.Int( 20 )), 255 );
	//	((ZeCore)ZeCore.Current).RoundStatusCheck = true;
	//}



	public async void Infector()
	{
		//IsZombie = true;
		Tags.Add( "zombie" );
		((ZeCore)ZeCore.Current).Humans--;
		((ZeCore)ZeCore.Current).Zombies++;

		this.Inventory.DeleteContents();

		await GameTask.DelaySeconds( 0.01f );


		Inventory.Add( new Knife(), true );
		Health = 5000;
		Sound.FromEntity( "zm_infect", this );
		this.RenderColor = new Color32( (byte)(105 + Rand.Int( 20 )), (byte)(174 + Rand.Int( 20 )), (byte)(59 + Rand.Int( 20 )), 255 ).ToColor();
	}


	public override void Spawn()
	{
		MainCamera = new FirstPersonCamera();
		LastCamera = MainCamera;

		if( !((ZeCore)ZeCore.Current).OnlyOnce )
		{
			((ZeCore)ZeCore.Current).OnlyOnce = true;
			//MotherZombie();
			_ = ((ZeCore)ZeCore.Current).MotherZombie();
		}

		if( ((ZeCore)ZeCore.Current).CounterToMotherZombie == 0 )
		{
			//IsZombie = true;
			Tags.Add( "zombie" );
			this.Health = 5000;
		}


		//if( !Tags.Has("zombie") )
		//{
		//	((ZeCore)ZeCore.Current).Humans++;
		//} else
		//{
		//	((ZeCore)ZeCore.Current).Zombies++;
		//}


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



		if( Tags.Has("zombie") )
		{
			((ZeCore)ZeCore.Current).Zombies++;
			Inventory.Add( new Knife(), true );
			DebugOverlay.ScreenText( 9, "You're zombie!", 5.0f );
			this.Health = 5000;
			this.RenderColor = new Color32( (byte)(105 + Rand.Int( 20 )), (byte)(174 + Rand.Int( 20 )), (byte)(59 + Rand.Int( 20 )), 255 ).ToColor();

		} else
		{
			DebugOverlay.ScreenText( 10, "You're human!", 5.0f );

			((ZeCore)ZeCore.Current).Humans++;

			Inventory.Add( new PM(), true );
			Inventory.Add( new SMG() );
			Inventory.Add( new AK74() );

			// basic citizen color
			this.RenderColor = new Color32( 255, 255, 255, 255 ).ToColor();
		}

		SupressPickupNotices = true;

		GiveAmmo( AmmoType.Pistol, 100 );

		SupressPickupNotices = false;

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


		DebugOverlay.ScreenText( 3, "Humans: " + ((ZeCore)ZeCore.Current).Humans.ToString() );
		DebugOverlay.ScreenText( 4, "Zombies: " + ((ZeCore)ZeCore.Current).Zombies.ToString() );


		

		if ( ((ZeCore)ZeCore.Current).RoundStatusCheck )
		{
			if ( ((ZeCore)ZeCore.Current).RoundCounter == 0 && (((ZeCore)ZeCore.Current).Humans == 0 || ((ZeCore)ZeCore.Current).Zombies == 0) )
			{
				((ZeCore)ZeCore.Current).RoundCounter++;
				if ( ((ZeCore)ZeCore.Current).Humans == 0 )
				{
					((ZeCore)ZeCore.Current).RoundResultText = "ZOMBIES WIN THE ROUND";
					((ZeCore)ZeCore.Current).ZombieWinRounds++;
					_ = ((ZeCore)ZeCore.Current).RoundOver();
					//_ = ((ZeCore)ZeCore.Current).MotherZombie();
				}
				else
				{
					((ZeCore)ZeCore.Current).RoundResultText = "HUMANS WIN THE ROUND";
					((ZeCore)ZeCore.Current).HumanWinRounds++;
				}
			}
		}


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

		if(Input.Pressed(InputButton.Flashlight))
		{
			Sound.FromEntity( "ayayo", this );
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
			DebugOverlay.ScreenText(1, "Ground" );
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



	DamageInfo LastDamage;

	public override void TakeDamage( DamageInfo info )
	{
		LastDamage = info;
		if ( !Tags.Has("zombie") && info.Weapon is Knife)
		{
			Infector();
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

