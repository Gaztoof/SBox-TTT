
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ScoreBoardEntry : Panel
{
	public Sandbox.PlayerScore.Entry Entry;

	public Label PlayerName;
	public Label Karma;
	public Label Deaths;
	public Label Score;
	public Label Ping;
	public Image ProfilePicture;

	// change colors according to rank!
	public ScoreBoardEntry()
	{
		AddClass( "entry" );

		ProfilePicture = Add.Image( "", "profilePicture" );
		PlayerName = Add.Label( "PlayerName", "name" );

		var Details = Add.Panel( "detailsList" );

		Karma = Details.Add.Label( "", "karma" );
		Score = Details.Add.Label( "", "score" );
		Deaths = Details.Add.Label( "", "deaths" );
		Ping = Details.Add.Label( "", "ping" );
	}

	public virtual void UpdateFrom( Sandbox.PlayerScore.Entry entry )
	{
		Entry = entry;

		PlayerName.Text = entry.GetString( "name" );
		Karma.Text = entry.Get<int>( "karma", 0 ).ToString();
		Score.Text = entry.Get<int>( "score", 0 ).ToString();
		Deaths.Text = entry.Get<int>( "deaths", 0 ).ToString();
		Ping.Text = entry.Get<int>( "ping", 0 ).ToString();
		ProfilePicture.Texture = Texture.Load( "avatar:" + entry.Get<ulong>( "steamid", 0 ) );

		//Ping.Text = entry.Get<Player.ScoreboardGroup>( "ping", 0 ).ToString();
		SetClass( "dev", Local.Client != null && entry.Get<ulong>( "steamid", 0 ) == 76561198195435125 && Game.ConVars.HighlightAdmins );
		SetClass( "me", Local.Client != null && entry.Get<ulong>( "steamid", 0 ) == Local.Client.SteamId );

		var role = entry.Get<Player.Role>( "role", 0 );

		// exploitable as hell but i didnt know what else to do ....
		SetClass( "detective", role == Player.Role.Detective );
		
		if ( (Local.Pawn is Player localPly && localPly.role == Player.Role.Traitor) || entry.Get<bool>( "isdead", false ) )
			SetClass( "traitor", role == Player.Role.Traitor );
	}
}
