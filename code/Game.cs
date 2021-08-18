using Sandbox;
using System.Threading.Tasks;
using System.Linq;

public static class EventType
{
	public const string ClientTick = "client.tick";
	public const string Tick = "tick";
}

[Library( "TTT", Title = "Trouble In Terrorist Town" )]
partial class Game : Sandbox.Game
{
	public TTTHud Hud { get; set; }
	[Net] public BaseRound Round { get; private set; }
	[Net] public Player CurrentPlayer { get; set; }

		

	public static Game Instance
	{
		get => Current as Game;
	}

	public void ChangeRound( BaseRound round )
	{
		Assert.NotNull( round );

		Round?.Finish();
		Round = round;
		Round?.Start();
	}
	public override void PostLevelLoaded()
	{
		_ = StartSecondTimer();

		if ( IsServer )
		{
			//var cue = new PoolCue();
			//Cue = cue;
		}

		base.PostLevelLoaded();
	}
	public async Task StartSecondTimer()
	{
		while ( true )
		{
			await Task.DelaySeconds( 1 );
			OnSecond();
		}
	}

	private void CheckMinimumPlayers()
	{
		if (Round is not null && Round.Players.Count >= ConVars.MinPlayers )
		{
			if(Round is LobbyRound || Round == null)
				ChangeRound( new TTTRound() );
		}
		else if ( Round is not LobbyRound )
		{
			ChangeRound( new LobbyRound() );
		}
	}

	public Game()
	{
		if ( IsServer )
		{
			// Create the HUD
			Hud = new();
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var player = new Player();
		player.Respawn();

		Round?.OnPlayerJoin( player );

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	// disabled!
	public override void DoPlayerNoclip( Client player )
	{
		return;
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}
	private void OnSecond()
	{
		CheckMinimumPlayers();
		Round?.OnSecond();
	}
	[Event( EventType.Tick )]
	private void Tick()
	{
		Round?.OnTick();
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		if ( Round is not TTTRound round || IsClient ) return;
		if ( pawn.LastAttacker is not Player ) return;

		if( (pawn as Player).team == Player.Team.Terror )
			Karma.Killed( round.Players.First(x => x.GetClientOwner().SteamId == pawn.LastAttacker.GetClientOwner().SteamId ), round.Players.First( x => x.GetClientOwner().SteamId == client.SteamId ) );
		if(pawn is Player)
			Round?.OnPlayerKilled( pawn as Player );
		base.OnKilled( client, pawn) ;
	}
	public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
	{
		Round.OnPlayerLeave( client.Pawn as Player );

		base.ClientDisconnect( client, reason );
	}
	[ServerCmd]
	public static void StopInspectingRagdoll()
	{
		Instance.Round.Players.First(x => x.GetClientOwner().SteamId == ConsoleSystem.Caller.SteamId ).CurrentlyInspectingRagdoll = null;
	}
	[ServerCmd]
	public static void ConfirmDeath()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player caller || caller.CurrentlyInspectingRagdoll is not PlayerRagdoll ragdoll || ragdoll.IsFound) return;
		ragdoll.FindRagdoll(caller);
		

		if ( caller.GetClientOwner() is not null && ragdoll.Owner.GetClientOwner() is not null )
		{
			Sandbox.UI.ChatBox.AddInformation( To.Everyone, $"{caller.GetClientOwner().Name} found {ragdoll.Owner.GetClientOwner().Name}'s dead body! He was a " + ragdoll.Role.ToString(), "https://i.imgur.com/XUxaxpB.png" );
		}

	}

	// add timeout to this!!!! ppl can just spam it :(
	// TO-DO: DetectiveCalled networked bool on PlayerRagdoll, and call only if false, else set to true and call
	[ServerCmd]
	public static void CallDetective()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player caller || caller.CurrentlyInspectingRagdoll is not PlayerRagdoll ragdoll ) return;
		
		if ( caller.GetClientOwner() is not null && ragdoll.Owner.GetClientOwner() is not null )
		{
			Sandbox.UI.ChatBox.AddInformation( To.Everyone, $"{caller.GetClientOwner().Name} has called a detective to {ragdoll.Owner.GetClientOwner().Name}'s dead body!", "https://i.imgur.com/XUxaxpB.png" );
		}
		foreach(var player in Instance.Round.Players)
		{
			if ( player.role == Player.Role.Detective )
				Radar.AddPoint( To.Single( player ), ragdoll.Position );
		}

	}

}
