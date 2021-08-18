
using Sandbox;
using Sandbox.Hooks;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

public partial class ScoreBoard<T> : Panel where T : ScoreBoardEntry, new()
{
	public class ListGroup
	{
		public Panel Separator { get; set; }
		public Label SeparatorLabel { get; set; }
		public Panel ListCanvas { get; set; }
		private string Name { get; set; }
		public int Value { get; set; } = 0;
		public ListGroup(string name)
		{
			Name = name;
		}
		public void UpdateValue(int value)
		{
			SeparatorLabel.SetText( Name + $" ({value})" );
			if ( value == 0 )
				ListCanvas.Style.Display = DisplayMode.None;
			else ListCanvas.Style.Display = DisplayMode.Flex;
			ListCanvas.Style.Dirty();
		}
		public void Init(string className)
		{
			Separator = ListCanvas.Add.Panel( "separator" );
			SeparatorLabel = Separator.Add.Label( "", "separatorLabel" );
			Separator.SetClass( className, true );
		}
	}
	public Panel Canvas { get; protected set; }
	Dictionary<int, T> Entries = new();

	public Panel Header { get; protected set; }
	public ListGroup TerrorListGroup { get; protected set; } = new("Terrorists");
	public ListGroup MissingListGroup { get; protected set; } = new ("Missing In Action");
	public ListGroup ConfirmedListGroup { get; protected set; } = new ("Confirmed Dead");
	public ListGroup SpectatorListGroup { get; protected set; } = new ("Spectators");

	public ScoreBoard()
	{
		StyleSheet.Load( "/ui/ScoreBoard.scss" );
		AddClass( "scoreboard" );

		Add.Label( "You are playing on...", "playingOn" );


		var serverBar = Add.Panel( "serverBar" );
		serverBar.Add.Label( "Trouble In Terrorist Town", "serverName" );
		// edit that later?

		Add.Image( "images/logo.png", "logo" );

		AddHeader();


		Canvas = Add.Panel( "canvas" );

		PlayerScore.OnPlayerAdded += AddPlayer;
		PlayerScore.OnPlayerUpdated += UpdatePlayer;
		PlayerScore.OnPlayerRemoved += RemovePlayer;
		TerrorListGroup.ListCanvas = Canvas.Add.Panel( "playersList" );
		MissingListGroup.ListCanvas = Canvas.Add.Panel( "playersList" );
		ConfirmedListGroup.ListCanvas = Canvas.Add.Panel( "playersList" );
		SpectatorListGroup.ListCanvas = Canvas.Add.Panel( "playersList" );

		TerrorListGroup.Init( "terror" );
		MissingListGroup.Init( "mia" );
		ConfirmedListGroup.Init( "confirmed" );
		SpectatorListGroup.Init( "spectators" );
		TerrorListGroup.UpdateValue( 0 );
		MissingListGroup.UpdateValue( 0 );
		ConfirmedListGroup.UpdateValue( 0 );
		SpectatorListGroup.UpdateValue( 0 );

		foreach ( var player in PlayerScore.All )
		{
			AddPlayer( player );
		}
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "open", Input.Down( InputButton.Score ) );
	}


	protected virtual void AddHeader()
	{
		Header = Add.Panel( "header" );
		Header.Add.Panel( "name" );
		var Details = Header.Add.Panel( "detailsList" );
		Details.Add.Label( "Karma", "karma" ).SetClass("noBack", true);
		Details.Add.Label( "Score", "score" ).SetClass( "noBack", true );
		Details.Add.Label( "Deaths", "deaths" ).SetClass( "noBack", true );
		Details.Add.Label( "Ping", "ping" ).SetClass( "noBack", true );
	}

	protected virtual void AddPlayer( Sandbox.PlayerScore.Entry entry )
	{
		var scoreboardGroup = entry.Get<Player.ScoreboardGroup>( "state", Player.ScoreboardGroup.Spectator );
		ListGroup targetPanel = SpectatorListGroup;
		switch ( scoreboardGroup )
		{
			case Player.ScoreboardGroup.Terror:
				targetPanel = TerrorListGroup;
				break;
			case Player.ScoreboardGroup.Found:
				targetPanel = ConfirmedListGroup;
				break;
			case Player.ScoreboardGroup.NotFound:
				targetPanel = MissingListGroup;
				break;
			case Player.ScoreboardGroup.Spectator:
				targetPanel = SpectatorListGroup;
				break;
		}
		targetPanel.UpdateValue( ++targetPanel.Value );


		var p = targetPanel.ListCanvas.AddChild<T>();
		p.UpdateFrom( entry );


		Entries[entry.Id] = p;
	}

	protected virtual void UpdatePlayer( Sandbox.PlayerScore.Entry entry )
	{
		var scoreboardGroup = entry.Get<Player.ScoreboardGroup>( "state", Player.ScoreboardGroup.Spectator );

		if ( Entries.TryGetValue( entry.Id, out var panel ) )
		{
			ListGroup targetPanel = SpectatorListGroup;
			switch(scoreboardGroup)
			{
				case Player.ScoreboardGroup.Terror:
					targetPanel = TerrorListGroup;
					break;
				case Player.ScoreboardGroup.Found:
					targetPanel = ConfirmedListGroup;
					break;
				case Player.ScoreboardGroup.NotFound:
					targetPanel = MissingListGroup;
					break;
				case Player.ScoreboardGroup.Spectator:
					targetPanel = SpectatorListGroup;
					break;
			}
			RemovePlayer( entry );
			AddPlayer( entry );

			panel.UpdateFrom( entry );
		}
	}

	protected virtual void RemovePlayer( Sandbox.PlayerScore.Entry entry )
	{
		if ( Entries.TryGetValue( entry.Id, out var panel ) )
		{
			if ( TerrorListGroup.ListCanvas == panel.Parent ) TerrorListGroup.UpdateValue( --TerrorListGroup.Value );
			if ( MissingListGroup.ListCanvas == panel.Parent ) MissingListGroup.UpdateValue( --MissingListGroup.Value );
			if ( ConfirmedListGroup.ListCanvas == panel.Parent ) ConfirmedListGroup.UpdateValue( --ConfirmedListGroup.Value );
			if ( SpectatorListGroup.ListCanvas == panel.Parent ) SpectatorListGroup.UpdateValue( --SpectatorListGroup.Value );
			
				panel.Delete();
			
			Entries.Remove( entry.Id );
		}
	}
}
