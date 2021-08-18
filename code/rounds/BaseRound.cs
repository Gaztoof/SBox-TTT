using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract partial class BaseRound : NetworkComponent
{
	public virtual int RoundDuration => 0;
	public virtual string RoundName => "";
	public virtual bool CanPlayerSuicide => true;
	public virtual bool ShowTimeLeft => false;
	public virtual bool ShowRoundInfo => false;

	public List<Player> Players { get; set; } = new();
	public List<Player> Spectators { get; set; } = new();

	public float RoundEndTime { get; set; }

	public float TimeLeft
	{
		get
		{
			return RoundEndTime - Sandbox.Time.Now;
		}
	}

	[Net] public int TimeLeftSeconds { get; set; }

	public void Start()
	{
		if ( Host.IsServer && RoundDuration > 0 )
		{
			RoundEndTime = Sandbox.Time.Now + RoundDuration;
			TimeLeftSeconds = TimeLeft.CeilToInt();
		}
		OnStart();
	}

	public void Finish()
	{
		if ( Host.IsServer )
		{
			RoundEndTime = 0f;
			Players.Clear();
			Spectators.Clear();
		}

		OnFinish();
	}

	public void AddPlayer( Player player )
	{
		Host.AssertServer();

		if ( !Players.Contains( player ) )
			Players.Add( player );
		if ( Spectators.Contains( player ) )
			Spectators.Remove( player );

	}
	public void AddSpectator( Player player )
	{
		Host.AssertServer();

		if ( !Spectators.Contains( player ) )
			Spectators.Add( player );
		if ( Players.Contains( player ) )
			Players.Remove( player );
	}
	public void RemovePlayer( Player player )
	{
		Host.AssertServer();

		Players.Remove( player );
	}
	public void RemoveSpectator( Player player )
	{
		Host.AssertServer();

		Spectators.Remove( player );
	}

	//public virtual void OnBallEnterPocket( PoolBall ball, TriggerBallPocket pocket ) { }

	//public virtual void OnBallHitOtherBall( PoolBall ball, PoolBall other ) { }

	public virtual void OnPlayerJoin( Player player ) { }
	public virtual void OnPlayerSpawn( Player player ) { }

	public virtual void OnPlayerKilled( Player player ) { }

	public virtual void OnPlayerLeave( Player player )
	{
		Players.Remove( player );
		Spectators.Remove( player );
	}

	public virtual void UpdatePlayerPosition( Player player )
	{
		var zoomOutDistance = 350f;

		player.Position = new Vector3( 0f, 0f, zoomOutDistance );
		player.Rotation = Rotation.LookAt( Vector3.Down );
	}

	public virtual void OnTick() { }

	public virtual void OnSecond()
	{
		if ( Host.IsServer )
		{
			foreach(var ply in Client.All.Select( ( client ) => client.Pawn as Player ).ToList() )
			{
				if ( ply is not Player || !ply.IsValid() ) return;

				if ( ply.GetClientOwner() is Client owner )
				{
					owner.SetScore( "karma", ply.karma );
					owner.SetScore( "score", 0 );
					owner.SetScore( "deaths", ply.Deaths );
					owner.SetScore( "ping", 69 );
					owner.SetScore( "health", ply.Health );
					owner.SetScore( "steamid", ply.GetClientOwner().SteamId );
					owner.SetScore( "role", ply.role ); // exploitable asf but idk what else to do ....
					owner.SetScore( "isdead", ply.IsDead() );
				}
				if ( ply.spectatorOnly && !ply.IsSpectator())
				{
					AddSpectator( ply );
					ply.MakeSpectator(true, true);
					ply.OnKilled();
				}
			}

			if ( RoundEndTime > 0 && Sandbox.Time.Now >= RoundEndTime )
			{
				RoundEndTime = 0f;
				OnTimeUp();
			}
			else
			{
				TimeLeftSeconds = TimeLeft.CeilToInt();
			}
		}
	}

	protected virtual void OnStart() { }

	protected virtual void OnFinish() { }

	protected virtual void OnTimeUp() { }
}
