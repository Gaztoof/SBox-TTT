using Sandbox;
using Sandbox.UI;

[Library]
public partial class TTTHud : HudEntity<RootPanel>
{
	public TTTHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/TTTHud.scss" );
		RootPanel.StyleSheet.Load( "/ui/SpawnMenu.scss" );
		RootPanel.StyleSheet.Load( "/ui/PlayerInfo.scss" );
		RootPanel.StyleSheet.Load( "/ui/InventoryBar.scss" );
		RootPanel.StyleSheet.Load( "/ui/RagdollInspect.scss" );
		RootPanel.StyleSheet.Load( "/ui/Radar.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<RagdollNameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<ScoreBoard<ScoreBoardEntry>>();
		RootPanel.AddChild<CurrentTool>();

		RootPanel.AddChild<PlayerInfo>();

		RootPanel.AddChild<InventoryBar>(); // inventory slots

		RootPanel.AddChild<Radar>();
		RootPanel.AddChild<RagdollInspect>();
		//RootPanel.AddChild<SpawnMenu>(); // to disable on real game
	}
}
