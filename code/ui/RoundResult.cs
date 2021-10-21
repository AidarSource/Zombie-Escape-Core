using Sandbox.UI;
using Sandbox.UI.Construct;

public partial class RoundResult : Panel
{
	public Label Label;
	public RoundResult()
	{
		Label = Add.Label( "Result", "result" );
	}

	public override void Tick()
	{
		SetClass( "hidden", ((ZeCore)ZeCore.Current).RoundCounter == 0 );
		//SetText( ((ZeCore)ZeCore.Current).RoundResultText );
		Label.Text = ((ZeCore)ZeCore.Current).RoundResultText;
		SetProperty( "color", "red" );
	}
}

