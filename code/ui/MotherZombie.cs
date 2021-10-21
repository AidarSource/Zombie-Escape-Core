using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class MotherZM : Panel
{
	public Label Label;
	public MotherZM()
	{
		Label = Add.Label( "Test" );
	}
	public override void Tick()
	{
		SetClass( "hidden", ((ZeCore)ZeCore.Current).CounterToMotherZombie <= 0 );
		Label.Text = $"Zombie infection starts in {((ZeCore)ZeCore.Current).CounterToMotherZombie} seconds.";
	}
}

