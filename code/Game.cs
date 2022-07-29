using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;



public partial class ZeCore : Game
{
	public List<string> LastRoundZombies_Collection = new();
	public List<string> LastLastRoundZombies_Collection = new();
	// ...
	// Mother Zombie Variables
	// ...
	[Net] public int CounterToMotherZombie { get; set; } = 239139219;
	[Net] public float MotherZombie_SpawnRate { get; set; } = 0.1f;
	public byte CounterToClear_LastLastMZM_List = 0;
	public bool ItsFirstRound = true;
	[Net] public bool OnlyOnce { get; set; } = false;
	[Net] public bool IsZombie { get; set; } = false;
	[Net] public bool IgnoreImmunity { get; set; } = false;
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
		DebugOverlay.ScreenText( "Human Win rounds: " + HumanWinRounds, 11 );
		DebugOverlay.ScreenText( "Zombie Win rounds: " + ZombieWinRounds, 12 );
		base.Simulate( cl );
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		Event.Run( ZeEvent.Player.Connected, cl );

		var player = new ZePlayer();
		
		cl.Pawn = player;

		player.SetTeam( Team.Humans );
		player.Respawn();
	}


	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		Event.Run( ZeEvent.Player.Disconnected, cl.PlayerId, reason );

		base.ClientDisconnect( cl, reason );
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

		List<string> LastRoundMZM = new List<string>(LastRoundZombies_Collection);


		List<string> LastLastRoundMZM = new List<string>(LastLastRoundZombies_Collection);

		LastRoundZombies_Collection.Clear();
		LastLastRoundZombies_Collection.Clear();

		int Successfully_Spawned = 0;
		int CurrentlyPlayingPlayer_WithImmunity = 0;

		foreach( Client client in Client.All )
		{
			if ( client.Pawn is not ZePlayer player )
			{
				continue;
			}
			if( LastRoundMZM.Contains(client.ToString()) || LastLastRoundMZM.Contains( client.ToString() ) )
			{
				CurrentlyPlayingPlayer_WithImmunity++;
			}
		}

		if(CurrentlyPlayingPlayer_WithImmunity == Client.All.Count)
		{
			IgnoreImmunity = true;
		}


		while ( Successfully_Spawned <= NumberZmToSpawn )
		{

			Random rand = new Random();
			
			var target = Client.All[rand.Next( Client.All.Count )];

			// TODO: Convert to SteamId
			if ( (target.Pawn.Tags.Has( "zombie" ) || LastRoundMZM.Contains( target.ToString() )) && !IgnoreImmunity )
			{
				continue; // avoid from random choosing same zombie, or choosing last round MotherZombie
			}
			if ( LastLastRoundMZM.Contains( target.ToString() ) && rand.Next( 2 ) == 1 && !IgnoreImmunity )
			{
				// 50% chance to get immunity
				continue; // give immunity to LastLastRound spawned mother zombie
			}


			target.Pawn.Tags.Add( "zombie" );



			LastRoundZombies_Collection.Add( target.ToString() );
			if( IsServer)
			{
				var player = target.Pawn as ZePlayer;
				//target.Pawn.Inventory.DeleteContents();
				player.Inventory.DeleteContents();
			}
				
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
		LastLastRoundZombies_Collection = LastRoundMZM;
	}

	[ConCmd.Server( "ze_switch_teams" )]
	public static void SwitchTeams()
	{
		if ( ConsoleSystem.Caller.Pawn is ZePlayer player )
		{
			if ( player.Team == Team.Zombies )
				player.SetTeam( Team.Humans );
			else
				player.SetTeam( Team.Zombies );

			player.Respawn();
		}
	}

	public async Task RoundOver()
	{
		await GameTask.DelaySeconds( 7.0f );
		RoundCounter = 0;

		Humans = 0;
		Zombies = 0;

		foreach ( Client client in Client.All )
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

			if ( IsServer )
			{
				player.Inventory.DeleteContents();
			}

			await GameTask.DelaySeconds( 0.0001f );
			player.Respawn();

			Event.Run( ZeEvent.Game.RoundChange );


		}

		CounterToMotherZombie = 20;
		await MotherZombie();
	}
}
