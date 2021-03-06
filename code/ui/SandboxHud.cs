using Sandbox;
using Sandbox.UI;

[Library]
public partial class SandboxHud : HudEntity<RootPanel>
{
	public SandboxHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/SandboxHud.scss" );

		RootPanel.AddChild<DamageIndicator>();
		//RootPanel.AddChild<NameTags>();
		//RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		//RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard>();
		//RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<MotherZM>();
		RootPanel.AddChild<RoundResult>();
		RootPanel.AddChild<Ammo>();
	}
}
