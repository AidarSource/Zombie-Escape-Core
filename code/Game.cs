using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public partial class ZeCore : Game
{
	//public List<string> LastRoundZombies_Collection;
	public List<string> LastRoundZombies_Collection = new();
	// ...
	// Mother Zombie Variables
	// ...
	[Net] public int CounterToMotherZombie { get; set; } = 25;
	[Net] public float MotherZombie_SpawnRate { get; set; } = 0.1f;
	[Net] public bool OnlyOnce { get; set; } = false;
	[Net] public bool IsZombie { get; set; } = false;
	// ...
	// Team status Variables
	// ...
	[Net] public int Humans { get; set; } = 0;
	[Net] public int Zombies { get; set; } = 0;
	[Net] public int HumanWinRounds { get; set; } = 0;
	[Net] public int ZombieWinRounds { get; set; } = 0;
	// ...
	// Round Result Variables
	// ...
	[Net] public string RoundResultText { get; set; } = "";
	[Net] public int RoundCounter { get; set; } = 0;
	// Start Round win checker when MotherZombie spawned
	[Net] public bool RoundStatusCheck { get; set; } = false;

	public ZeCore()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
	}

	public override void Simulate( Client cl )
	{
		DebugOverlay.ScreenText( 11, "Human Win rounds: " + HumanWinRounds );
		DebugOverlay.ScreenText( 12, "Zombie Win rounds: " + ZombieWinRounds );
		base.Simulate( cl );
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new ZePlayer();
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	// Timer to spawn MotherZombie
	public async Task UI_MotherZombie()
	{
		while ( true )
		{
			await GameTask.DelaySeconds( 1.0f );
			CounterToMotherZombie--;
			

			if ( CounterToMotherZombie <= 0 )
			{
				return;
			}
		}
	}

	public async Task MotherZombie()
	{
		await UI_MotherZombie();
		
		
		int NumberZmToSpawn = (int)Math.Round( Client.All.Count * MotherZombie_SpawnRate );

		List<string> LastRoundMZM = new();
		LastRoundMZM = LastRoundZombies_Collection;

		LastRoundZombies_Collection.Clear();



		int Successfully_Spawned = 0;

		while ( Successfully_Spawned <= NumberZmToSpawn )
		{
			

			Random rand = new Random();
			
			var target = Client.All[rand.Next( Client.All.Count )];
			if ( target.Pawn.Tags.Has( "zombie" ) || LastRoundMZM.Contains( target.ToString() ) )
			{
				continue; // avoid from random choosing same zombie, or choosing last round MotherZombie
			}

			target.Pawn.Tags.Add( "zombie" );
			Log.Info( target.ToString() );

			Log.Info( LastRoundMZM );
			
			LastRoundZombies_Collection.Add( target.ToString() );
			target.Pawn.Inventory.DeleteContents();
			await GameTask.DelaySeconds( 0.0001f );



			foreach ( Client client in Client.All )
			{
				if ( client.Pawn is not ZePlayer player )
				{
					continue;
				}
				if ( client == target )
				{
					player.Respawn();
					player.Health = 5000;
					Humans--;
				}

			}
			target.Pawn.PlaySound( "zm_infect" );

			RoundStatusCheck = true;

			Successfully_Spawned++;
		}
	}

	public async Task RoundOver()
	{
		await GameTask.DelaySeconds( 5.0f );
		RoundCounter = 0;

		foreach (Client client in Client.All)
		{
			if ( client.Pawn is not ZePlayer player )
			{
				continue;
			}

			if ( player.Tags.Has( "zombie" ) )
			{
				player.Tags.Remove( "zombie" );
			}

			RoundStatusCheck = false;
			


			Humans = 0;
			Zombies = 0;
			player.Inventory.DeleteContents();

			await GameTask.DelaySeconds( 0.0001f );
			player.Respawn();


		}
		CounterToMotherZombie = 25;
		await MotherZombie();
	}
}
