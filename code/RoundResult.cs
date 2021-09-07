using Sandbox.UI;

public partial class RoundResult : Label
{
	public override void Tick()
	{
		SetClass( "hidden", ((ZeCore)ZeCore.Current).RoundCounter == 0 );
		SetText( ((ZeCore)ZeCore.Current).RoundResultText );
		SetProperty( "color", "red" );
	}
}

