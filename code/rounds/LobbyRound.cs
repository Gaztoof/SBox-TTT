using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LobbyRound : BaseRound
{
	public override string RoundName => "LOBBY";

	protected override void OnStart()
	{
		Log.Info( "Started Lobby Round" );

		if ( Host.IsServer )
		{
			var players = Client.All.Select( ( client ) => client.Pawn as Player );

			foreach ( var player in players )
			{
				OnPlayerJoin( player );
			}

			//Game.Instance.RemoveAllBalls();
		}
	}

	protected override void OnFinish()
	{
		Log.Info( "Finished Lobby Round" );
	}

	public override void OnPlayerJoin( Player player )
	{
		if ( Players.Contains( player ) || Spectators.Contains( player )  )
		{
			return;
		}

		if ( player.spectatorOnly )
		{
			player.team = Player.Team.Spectator;
			player.MakeSpectator( true, true );
			AddSpectator( player );
		}
		else
		{
			player.team = Player.Team.Terror;
			AddPlayer( player );
		}


		AddPlayer( player );

		base.OnPlayerJoin( player );
	}
	public override void OnPlayerSpawn( Player player )
	{
		base.OnPlayerSpawn( player );
		if(player.spectatorOnly)
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
