using Sandbox;
using System.Threading.Tasks;


public partial class ZeCore : Game
{
	[Net] public float CounterToMotherZombie { get; set; } = 25.0f;
	[Net] public bool OnlyOnce { get; set; } = false;
	[Net] public bool IsZombie { get; set; } = false;

	[Net] public int Humans { get; set; } = 0;
	[Net] public int Zombies { get; set; } = 0;


	[Net] public string RoundResultText { get; set; } = "";
	[Net] public int RoundCounter { get; set; } = 0;
	[Net] public bool RoundStatusCheck { get; set; } = false;

	public ZeCore()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
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

	public async Task UI_MotherZombie()
	{
		//if(OnlyOnce)
		//{
		//	return;
		//}

		//OnlyOnce = true;

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
}
