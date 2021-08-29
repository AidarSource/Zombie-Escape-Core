using Sandbox.UI;

public partial class MotherZM : Label
{
	public override void Tick()
	{
		SetClass( "hidden", ((ZeCore)ZeCore.Current).CounterToMotherZombie <= 0 );
		SetText( $"Zombie infection starts in {((ZeCore)ZeCore.Current).CounterToMotherZombie} seconds." );
	}
}

