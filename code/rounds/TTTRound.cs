using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TTTRound : BaseRound
{
	public enum RoundPhase
	{
		Preparation,
		InGame,
		RoundOver
	}
	public override string RoundName => "TTT";
	public int PreparationTime => 60;
	public int RoundOverTime => 10;
	public override int RoundDuration => (300 + PreparationTime + RoundOverTime);
	public string PreparationTimeString { get { return ((TimeLeftSeconds > (RoundDuration - PreparationTime)) ? TimeSpan.FromSeconds( PreparationTime - (RoundDuration - TimeLeftSeconds) ).ToString( "mm':'ss" ) : TimeSpan.FromSeconds(0).ToString( "hh':'mm" )); } }
	public string TimeLeftString { get { return TimeSpan.FromSeconds( TimeLeftSeconds - RoundOverTime).ToString( "mm':'ss" ); } }
	public string RoundOverLeftString { get { return TimeSpan.FromSeconds( TimeLeftSeconds).ToString( "mm':'ss" ); } }

	// preparing, playing
	//30s after round end it restarts
	protected override void OnStart()
	{
		Log.Info( "Started TTT Round" );

		onRoundStartCalled = false;
		onRoundOverCalled = false;
		if ( Host.IsServer )
		{
			var players = Client.All.Select( ( client ) => client.Pawn as Player ).ToList();
			foreach ( var player in players )
			{
				// if hes not a specOnly, reset his health and respawn him
				if ( !player.spectatorOnly )
				{
					player.shouldBeSpec = false;
					player.Respawn();
					player.Health = player.maxHealth;
					player.team = Player.Team.Terror;
				}
				else
				{
					AddSpectator( player );
					player.team = Player.Team.Spectator;
				}

				OnPlayerJoin( player );
			}
			

				Karma.RoundBegin( players );
			//Game.Instance.RemoveAllBalls();

			// maybe clear inventories? / entities
		}

	}
	public RoundPhase GetRoundPhase()
	{
		if ( TimeLeftSeconds >= (RoundDuration - PreparationTime) )
			return RoundPhase.Preparation;
		else if(TimeLeftSeconds > RoundOverTime)return RoundPhase.InGame;
		else return RoundPhase.RoundOver;
	}
	protected override void OnFinish()
	{
		Log.Info( "Finished TTT Round" );
		Karma.RoundEnd( Players );
	}

	public override void OnPlayerJoin( Player player )
	{
		if ( Players.Contains( player ) )
		{
			return;
		}
		
		if ( (GetRoundPhase() == RoundPhase.InGame || GetRoundPhase() == RoundPhase.Preparation) && player.team != Player.Team.Spectator )
		{
				Log.Info(  "x was made a player on join." );
			AddPlayer( player );
		}
		else
		{
				Log.Info( " was made a spec on join." );
			player.MakeSpectator( true, true );
			AddSpectator( player );
		}

		base.OnPlayerJoin( player );
	}
	public override void OnPlayerLeave( Player player )
	{
		RemovePlayer( player );
		base.OnPlayerLeave( player );
	}
	protected override void OnTimeUp( )
	{
		// restart when time up
		Start();
	}
	private bool onRoundStartCalled;
	public void OnRoundStart( )
	{
		Log.Info( "Round start!" );
		List<Player> choices = new();
		foreach ( var ply in Players )
		{
			if ( ply is not Player || ply.spectatorOnly ) continue;
			ply.previousRole = ply.role;
			ply.role = Player.Role.Innocent;
			choices.Add( ply );
		}
		
		if ( Host.IsServer )
		{
			int DesiredTraitors = GetTraitorCount( choices.Count );
			int DesiredDetectives = GetDetectiveCount( choices.Count );
			int traitorsCount = 0;
			int detectivesCount = 0;

			while ( traitorsCount < DesiredTraitors && choices.Count > 0 )
			{
				var selectedPly = choices[Rand.Int( 0, choices.Count - 1 )];
				if ( selectedPly.role == Player.Role.Traitor || Rand.Int( 0, 90 ) <= 30 )
				{
					selectedPly.role = Player.Role.Traitor;
					traitorsCount++;
					choices.Remove( selectedPly );
				}
			}
			while ( detectivesCount < DesiredDetectives && choices.Count > 0 )
			{
				if ( choices.Count <= (DesiredDetectives - detectivesCount) )
				{
					foreach ( var choice in choices )
					{
						choice.role = Player.Role.Detective;
					}
					break;
				}

				var selectedPly = choices[Rand.Int( 0, choices.Count - 1 )];
				if ( selectedPly.karma < Game.ConVars.MinDetectiveKarma && selectedPly.previousRole == Player.Role.Innocent || Rand.Int( 0, 90 ) <= 30 )
				{
					if ( !selectedPly.avoidDetective )
					{
						selectedPly.role = Player.Role.Detective;
						detectivesCount++;
					}
					choices.Remove( selectedPly );
				}
			}
		}
		/*foreach ( var ply in Players )
			Log.Info( ply.GetClientOwner().Name + " : " + ply.team.ToString() + " : " + ply.role.ToString()) ;*/

		onRoundStartCalled = true;
	}
	private bool onRoundOverCalled;
	public void OnRoundOver( )
	{
		Log.Info( "Round over!" );
		onRoundOverCalled = true;
	}
	public override void OnSecond()
	{
		base.OnSecond();
		if ( Host.IsClient ) return;

		// for reasons i ignore, this only called by cli
		if ( !onRoundStartCalled && TimeLeftSeconds <= (RoundDuration-PreparationTime) && TimeLeftSeconds > 1 ) OnRoundStart();
		if ( !onRoundOverCalled && TimeLeftSeconds <= RoundOverTime && TimeLeftSeconds > 1 ) OnRoundOver();
		if(GetRoundPhase() == RoundPhase.InGame && IsRoundOver() != Player.Role.Spectator)
		{
			Log.Info( "Round end! Winning team: " + IsRoundOver() );
			Sandbox.UI.ChatBox.AddInformation( To.Everyone, $"Congratulations, {IsRoundOver().ToString()}s won!", "https://i.imgur.com/XUxaxpB.png" );

			RoundEndTime = Sandbox.Time.Now + RoundOverTime;
			//Start();
		}
		foreach(var ply in Players)
		{
			if ( ply.GetClientOwner() != null )
			{
					ply.GetClientOwner().SetScore( "state", ply.Scoreboardgroup );
			}
		}
		foreach(var ply in Spectators)
		{
			ply.GetClientOwner().SetScore( "state", Player.ScoreboardGroup.Spectator );
		}
	}
	public int GetTraitorCount(int ply_count)
	{
		return Math.Clamp( (int)Math.Floor( ply_count * Game.ConVars.TraitorPct ), 1, Game.ConVars.MaxTraitor );
	}
	public int GetDetectiveCount(int ply_count)
	{
		if ( Players.Count < Game.ConVars.DetectiveMinPlayers ) return 0;
		return Math.Clamp( (int)Math.Floor( ply_count * Game.ConVars.DetectivePct ), 1, Game.ConVars.MaxDetective );
	}
	private Player.Role IsRoundOver()
	{
		int terrorists = 0;
		int innocents = 0;
		foreach ( var ply in Players )
		{
			if ( ply.team == Player.Team.Terror && !ply.IsDead() )
			{
				if ( ply.role == Player.Role.Traitor )
					terrorists++;
				else if ( ply.role == Player.Role.Innocent || ply.role == Player.Role.Detective )
					innocents++;
			}
		}

		// if both team have players, or both team empty, return nobody
		if ( (terrorists == 0 && innocents == 0) || (terrorists > 0 && innocents > 0) )
					return Player.Role.Spectator;
		else return innocents == 0 ? Player.Role.Traitor : Player.Role.Innocent;
	}
	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );
		
		if ( GetRoundPhase() == RoundPhase.InGame && !player.IsSpectator() )
		{
			player.MakeSpectator( true, false );
			Log.Info( player.GetClientOwner().Name+" player turned into spectator." );
		}
	}
	public override void OnPlayerSpawn( Player player )
	{
		base.OnPlayerSpawn( player );
		if ( player.spectatorOnly )
		{
			if ( !Spectators.Contains( player ) ) AddSpectator( player );
			if ( Players.Contains( player ) ) RemovePlayer( player );
		}
		else
		{
			RemoveSpectator( player );
			AddPlayer( player );
			// setloadout
		}
	}

}
