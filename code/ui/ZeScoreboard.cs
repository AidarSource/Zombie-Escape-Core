using Sandbox.UI;


public partial class ZeScoreboard : Panel
{
	public static ZeScoreboard Current;

	public ZeScoreboard()
	{
		Current = this;
		StyleSheet.Load( "/ui/DamageIndicator.scss" );
	}
}
